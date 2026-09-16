import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { DataTable, DataTableColumn } from '../../../shared/components/data-table/data-table';
import { Pagination } from '../../../shared/components/pagination/pagination';
import { FilterBar } from '../../../shared/components/filter-bar/filter-bar';
import { BookingService } from '../../../core/services/booking.service';
import { BOOKING_STATUSES, Booking } from '../../../core/models/booking.models';

const PAGE_SIZE = 10;

@Component({
  selector: 'app-bookings-list',
  standalone: true,
  imports: [DataTable, Pagination, FilterBar, DatePipe, DecimalPipe],
  templateUrl: './bookings-list.html',
  styleUrls: ['../../destinations/destinations.scss', '../../leads/leads-list/leads-list.scss', '../../quotations/quotations-list/quotations-list.scss', './bookings-list.scss']
})
export class BookingsList {
  private readonly bookingService = inject(BookingService);
  private readonly router = inject(Router);

  readonly statuses = BOOKING_STATUSES;

  readonly columns: DataTableColumn[] = [
    { key: 'number', label: 'Booking #' },
    { key: 'customer', label: 'Customer' },
    { key: 'travelDate', label: 'Travel Date' },
    { key: 'total', label: 'Total' },
    { key: 'status', label: 'Status' },
    { key: 'actions', label: '' }
  ];

  readonly bookings = signal<Booking[]>([]);
  readonly loading = signal(false);
  readonly pageNumber = signal(1);
  readonly totalCount = signal(0);
  readonly searchTerm = signal('');
  readonly statusFilter = signal('');

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.bookingService
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
            this.bookings.set(response.data.items);
            this.totalCount.set(response.data.totalCount);
          }
        },
        error: () => this.loading.set(false)
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

  createBooking(): void {
    this.router.navigate(['/admin/bookings/new']);
  }

  openBooking(booking: Booking): void {
    this.router.navigate(['/admin/bookings', booking.id]);
  }
}
