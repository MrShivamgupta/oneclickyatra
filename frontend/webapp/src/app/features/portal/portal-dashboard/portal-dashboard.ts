import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { PortalService } from '../../../core/services/portal.service';
import { Booking } from '../../../core/models/booking.models';
import { Spinner } from '../../../shared/components/spinner/spinner';

@Component({
  selector: 'app-portal-dashboard',
  standalone: true,
  imports: [RouterLink, Spinner],
  templateUrl: './portal-dashboard.html',
  styleUrl: '../portal-shared.scss'
})
export class PortalDashboard {
  protected readonly authService = inject(AuthService);
  private readonly portalService = inject(PortalService);

  readonly recentBookings = signal<Booking[]>([]);
  readonly totalBookings = signal(0);
  readonly loading = signal(true);
  readonly loadError = signal<string | null>(null);

  constructor() {
    this.portalService.getBookings({ pageNumber: 1, pageSize: 5 }).subscribe({
      next: (response) => {
        this.loading.set(false);
        if (response.success && response.data) {
          this.recentBookings.set(response.data.items);
          this.totalBookings.set(response.data.totalCount);
        }
      },
      error: (error) => {
        this.loading.set(false);
        this.loadError.set(error?.error?.message ?? 'Could not load your dashboard. Please try again.');
      }
    });
  }
}
