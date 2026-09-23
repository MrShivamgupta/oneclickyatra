import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { PortalService } from '../../../core/services/portal.service';
import { Booking } from '../../../core/models/booking.models';
import { Spinner } from '../../../shared/components/spinner/spinner';

const PAGE_SIZE = 10;

@Component({
  selector: 'app-my-bookings',
  standalone: true,
  imports: [DatePipe, DecimalPipe, Spinner],
  templateUrl: './my-bookings.html',
  styleUrl: '../portal-shared.scss'
})
export class MyBookings {
  private readonly portalService = inject(PortalService);
  private readonly router = inject(Router);

  readonly bookings = signal<Booking[]>([]);
  readonly loading = signal(true);
  readonly loadError = signal<string | null>(null);
  readonly pageNumber = signal(1);
  readonly totalCount = signal(0);

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.loadError.set(null);
    this.portalService.getBookings({ pageNumber: this.pageNumber(), pageSize: PAGE_SIZE }).subscribe({
      next: (response) => {
        this.loading.set(false);
        if (response.success && response.data) {
          this.bookings.set(response.data.items);
          this.totalCount.set(response.data.totalCount);
        }
      },
      error: (error) => {
        this.loading.set(false);
        this.loadError.set(error?.error?.message ?? 'Could not load your bookings. Please try again.');
      }
    });
  }

  onPageChange(page: number): void {
    this.pageNumber.set(page);
    this.load();
  }

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.totalCount() / PAGE_SIZE));
  }

  openBooking(booking: Booking): void {
    this.router.navigate(['/portal/bookings', booking.id]);
  }
}
