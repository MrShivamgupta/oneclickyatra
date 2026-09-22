import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, inject, signal, computed } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { DashboardService } from '../../core/services/dashboard.service';
import { FollowUpService } from '../../core/services/followup.service';
import { ChartCanvas } from '../../shared/components/chart-canvas/chart-canvas';
import { Icon, IconName } from '../../shared/components/icon/icon';
import { Spinner } from '../../shared/components/spinner/spinner';
import {
  DATE_PRESETS,
  DATE_PRESET_LABELS,
  DashboardAlert,
  DashboardKpis,
  DatePreset,
  DestinationPerformance,
  LeadFunnelStage,
  RevenueTrendPoint,
  SalesPerformance
} from '../../core/models/dashboard.models';
import { Lead } from '../../core/models/crm.models';
import { Booking } from '../../core/models/booking.models';
import { EnquiryResponse } from '../../core/models/enquiry.models';
import { FollowUp } from '../../core/models/crm.models';
import type { ChartData } from 'chart.js';

const LEAD_STAGE_ORDER = ['New', 'Contacted', 'QuotationSent', 'Negotiation', 'Confirmed', 'Lost'];

interface KpiCard {
  label: string;
  value: number;
  format: 'number' | 'currency' | 'percent';
  link?: string;
  /** 'blue'/'green'/'amber'/'purple' are the bold, solid-fill headline treatment (white text).
   * The '-light' variants are a lighter tint used on the remaining secondary KPIs, per direct
   * follow-up request to color every card, not just the 4 headline ones -- reuses the same hue
   * tokens at a light background/dark-icon tint instead of inventing a second palette. */
  accent?: 'blue' | 'green' | 'amber' | 'purple' | 'blue-light' | 'green-light' | 'amber-light' | 'purple-light' | 'teal-light' | 'danger-light';
  icon?: IconName;
}

function toIsoDate(date: Date): string {
  return date.toISOString().slice(0, 10);
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterLink, DatePipe, DecimalPipe, ChartCanvas, Icon, Spinner],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss'
})
export class Dashboard {
  protected readonly authService = inject(AuthService);
  private readonly dashboardService = inject(DashboardService);
  private readonly followUpService = inject(FollowUpService);
  private readonly router = inject(Router);

  readonly presets = DATE_PRESETS;
  readonly presetLabels = DATE_PRESET_LABELS;

  readonly selectedPreset = signal<DatePreset>('Last30Days');
  readonly customFrom = signal(toIsoDate(new Date()));
  readonly customTo = signal(toIsoDate(new Date()));
  readonly quickActionsOpen = signal(false);

  readonly loading = signal(false);
  readonly loadError = signal<string | null>(null);
  readonly kpis = signal<DashboardKpis | null>(null);
  readonly alerts = signal<DashboardAlert[]>([]);
  readonly revenueTrend = signal<RevenueTrendPoint[]>([]);
  readonly leadFunnel = signal<LeadFunnelStage[]>([]);
  readonly destinationPerformance = signal<DestinationPerformance[]>([]);
  readonly salesPerformance = signal<SalesPerformance[]>([]);

  readonly recentLeads = signal<Lead[]>([]);
  readonly recentBookings = signal<Booking[]>([]);
  readonly pendingEnquiries = signal<EnquiryResponse[]>([]);
  readonly refundRequests = signal<Booking[]>([]);
  readonly upcomingDepartures = signal<Booking[]>([]);
  readonly todayFollowUps = signal<FollowUp[]>([]);
  readonly todayFollowUpCount = signal(0);

  readonly range = computed(() => this.computeRange(this.selectedPreset(), this.customFrom(), this.customTo()));

