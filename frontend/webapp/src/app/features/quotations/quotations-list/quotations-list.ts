import { DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { DataTable, DataTableColumn } from '../../../shared/components/data-table/data-table';
import { Pagination } from '../../../shared/components/pagination/pagination';
import { FilterBar } from '../../../shared/components/filter-bar/filter-bar';
import { QuotationService } from '../../../core/services/quotation.service';
import { QUOTATION_STATUSES, Quotation } from '../../../core/models/quotation.models';

const PAGE_SIZE = 10;

@Component({
  selector: 'app-quotations-list',
  standalone: true,
  imports: [DataTable, Pagination, FilterBar, DatePipe],
  templateUrl: './quotations-list.html',
  styleUrls: ['../../destinations/destinations.scss', '../../leads/leads-list/leads-list.scss', './quotations-list.scss']
})
export class QuotationsList {
  private readonly quotationService = inject(QuotationService);
  private readonly router = inject(Router);

  readonly statuses = QUOTATION_STATUSES;

  readonly columns: DataTableColumn[] = [
    { key: 'number', label: 'Quotation #' },
    { key: 'lead', label: 'Lead' },
    { key: 'title', label: 'Title' },
    { key: 'status', label: 'Status' },
    { key: 'validUntil', label: 'Valid Until' },
    { key: 'actions', label: '' }
  ];

  readonly quotations = signal<Quotation[]>([]);
  readonly loading = signal(false);
  readonly loadError = signal<string | null>(null);
  readonly pageNumber = signal(1);
  readonly totalCount = signal(0);
  readonly searchTerm = signal('');
  readonly statusFilter = signal('');

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.loadError.set(null);
    this.quotationService
      .search({
        pageNumber: this.pageNumber(),
        pageSize: PAGE_SIZE,
        searchTerm: this.searchTerm() || undefined,
        status: this.statusFilter() || undefined
      })
      .subscribe({
        next: (response) => {
          this.loading.set(false);
          if (response.success && response.data) {
            this.quotations.set(response.data.items);
            this.totalCount.set(response.data.totalCount);
          }
        },
        error: (error) => {
          this.loading.set(false);
          this.loadError.set(error?.error?.message ?? 'Could not load quotations. Please try again.');
        }
      });
  }

  onSearchChange(term: string): void {
    this.searchTerm.set(term);
    this.pageNumber.set(1);
    this.load();
  }

  onStatusFilterChange(status: string): void {
    this.statusFilter.set(status);
    this.pageNumber.set(1);
    this.load();
  }

  onPageChange(page: number): void {
    this.pageNumber.set(page);
    this.load();
  }

  createQuotation(): void {
    this.router.navigate(['/admin/quotations/new']);
  }

  openQuotation(quotation: Quotation): void {
    this.router.navigate(['/admin/quotations', quotation.id]);
  }
}
