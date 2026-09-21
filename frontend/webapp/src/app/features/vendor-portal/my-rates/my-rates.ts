import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { VendorPortalService } from '../../../core/services/vendor-portal.service';
import { VendorRate } from '../../../core/models/vendor.models';

@Component({
  selector: 'app-my-rates',
  standalone: true,
  imports: [DatePipe, DecimalPipe],
  templateUrl: './my-rates.html',
  styleUrl: '../vendor-portal-shared.scss'
})
export class MyRates {
  private readonly vendorPortalService = inject(VendorPortalService);

  readonly rates = signal<VendorRate[]>([]);
  readonly loading = signal(true);
  readonly loadError = signal<string | null>(null);

  constructor() {
    this.vendorPortalService.getRates().subscribe({
      next: (response) => {
        this.loading.set(false);
        if (response.success && response.data) this.rates.set(response.data);
      },
      error: (error) => {
        this.loading.set(false);
        this.loadError.set(error?.error?.message ?? 'Could not load your rates. Please try again.');
      }
    });
  }
}