  readonly kpiCards = computed<KpiCard[]>(() => {
    const k = this.kpis();
    if (!k) return [];
    return [
      { label: 'Total Leads', value: k.totalLeads, format: 'number', link: '/admin/leads', accent: 'blue', icon: 'users' },
      {
        label: 'Confirmed Bookings',
        value: k.confirmedBookings,
        format: 'number',
        link: '/admin/bookings',
        accent: 'green',
        icon: 'check-circle'
      },
      {
        label: 'Revenue (period)',
        value: k.periodRevenue,
        format: 'currency',
        link: '/admin/bookings',
        accent: 'amber',
        icon: 'currency'
      },
      {
        label: 'Open Enquiries',
        value: k.openEnquiries,
        format: 'number',
        link: '/admin/enquiries',
        accent: 'purple',
        icon: 'inbox'
      },
      {
        label: "Today's Follow-Ups",
        value: this.todayFollowUpCount(),
        format: 'number',
        link: '/admin/followups',
        accent: 'teal-light',
        icon: 'phone'
      },
      {
        label: 'Active Quotations',
        value: k.activeQuotations,
        format: 'number',
        link: '/admin/quotations',
        accent: 'blue-light',
        icon: 'file-text'
      },
      {
        label: 'Pending Payments',
        value: k.pendingPayments,
        format: 'currency',
        link: '/admin/bookings',
        accent: 'amber-light',
        icon: 'credit-card'
      },
      {
        label: 'Upcoming Departures',
        value: k.upcomingDepartures,
        format: 'number',
        link: '/admin/bookings',
        accent: 'purple-light',
        icon: 'calendar-check'
      },
      {
        label: 'Total Bookings',
        value: k.totalBookings,
        format: 'number',
        link: '/admin/bookings',
        accent: 'green-light',
        icon: 'package'
      },
      {
        label: 'Cancellations',
        value: k.cancellations,
        format: 'number',
        link: '/admin/bookings',
        accent: 'danger-light',
        icon: 'x-circle'
      },
      {
        label: 'Conversion Rate',
        value: k.conversionRate,
        format: 'percent',
        link: '/admin/leads',
        accent: 'teal-light',
        icon: 'bar-chart'
      }
    ];
  });

  readonly revenueChartData = computed<ChartData>(() => ({
    labels: this.revenueTrend().map((p) => p.trendDate),
    datasets: [
      {
        label: 'Revenue',
        data: this.revenueTrend().map((p) => p.revenue),
        borderColor: '#1273d6',
        backgroundColor: 'rgba(18, 115, 214, 0.12)',
        fill: true,
        tension: 0.35,
        borderWidth: 2,
        pointRadius: 3,
        pointHoverRadius: 5,
        pointBackgroundColor: '#1273d6',
        pointBorderColor: '#ffffff',
        pointBorderWidth: 1.5
      }
    ]
  }));

  readonly leadFunnelChartData = computed<ChartData>(() => {
    const byStatus = new Map(this.leadFunnel().map((s) => [s.status, s.leadCount]));
    const orderedStages = LEAD_STAGE_ORDER.filter((s) => s !== 'Lost');
    return {
      labels: [...orderedStages, 'Lost'],
      datasets: [
        {
          label: 'Leads',
          data: [...orderedStages.map((s) => byStatus.get(s) ?? 0), byStatus.get('Lost') ?? 0],
          backgroundColor: [...orderedStages.map(() => '#1273d6'), '#e0453f'],
          borderRadius: 6,
          maxBarThickness: 22
        }
      ]
    };
  });

  // "Revenue by Destination" is a share-of-total question, which a donut communicates more directly
  // than a bar chart -- a fixed 6-color palette (reused from the app's own token colors) cycles per
  // slice via modulo, so this doesn't break if a period ever has more than 6 destinations.
  private static readonly DonutPalette = ['#1273d6', '#147a51', '#e6a417', '#6b4fe0', '#e0453f', '#0e90a8'];

  readonly destinationChartData = computed<ChartData>(() => ({
    labels: this.destinationPerformance().map((d) => d.destinationName),
    datasets: [
      {
        label: 'Revenue',
        data: this.destinationPerformance().map((d) => d.revenue),
        backgroundColor: this.destinationPerformance().map((_, i) => Dashboard.DonutPalette[i % Dashboard.DonutPalette.length]),
        borderWidth: 2,
        borderColor: '#ffffff',
        hoverOffset: 6
      }
    ]
  }));

  readonly destinationChartOptions = {
    plugins: { legend: { display: true, position: 'right' as const, labels: { boxWidth: 10, font: { size: 11 } } } },
    cutout: '65%'
  };

  readonly salesChartData = computed<ChartData>(() => ({
    labels: this.salesPerformance().map((s) => s.staffName),
    datasets: [
      {
        label: 'Revenue',
        data: this.salesPerformance().map((s) => s.revenue),
        backgroundColor: '#6b4fe0',
        borderRadius: 6,
        maxBarThickness: 32
      }
    ]
  }));

  /** Shared, minimal Chart.js theming: no redundant per-series legend (the section already has an
   * h2 title), softer gridlines than the Chart.js default, and no vertical gridlines on bar/line
   * x-axes -- reduces visual noise without hiding any data. */
  readonly baseChartOptions = {
    plugins: { legend: { display: false } },
    scales: {
      x: { grid: { display: false } },
      y: { grid: { color: 'rgba(15, 28, 63, 0.06)' }, beginAtZero: true }
    }
  };

