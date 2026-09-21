import { DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { VendorPortalService } from '../../../core/services/vendor-portal.service';
import { VendorBookingRequestItem } from '../../../core/models/vendor.models';

const PAGE_SIZE = 10;

@Component({
  selector: 'app-booking-requests',
  standalone: true,
  imports: [DatePipe],
  templateUrl: './booking-requests.html',
  styleUrl: '../vendor-portal-shared.scss'
})
export class BookingRequests {
  private readonly vendorPortalService = inject(VendorPortalService);

  readonly requests = signal<VendorBookingRequestItem[]>([]);
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
    this.vendorPortalService.getBookingRequests({ pageNumber: this.pageNumber(), pageSize: PAGE_SIZE }).subscribe({
      next: (response) => {
        this.loading.set(false);
        if (response.success && response.data) {
          this.requests.set(response.data.items);
          this.totalCount.set(response.data.totalCount);
        }
      },
      error: (error) => {
        this.loading.set(false);
        this.loadError.set(error?.error?.message ?? 'Could not load booking requests. Please try again.');
      }
    });
  }

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.totalCount() / PAGE_SIZE));
  }

  onPageChange(page: number): void {
    this.pageNumber.set(page);
    this.load();
  }
}
