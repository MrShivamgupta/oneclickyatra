import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ReportService } from '../../../core/services/report.service';
import { EXPORT_FORMATS, ExportFormat, REPORT_DEFINITIONS, REPORT_NAMES, ReportDefinition, ReportGroupBy, ReportName, ReportRow } from '../../../core/models/report.models';
import { Spinner } from '../../../shared/components/spinner/spinner';

// Same preset set and computeRange logic as features/dashboard/dashboard.ts, copied rather than
// re-derived so both places compute date ranges identically.
const DATE_PRESETS = ['Today', 'Yesterday', 'Last7Days', 'Last30Days', 'ThisMonth', 'LastMonth', 'Custom'] as const;
type DatePreset = (typeof DATE_PRESETS)[number];

const DATE_PRESET_LABELS: Record<DatePreset, string> = {
  Today: 'Today',
  Yesterday: 'Yesterday',
  Last7Days: 'Last 7 Days',
  Last30Days: 'Last 30 Days',
  ThisMonth: 'This Month',
  LastMonth: 'Last Month',
  Custom: 'Custom Range'
};

function toIsoDate(date: Date): string {
  return date.toISOString().slice(0, 10);
}

const FALLBACK_DEFINITION: ReportDefinition = {
  name: 'sales',
  label: 'Report',
  description: '',
  hasDateRange: true,
  hasGroupBy: false
};

interface ReportQueryParams {
  fromDate?: string;
  toDate?: string;
  groupBy?: ReportGroupBy;
}

@Component({
  selector: 'app-report-viewer',
  standalone: true,
  imports: [RouterLink, Spinner],
  templateUrl: './report-viewer.html',
  styleUrl: './report-viewer.scss'
})
export class ReportViewer {
  private readonly reportService = inject(ReportService);
  private readonly route = inject(ActivatedRoute);

  readonly presets = DATE_PRESETS;
  readonly presetLabels = DATE_PRESET_LABELS;
  readonly exportFormats: ExportFormat[] = EXPORT_FORMATS;

  readonly reportName = signal<ReportName>('sales');
  readonly definition = computed<ReportDefinition>(() => REPORT_DEFINITIONS.find((d) => d.name === this.reportName()) ?? FALLBACK_DEFINITION);

  readonly selectedPreset = signal<DatePreset>('Last30Days');
  readonly customFrom = signal(toIsoDate(new Date()));
  readonly customTo = signal(toIsoDate(new Date()));
  readonly groupBy = signal<ReportGroupBy>('day');

  readonly rows = signal<ReportRow[]>([]);
  readonly loading = signal(false);
  readonly loadError = signal<string | null>(null);
  readonly exportingFormat = signal<ExportFormat | null>(null);
  readonly exportError = signal<string | null>(null);

  readonly columns = computed(() => (this.rows().length > 0 ? Object.keys(this.rows()[0]) : []));
  readonly range = computed(() => this.computeRange(this.selectedPreset(), this.customFrom(), this.customTo()));

  constructor() {
    this.route.paramMap.subscribe((params) => {
      const rawName = params.get('reportName');
      const name: ReportName = (REPORT_NAMES as readonly string[]).includes(rawName ?? '') ? (rawName as ReportName) : 'sales';
      this.reportName.set(name);
      this.groupBy.set(name === 'monthly-growth' ? 'month' : 'day');
      this.load();
    });
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

  private buildParams(): ReportQueryParams {
    const def = this.definition();
    const params: ReportQueryParams = {};
    if (def.hasDateRange) {
      const range = this.range();
      params.fromDate = range.fromDate;
      params.toDate = range.toDate;
    }
    if (def.hasGroupBy) {
      params.groupBy = this.groupBy();
    }
    return params;
  }

  onPresetChange(preset: DatePreset): void {
    this.selectedPreset.set(preset);
    if (preset !== 'Custom') {
      this.load();
    }
  }

  applyCustomRange(): void {
    this.load();
  }

  onGroupByChange(value: ReportGroupBy): void {
    this.groupBy.set(value);
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.loadError.set(null);

    this.reportService.getReport(this.reportName(), this.buildParams()).subscribe({
      next: (response) => {
        this.loading.set(false);
        if (response.success && response.data) {
          this.rows.set(response.data);
        } else {
          this.rows.set([]);
          this.loadError.set(response.message || 'Could not load this report.');
        }
      },
      error: (error) => {
        this.loading.set(false);
        this.rows.set([]);
        this.loadError.set(error?.error?.message ?? 'Could not load this report.');
      }
    });
  }

  downloadExport(format: ExportFormat): void {
    this.exportingFormat.set(format);
    this.exportError.set(null);

    this.reportService.downloadExport(this.reportName(), format, this.buildParams()).subscribe({
      next: (blob) => {
        this.exportingFormat.set(null);
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `${this.reportName()}.${format}`;
        link.click();
        URL.revokeObjectURL(url);
      },
      error: () => {
        this.exportingFormat.set(null);
        this.exportError.set('Could not export this report. Please try again.');
      }
    });
  }

  columnLabel(key: string): string {
    return key.replace(/([a-z0-9])([A-Z])/g, '$1 $2').replace(/^./, (c) => c.toUpperCase());
  }

  formatCell(value: string | number | boolean | null): string {
    if (value === null || value === undefined || value === '') {
      return '—';
    }
    if (typeof value === 'boolean') {
      return value ? 'Yes' : 'No';
    }
    if (typeof value === 'number') {
      return new Intl.NumberFormat('en-IN', { maximumFractionDigits: 2 }).format(value);
    }
    if (/^\d{4}-\d{2}-\d{2}(T\d{2}:\d{2}:\d{2})?/.test(value)) {
      const parsed = new Date(value);
      if (!isNaN(parsed.getTime())) {
        return parsed.toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' });
      }
    }
    return value;
  }
}
