import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { VendorPortalService } from '../../../core/services/vendor-portal.service';
import { Vendor } from '../../../core/models/vendor.models';
import { Spinner } from '../../../shared/components/spinner/spinner';

@Component({
  selector: 'app-vendor-portal-dashboard',
  standalone: true,
  imports: [RouterLink, Spinner],
  templateUrl: './vendor-portal-dashboard.html',
  styleUrl: '../vendor-portal-shared.scss'
})
export class VendorPortalDashboard {
  protected readonly authService = inject(AuthService);
  private readonly vendorPortalService = inject(VendorPortalService);

  readonly vendor = signal<Vendor | null>(null);
  readonly loading = signal(true);
  readonly loadError = signal<string | null>(null);

  constructor() {
    this.vendorPortalService.getProfile().subscribe({
      next: (response) => {
        this.loading.set(false);
        if (response.success && response.data) this.vendor.set(response.data);
      },
      error: (error) => {
        this.loading.set(false);
        this.loadError.set(error?.error?.message ?? 'Could not load your vendor profile. Please try again.');
      }
    });
  }
}
