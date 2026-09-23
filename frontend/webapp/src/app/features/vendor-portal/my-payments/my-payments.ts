import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { VendorPortalService } from '../../../core/services/vendor-portal.service';
import { VendorPayment } from '../../../core/models/vendor.models';
import { Spinner } from '../../../shared/components/spinner/spinner';

const PAGE_SIZE = 10;

@Component({
  selector: 'app-vendor-my-payments',
  standalone: true,
  imports: [DatePipe, DecimalPipe, Spinner],
  templateUrl: './my-payments.html',
  styleUrl: '../vendor-portal-shared.scss'
})
export class MyPayments {
  private readonly vendorPortalService = inject(VendorPortalService);

  readonly payments = signal<VendorPayment[]>([]);
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
    this.vendorPortalService.getPayments({ pageNumber: this.pageNumber(), pageSize: PAGE_SIZE }).subscribe({
      next: (response) => {
        this.loading.set(false);
        if (response.success && response.data) {
          this.payments.set(response.data.items);
          this.totalCount.set(response.data.totalCount);
        }
      },
      error: (error) => {
        this.loading.set(false);
        this.loadError.set(error?.error?.message ?? 'Could not load your payments. Please try again.');
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
