import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FilterBar } from '../../shared/components/filter-bar/filter-bar';
import { Pagination } from '../../shared/components/pagination/pagination';
import { DestinationService } from '../../core/services/destination.service';
import { Destination } from '../../core/models/master-data.models';

const PAGE_SIZE = 12;

@Component({
  selector: 'app-destination-list',
  standalone: true,
  imports: [RouterLink, FilterBar, Pagination],
  templateUrl: './destination-list.html',
  styleUrl: './destination-list.scss'
})
export class DestinationList {
  private readonly destinationService = inject(DestinationService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly destinations = signal<Destination[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly pageNumber = signal(1);
  readonly totalCount = signal(0);
  readonly searchTerm = signal('');

  constructor() {
    this.route.queryParamMap.subscribe((params) => {
      this.searchTerm.set(params.get('searchTerm') ?? '');
      this.pageNumber.set(Math.max(1, Number(params.get('page') ?? '1') || 1));
      this.load();
    });
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.destinationService
      .search({
        pageNumber: this.pageNumber(),
        pageSize: PAGE_SIZE,
        isPublished: true,
        searchTerm: this.searchTerm() || undefined
      })
      .subscribe({
        next: (response) => {
          this.loading.set(false);
          if (response.success && response.data) {
            this.destinations.set(response.data.items);
            this.totalCount.set(response.data.totalCount);
          } else {
            this.destinations.set([]);
            this.totalCount.set(0);
            this.error.set(response.message || 'Could not load destinations. Please try again.');
          }
        },
        error: (error) => {
          this.loading.set(false);
          this.error.set(error?.error?.message ?? 'Could not load destinations. Please try again.');
        }
      });
  }

  onSearchChange(term: string): void {
    this.searchTerm.set(term);
    this.pageNumber.set(1);
    this.syncUrl();
  }

  onPageChange(page: number): void {
    this.pageNumber.set(page);
    this.syncUrl();
  }

  private syncUrl(): void {
    const queryParams: Record<string, string | number> = {};
    if (this.searchTerm()) {
      queryParams['searchTerm'] = this.searchTerm();
    }
    if (this.pageNumber() > 1) {
      queryParams['page'] = this.pageNumber();
    }

    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: Object.keys(queryParams).length > 0 ? queryParams : null
    });
  }
}
