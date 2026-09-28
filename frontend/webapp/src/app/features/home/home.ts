import { Component, DestroyRef, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { DestinationService } from '../../core/services/destination.service';
import { PackageService } from '../../core/services/package.service';
import { CurrencyService } from '../../core/services/currency.service';
import { FeedbackService } from '../../core/services/feedback.service';
import { Destination } from '../../core/models/master-data.models';
import { PackageSummary } from '../../core/models/package.models';
import { Testimonial } from '../../core/models/feedback.models';
import { Spinner } from '../../shared/components/spinner/spinner';

const RATING_STARS = [1, 2, 3, 4, 5];

interface TrustFeature {
  icon: string;
  title: string;
  description: string;
}

const HERO_ROTATION_MS = 6000;

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [FormsModule, RouterLink, Spinner],
  templateUrl: './home.html',
  styleUrl: './home.scss'
})
export class Home {
  private readonly destinationService = inject(DestinationService);
  private readonly packageService = inject(PackageService);
  private readonly feedbackService = inject(FeedbackService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  protected readonly currencyService = inject(CurrencyService);

  readonly ratingStars = RATING_STARS;

  readonly trustFeatures: TrustFeature[] = [
    {
      icon: '✔',
      title: 'Best Price Guarantee',
      description: 'Transparent pricing with no hidden fees, matched against the best rates we can find.'
    },
    {
      icon: '🛟',
      title: '24/7 Traveller Support',
      description: 'Our team is reachable around the clock, before, during, and after your trip.'
    },
    {
      icon: '🔒',
      title: 'Secure Payments',
      description: 'Every payment is processed through a PCI-compliant, encrypted gateway.'
    },
    {
      icon: '🧭',
      title: 'Handpicked Itineraries',
      description: 'Every destination and package is curated and verified by our travel experts.'
    }
  ];

  destination = '';
  travelDate = '';
  travelers = 1;

  readonly heroImages = [
    'https://images.unsplash.com/photo-1507525428034-b723cf961d3e?w=1920&q=80&auto=format&fit=crop',
    'https://images.unsplash.com/photo-1573843981267-be1999ff37cd?w=1920&q=80&auto=format&fit=crop',
    'https://images.unsplash.com/photo-1506929562872-bb421503ef21?w=1920&q=80&auto=format&fit=crop',
    'https://images.unsplash.com/photo-1470770903676-69b98201ea1c?w=1920&q=80&auto=format&fit=crop',
    'https://images.unsplash.com/photo-1524492412937-b28074a5d7da?w=1920&q=80&auto=format&fit=crop'
  ];
  readonly heroIndex = signal(0);

  readonly featuredDestinations = signal<Destination[]>([]);
  readonly featuredPackages = signal<PackageSummary[]>([]);
  readonly loadingDestinations = signal(true);
  readonly loadingPackages = signal(true);
  readonly destinationsError = signal<string | null>(null);
  readonly packagesError = signal<string | null>(null);

  readonly testimonials = signal<Testimonial[]>([]);
  readonly loadingTestimonials = signal(true);

  constructor() {
    const rotationId = setInterval(() => {
      this.heroIndex.update((i) => (i + 1) % this.heroImages.length);
    }, HERO_ROTATION_MS);
    this.destroyRef.onDestroy(() => clearInterval(rotationId));

    this.destinationService
      .search({ pageNumber: 1, pageSize: 6, isFeatured: true, isPublished: true })
      .subscribe({
        next: (response) => {
          this.loadingDestinations.set(false);
          if (response.success && response.data) {
            this.featuredDestinations.set(response.data.items);
          }
        },
        error: (error) => {
          this.loadingDestinations.set(false);
          this.destinationsError.set(error?.error?.message ?? 'Could not load featured destinations right now.');
        }
      });

    this.packageService.search({ pageNumber: 1, pageSize: 6, status: 'Published' }).subscribe({
      next: (response) => {
        this.loadingPackages.set(false);
        if (response.success && response.data) {
          this.featuredPackages.set(response.data.items);
        }
      },
      error: (error) => {
        this.loadingPackages.set(false);
        this.packagesError.set(error?.error?.message ?? 'Could not load featured packages right now.');
      }
    });

    this.feedbackService.getPublicTestimonials().subscribe({
      next: (response) => {
        this.loadingTestimonials.set(false);
        if (response.success && response.data) this.testimonials.set(response.data);
      },
      error: () => {
        // A nice-to-have section -- if it fails to load, the section is simply hidden (see
        // @if (testimonials().length > 0) in the template), no error banner needed on a home page.
        this.loadingTestimonials.set(false);
      }
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
