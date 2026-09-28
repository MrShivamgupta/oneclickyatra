import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { PackageService } from '../../core/services/package.service';
import { CurrencyService } from '../../core/services/currency.service';
import { PackageDetail as PackageDetailDto, PackageInventoryDeparture } from '../../core/models/package.models';
import { Spinner } from '../../shared/components/spinner/spinner';

@Component({
  selector: 'app-package-detail',
  standalone: true,
  imports: [RouterLink, Spinner],
  templateUrl: './package-detail.html',
  styleUrl: './package-detail.scss'
})
export class PackageDetail {
  private readonly route = inject(ActivatedRoute);
  private readonly packageService = inject(PackageService);
  protected readonly currencyService = inject(CurrencyService);

  readonly loading = signal(true);
  readonly notFound = signal(false);
  readonly error = signal<string | null>(null);
  readonly detail = signal<PackageDetailDto | null>(null);

  readonly coverImageUrl = computed(() => {
    const detail = this.detail();
    if (!detail) {
      return null;
    }
    const cover = detail.media.find((item) => item.isCoverImage && item.mediaType === 'Image');
    return cover?.mediaUrl ?? detail.package.heroImageUrl ?? null;
  });

  readonly sortedItinerary = computed(() => [...(this.detail()?.itinerary ?? [])].sort((a, b) => a.dayNumber - b.dayNumber));
  readonly includedItems = computed(() => this.detail()?.inclusions.filter((item) => item.isIncluded) ?? []);
  readonly excludedItems = computed(() => this.detail()?.inclusions.filter((item) => !item.isIncluded) ?? []);
  readonly galleryImages = computed(() => {
    const detail = this.detail();
    if (!detail) {
      return [];
    }
    const coverUrl = this.coverImageUrl();
    return detail.media.filter(
      (item) => item.mediaType === 'Image' && item.mediaUrl && item.mediaUrl !== coverUrl
    );
  });

  readonly enquiryQueryParams = computed(() => {
    const pkg = this.detail()?.package;
    if (!pkg) {
      return {};
    }
    const params: Record<string, string> = { packageTitle: pkg.title };
    if (pkg.destinationId) {
      params['destinationId'] = pkg.destinationId;
    }
    return params;
  });

  constructor() {
    const slug = this.route.snapshot.paramMap.get('slug');
    if (!slug) {
      this.loading.set(false);
      this.notFound.set(true);
      return;
    }
    this.load(slug);
  }

  availableSeats(departure: PackageInventoryDeparture): number {
    return departure.availableSeats ?? Math.max(0, departure.totalSeats - departure.bookedSeats);
  }

  formatPrice(amount: number): string {
    return amount.toLocaleString('en-IN');
  }

  formatDate(dateValue: string): string {
    const date = new Date(dateValue);
    if (Number.isNaN(date.getTime())) {
      return dateValue;
    }
    return date.toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' });
  }

  private load(slug: string): void {
    this.loading.set(true);
    this.error.set(null);
    this.notFound.set(false);

    this.packageService.getBySlug(slug).subscribe({
      next: (response) => {
        this.loading.set(false);
        if (response.success && response.data) {
          this.detail.set(response.data);
        } else {
          this.notFound.set(true);
        }
      },
      error: (error) => {
        this.loading.set(false);
        if (error?.status === 404) {
          this.notFound.set(true);
        } else {
          this.error.set(error?.error?.message ?? 'Unable to load this package right now. Please try again.');
        }
      }
    });
  }
}