  readonly leadFunnelChartOptions = {
    indexAxis: 'y' as const,
    plugins: { legend: { display: false } },
    scales: {
      x: { grid: { color: 'rgba(15, 28, 63, 0.06)' }, beginAtZero: true },
      y: { grid: { display: false } }
    }
  };

  constructor() {
    this.loadAll();
  }

  private computeRange(preset: DatePreset, customFrom: string, customTo: string): { fromDate: string; toDate: string } {
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    switch (preset) {
      case 'Today':
        return { fromDate: toIsoDate(today), toDate: toIsoDate(today) };
      case 'Yesterday': {
        const yesterday = new Date(today);
        yesterday.setDate(yesterday.getDate() - 1);
        return { fromDate: toIsoDate(yesterday), toDate: toIsoDate(yesterday) };
      }
      case 'Last7Days': {
        const from = new Date(today);
        from.setDate(from.getDate() - 6);
        return { fromDate: toIsoDate(from), toDate: toIsoDate(today) };
      }
      case 'Last30Days': {
        const from = new Date(today);
        from.setDate(from.getDate() - 29);
        return { fromDate: toIsoDate(from), toDate: toIsoDate(today) };
      }
      case 'ThisMonth': {
        const from = new Date(today.getFullYear(), today.getMonth(), 1);
        return { fromDate: toIsoDate(from), toDate: toIsoDate(today) };
      }
      case 'LastMonth': {
        const from = new Date(today.getFullYear(), today.getMonth() - 1, 1);
        const to = new Date(today.getFullYear(), today.getMonth(), 0);
        return { fromDate: toIsoDate(from), toDate: toIsoDate(to) };
      }
      case 'Custom':
        return { fromDate: customFrom, toDate: customTo };
    }
  }

  onPresetChange(preset: DatePreset): void {
    this.selectedPreset.set(preset);
    if (preset !== 'Custom') {
      this.loadAll();
    }
  }

  applyCustomRange(): void {
    this.loadAll();
  }

  toggleQuickActions(): void {
    this.quickActionsOpen.update((open) => !open);
  }

  navigateQuickAction(path: string): void {
    this.quickActionsOpen.set(false);
    this.router.navigate([path]);
  }

  loadAll(): void {
    this.loading.set(true);
    this.loadError.set(null);
    const range = this.range();

    const onLoadError = (error: { error?: { message?: string } }) => {
      this.loading.set(false);
      this.loadError.set(error?.error?.message ?? 'Could not load the dashboard. Please try again.');
    };

    this.dashboardService.getStats(range).subscribe({
      next: (r) => r.success && r.data && this.kpis.set(r.data),
      error: onLoadError
    });
    this.dashboardService.getAlerts().subscribe({
      next: (r) => r.success && r.data && this.alerts.set(r.data),
      error: onLoadError
    });
    this.dashboardService.getRevenueChart(range).subscribe({
      next: (r) => r.success && r.data && this.revenueTrend.set(r.data),
      error: onLoadError
    });
    this.dashboardService.getLeadFunnel(range).subscribe({
      next: (r) => r.success && r.data && this.leadFunnel.set(r.data),
      error: onLoadError
    });
    this.dashboardService.getDestinationPerformance(range).subscribe({
      next: (r) => r.success && r.data && this.destinationPerformance.set(r.data),
      error: onLoadError
    });
    this.dashboardService.getSalesPerformance(range).subscribe({
      next: (r) => r.success && r.data && this.salesPerformance.set(r.data),
      error: onLoadError
    });

    this.dashboardService.getRecentLeads().subscribe({
      next: (r) => r.success && r.data && this.recentLeads.set(r.data),
      error: onLoadError
    });
    this.dashboardService.getRecentBookings().subscribe({
      next: (r) => r.success && r.data && this.recentBookings.set(r.data),
      error: onLoadError
    });
    this.dashboardService.getPendingEnquiries().subscribe({
      next: (r) => r.success && r.data && this.pendingEnquiries.set(r.data),
      error: onLoadError
    });
    this.dashboardService.getRefundRequests().subscribe({
      next: (r) => r.success && r.data && this.refundRequests.set(r.data),
      error: onLoadError
    });
    this.dashboardService.getUpcomingDepartures().subscribe({
      next: (r) => r.success && r.data && this.upcomingDepartures.set(r.data),
      error: onLoadError
    });

    this.followUpService.today({ pageNumber: 1, pageSize: 8 }).subscribe({
      next: (r) => {
        if (r.success && r.data) {
          this.todayFollowUpCount.set(r.data.totalCount);
          this.todayFollowUps.set(r.data.items);
        }
        this.loading.set(false);
      },
      error: onLoadError
    });
  }
}
