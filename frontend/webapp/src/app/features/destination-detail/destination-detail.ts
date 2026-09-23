import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DestinationService } from '../../core/services/destination.service';
import { Destination } from '../../core/models/master-data.models';
import { Spinner } from '../../shared/components/spinner/spinner';

@Component({
  selector: 'app-destination-detail',
  standalone: true,
  imports: [RouterLink, Spinner],
  templateUrl: './destination-detail.html',
  styleUrl: './destination-detail.scss'
})
export class DestinationDetail {
  private readonly route = inject(ActivatedRoute);
  private readonly destinationService = inject(DestinationService);

  readonly destination = signal<Destination | null>(null);
  readonly loading = signal(true);
  readonly notFound = signal(false);
  readonly error = signal<string | null>(null);

  constructor() {
    const slug = this.route.snapshot.paramMap.get('slug');
    if (slug) {
      this.load(slug);
    } else {
      this.loading.set(false);
      this.notFound.set(true);
    }
  }

  load(slug: string): void {
    this.loading.set(true);
    this.notFound.set(false);
    this.error.set(null);
    this.destinationService.getBySlug(slug).subscribe({
      next: (response) => {
        this.loading.set(false);
        if (response.success && response.data) {
          this.destination.set(response.data);
        } else {
          this.notFound.set(true);
        }
      },
      error: (error) => {
        this.loading.set(false);
        if (error?.status === 404) {
          this.notFound.set(true);
        } else {
          this.error.set(error?.error?.message ?? 'Could not load this destination. Please try again.');
        }
      }
    });
  }
}
