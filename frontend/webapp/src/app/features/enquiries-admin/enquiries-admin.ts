import { DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DataTable, DataTableColumn } from '../../shared/components/data-table/data-table';
import { Pagination } from '../../shared/components/pagination/pagination';
import { FilterBar } from '../../shared/components/filter-bar/filter-bar';
import { ConfirmationDialog } from '../../shared/components/confirmation-dialog/confirmation-dialog';
import { EnquiryService } from '../../core/services/enquiry.service';
import { ENQUIRY_STATUSES, EnquiryResponse } from '../../core/models/enquiry.models';

const PAGE_SIZE = 10;

@Component({
  selector: 'app-enquiries-admin',
  standalone: true,
  imports: [DataTable, Pagination, FilterBar, ConfirmationDialog, DatePipe, FormsModule],
  templateUrl: './enquiries-admin.html',
  styleUrls: ['../destinations/destinations.scss', '../leads/leads-list/leads-list.scss', './enquiries-admin.scss']
})
export class EnquiriesAdmin {
  private readonly enquiryService = inject(EnquiryService);

  readonly statuses = ENQUIRY_STATUSES;

  readonly columns: DataTableColumn[] = [
    { key: 'name', label: 'Name' },
    { key: 'destination', label: 'Destination' },
    { key: 'message', label: 'Message' },
    { key: 'status', label: 'Status' },
    { key: 'receivedAt', label: 'Received' },
    { key: 'actions', label: '' }
  ];

  readonly enquiries = signal<EnquiryResponse[]>([]);
  readonly loading = signal(false);
  readonly loadError = signal<string | null>(null);
  readonly pageNumber = signal(1);
  readonly totalCount = signal(0);
  readonly searchTerm = signal('');
  readonly statusFilter = signal('');
  readonly deleteTarget = signal<EnquiryResponse | null>(null);
  readonly deleting = signal(false);
  readonly deleteError = signal<string | null>(null);
  readonly statusUpdatingId = signal<string | null>(null);
  readonly statusError = signal<string | null>(null);

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.loadError.set(null);
    this.enquiryService
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
            this.enquiries.set(response.data.items);
            this.totalCount.set(response.data.totalCount);
          }
        },
        error: (error) => {
          this.loading.set(false);
          this.loadError.set(error?.error?.message ?? 'Could not load enquiries. Please try again.');
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

  updateStatus(enquiry: EnquiryResponse, status: string): void {
    this.statusUpdatingId.set(enquiry.id);
    this.statusError.set(null);
    this.enquiryService.updateStatus(enquiry.id, status).subscribe({
      next: () => {
        this.statusUpdatingId.set(null);
        this.load();
      },
      error: (error) => {
        this.statusUpdatingId.set(null);
        this.statusError.set(error?.error?.message ?? 'Could not update this enquiry\'s status. Please try again.');
      }
    });
  }

  confirmDelete(enquiry: EnquiryResponse): void {
    this.deleteError.set(null);
    this.deleteTarget.set(enquiry);
  }

  cancelDelete(): void {
    this.deleteTarget.set(null);
  }

  performDelete(): void {
    const target = this.deleteTarget();
    if (!target) return;
    this.deleting.set(true);
    this.deleteError.set(null);
    this.enquiryService.delete(target.id).subscribe({
      next: () => {
        this.deleting.set(false);
        this.deleteTarget.set(null);
        this.load();
      },
      error: (error) => {
        this.deleting.set(false);
        this.deleteError.set(error?.error?.message ?? 'Could not delete this enquiry. Please try again.');
      }
    });
  }
}
