import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { DataTable, DataTableColumn } from '../../shared/components/data-table/data-table';
import { Pagination } from '../../shared/components/pagination/pagination';
import { FilterBar } from '../../shared/components/filter-bar/filter-bar';
import { PaymentService } from '../../core/services/payment.service';
import { PAYMENT_STATUSES, Payment } from '../../core/models/payment.models';

const PAGE_SIZE = 10;

@Component({
  selector: 'app-payments',
  standalone: true,
  imports: [DataTable, Pagination, FilterBar, DatePipe, DecimalPipe],
  templateUrl: './payments.html',
  styleUrls: ['../destinations/destinations.scss', '../leads/leads-list/leads-list.scss', './payments.scss']
})
export class Payments {
  private readonly paymentService = inject(PaymentService);
  private readonly router = inject(Router);

  readonly statuses = PAYMENT_STATUSES;

  readonly columns: DataTableColumn[] = [
    { key: 'booking', label: 'Booking' },
    { key: 'customer', label: 'Customer' },
    { key: 'amount', label: 'Amount' },
    { key: 'status', label: 'Status' },
    { key: 'gateway', label: 'Gateway Order' },
    { key: 'createdAt', label: 'Created' }
  ];

  readonly payments = signal<Payment[]>([]);
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
    this.paymentService
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
            this.payments.set(response.data.items);
            this.totalCount.set(response.data.totalCount);
          } else {
            this.payments.set([]);
            this.totalCount.set(0);
            this.loadError.set(response.message || 'Could not load payments. Please try again.');
          }
        },
        error: (error) => {
          this.loading.set(false);
          this.loadError.set(error?.error?.message ?? 'Could not load payments. Please try again.');
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

  openBooking(payment: Payment): void {
    this.router.navigate(['/admin/bookings', payment.bookingId]);
  }
}
