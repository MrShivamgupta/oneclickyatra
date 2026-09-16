import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, inject, signal, computed } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { DashboardService } from '../../core/services/dashboard.service';
import { FollowUpService } from '../../core/services/followup.service';
import { ChartCanvas } from '../../shared/components/chart-canvas/chart-canvas';
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
}

function toIsoDate(date: Date): string {
  return date.toISOString().slice(0, 10);
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterLink, DatePipe, DecimalPipe, ChartCanvas],
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
      { label: 'Total Leads', value: k.totalLeads, format: 'number', link: '/admin/leads' },
      { label: "Today's Follow-Ups", value: this.todayFollowUpCount(), format: 'number', link: '/admin/followups' },
      { label: 'Active Quotations', value: k.activeQuotations, format: 'number', link: '/admin/quotations' },
      { label: 'Confirmed Bookings', value: k.confirmedBookings, format: 'number', link: '/admin/bookings' },
      { label: 'Revenue (period)', value: k.periodRevenue, format: 'currency', link: '/admin/bookings' },
      { label: 'Pending Payments', value: k.pendingPayments, format: 'currency', link: '/admin/bookings' },
      { label: 'Upcoming Departures', value: k.upcomingDepartures, format: 'number', link: '/admin/bookings' },
      { label: 'Total Bookings', value: k.totalBookings, format: 'number', link: '/admin/bookings' },
      { label: 'Cancellations', value: k.cancellations, format: 'number', link: '/admin/bookings' },
      { label: 'Open Enquiries', value: k.openEnquiries, format: 'number', link: '/admin/enquiries' },
      { label: 'Conversion Rate', value: k.conversionRate, format: 'percent', link: '/admin/leads' }
    ];
  });

  readonly revenueChartData = computed<ChartData>(() => ({
    labels: this.revenueTrend().map((p) => p.trendDate),
    datasets: [
      {
        label: 'Revenue',
        data: this.revenueTrend().map((p) => p.revenue),
        borderColor: '#2f6fed',
        backgroundColor: 'rgba(47, 111, 237, 0.15)',
        fill: true,
        tension: 0.3
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
          backgroundColor: [...orderedStages.map(() => '#2f6fed'), '#e0453f']
        }
      ]
    };
  });

  readonly destinationChartData = computed<ChartData>(() => ({
    labels: this.destinationPerformance().map((d) => d.destinationName),
    datasets: [{ label: 'Revenue', data: this.destinationPerformance().map((d) => d.revenue), backgroundColor: '#1fa971' }]
  }));

  readonly salesChartData = computed<ChartData>(() => ({
    labels: this.salesPerformance().map((s) => s.staffName),
    datasets: [{ label: 'Revenue', data: this.salesPerformance().map((s) => s.revenue), backgroundColor: '#e6a417' }]
  }));

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
    const range = this.range();

    this.dashboardService.getStats(range).subscribe((r) => r.success && r.data && this.kpis.set(r.data));
    this.dashboardService.getAlerts().subscribe((r) => r.success && r.data && this.alerts.set(r.data));
    this.dashboardService.getRevenueChart(range).subscribe((r) => r.success && r.data && this.revenueTrend.set(r.data));
    this.dashboardService.getLeadFunnel(range).subscribe((r) => r.success && r.data && this.leadFunnel.set(r.data));
    this.dashboardService.getDestinationPerformance(range).subscribe((r) => r.success && r.data && this.destinationPerformance.set(r.data));
    this.dashboardService.getSalesPerformance(range).subscribe((r) => r.success && r.data && this.salesPerformance.set(r.data));

    this.dashboardService.getRecentLeads().subscribe((r) => r.success && r.data && this.recentLeads.set(r.data));
    this.dashboardService.getRecentBookings().subscribe((r) => r.success && r.data && this.recentBookings.set(r.data));
    this.dashboardService.getPendingEnquiries().subscribe((r) => r.success && r.data && this.pendingEnquiries.set(r.data));
    this.dashboardService.getRefundRequests().subscribe((r) => r.success && r.data && this.refundRequests.set(r.data));
    this.dashboardService.getUpcomingDepartures().subscribe((r) => r.success && r.data && this.upcomingDepartures.set(r.data));

    this.followUpService.today({ pageNumber: 1, pageSize: 8 }).subscribe((r) => {
      if (r.success && r.data) {
        this.todayFollowUpCount.set(r.data.totalCount);
        this.todayFollowUps.set(r.data.items);
      }
      this.loading.set(false);
    });
  }
}
