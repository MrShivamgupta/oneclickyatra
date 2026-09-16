import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { DestinationService } from '../../core/services/destination.service';
import { PackageService } from '../../core/services/package.service';
import { Destination } from '../../core/models/master-data.models';
import { PackageSummary } from '../../core/models/package.models';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [FormsModule, RouterLink],
  templateUrl: './home.html',
  styleUrl: './home.scss'
})
export class Home {
  private readonly destinationService = inject(DestinationService);
  private readonly packageService = inject(PackageService);
  private readonly router = inject(Router);

  destination = '';
  travelDate = '';
  travelers = 1;

  readonly featuredDestinations = signal<Destination[]>([]);
  readonly featuredPackages = signal<PackageSummary[]>([]);
  readonly loadingDestinations = signal(true);
  readonly loadingPackages = signal(true);

  constructor() {
    this.destinationService
      .search({ pageNumber: 1, pageSize: 6, isFeatured: true, isPublished: true })
      .subscribe({
        next: (response) => {
          this.loadingDestinations.set(false);
          if (response.success && response.data) {
            this.featuredDestinations.set(response.data.items);
          }
        },
        error: () => this.loadingDestinations.set(false)
      });

    this.packageService.search({ pageNumber: 1, pageSize: 6, status: 'Published' }).subscribe({
      next: (response) => {
        this.loadingPackages.set(false);
        if (response.success && response.data) {
          this.featuredPackages.set(response.data.items);
        }
      },
      error: () => this.loadingPackages.set(false)
    });
  }

  formatPrice(amount: number): string {
    return amount.toLocaleString('en-IN');
  }

  search(): void {
    const trimmedDestination = this.destination.trim();
    const queryParams: Record<string, string | number> = {};

    if (trimmedDestination) {
      queryParams['searchTerm'] = trimmedDestination;
    }
    if (this.travelDate) {
      queryParams['travelDate'] = this.travelDate;
    }
    if (this.travelers > 1) {
      queryParams['travelers'] = this.travelers;
    }

    if (!trimmedDestination && (this.travelDate || this.travelers > 1)) {
      this.router.navigate(['/enquiry'], { queryParams });
      return;
    }

    this.router.navigate(['/packages'], {
      queryParams: Object.keys(queryParams).length > 0 ? queryParams : undefined
    });
  }
}
