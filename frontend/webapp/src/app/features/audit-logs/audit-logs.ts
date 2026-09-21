import { DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { DataTable, DataTableColumn } from '../../shared/components/data-table/data-table';
import { Pagination } from '../../shared/components/pagination/pagination';
import { FilterBar } from '../../shared/components/filter-bar/filter-bar';
import { AuditLogService } from '../../core/services/audit-log.service';
import { AuditLog } from '../../core/models/audit-log.models';

const PAGE_SIZE = 20;

@Component({
  selector: 'app-audit-logs',
  standalone: true,
  imports: [DataTable, Pagination, FilterBar, DatePipe],
  templateUrl: './audit-logs.html',
  styleUrls: ['../destinations/destinations.scss', './audit-logs.scss']
})
export class AuditLogs {
  private readonly auditLogService = inject(AuditLogService);

  readonly columns: DataTableColumn[] = [
    { key: 'actor', label: 'Actor' },
    { key: 'action', label: 'Action' },
    { key: 'entity', label: 'Entity' },
    { key: 'change', label: 'Old → New' },
    { key: 'date', label: 'Date' }
  ];

  readonly logs = signal<AuditLog[]>([]);
  readonly loading = signal(false);
  readonly loadError = signal<string | null>(null);
  readonly pageNumber = signal(1);
  readonly totalCount = signal(0);
  readonly actionFilter = signal('');
  readonly entityNameFilter = signal('');
  readonly fromDate = signal('');
  readonly toDate = signal('');

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.loadError.set(null);
    this.auditLogService
      .search({
        pageNumber: this.pageNumber(),
        pageSize: PAGE_SIZE,
        action: this.actionFilter() || undefined,
        entityName: this.entityNameFilter() || undefined,
        fromDate: this.fromDate() || undefined,
        toDate: this.toDate() || undefined
      })
      .subscribe({
        next: (response) => {
          this.loading.set(false);
          if (response.success && response.data) {
            this.logs.set(response.data.items);
            this.totalCount.set(response.data.totalCount);
          } else {
            this.logs.set([]);
            this.totalCount.set(0);
            this.loadError.set(response.message || 'Could not load audit logs. Please try again.');
          }
        },
        error: (error) => {
          this.loading.set(false);
          this.loadError.set(error?.error?.message ?? 'Could not load audit logs. Please try again.');
        }
      });
  }

  onActionFilterChange(term: string): void {
    this.actionFilter.set(term);
    this.pageNumber.set(1);
    this.load();
  }

  onEntityNameFilterChange(value: string): void {
    this.entityNameFilter.set(value.trim());
    this.pageNumber.set(1);
    this.load();
  }

  onFromDateChange(value: string): void {
    this.fromDate.set(value);
    this.pageNumber.set(1);
    this.load();
  }

  onToDateChange(value: string): void {
    this.toDate.set(value);
    this.pageNumber.set(1);
    this.load();
  }

  onPageChange(page: number): void {
    this.pageNumber.set(page);
    this.load();
  }

  /** "EntityName #EntityId", or just EntityName when there's no id — truncated in the template
   * with this same string as the title so the full value is still available on hover. */
  entityLabel(log: AuditLog): string {
    return log.entityId ? `${log.entityName} #${log.entityId}` : log.entityName;
  }
}
