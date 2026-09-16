import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FilterBar } from '../../shared/components/filter-bar/filter-bar';
import { Pagination } from '../../shared/components/pagination/pagination';
import { PackageService } from '../../core/services/package.service';
import { DestinationService } from '../../core/services/destination.service';
import { PackageSummary } from '../../core/models/package.models';
import { Destination } from '../../core/models/master-data.models';

const PAGE_SIZE = 12;

@Component({
  selector: 'app-package-search',
  standalone: true,
  imports: [RouterLink, FilterBar, Pagination],
  templateUrl: './package-search.html',
  styleUrl: './package-search.scss'
})
export class PackageSearch {
  private readonly packageService = inject(PackageService);
  private readonly destinationService = inject(DestinationService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly packages = signal<PackageSummary[]>([]);
  readonly destinations = signal<Destination[]>([]);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly pageNumber = signal(1);
  readonly totalCount = signal(0);
  readonly searchTerm = signal('');
  readonly destinationFilter = signal('');

  constructor() {
    this.destinationService.search({ pageNumber: 1, pageSize: 100, isPublished: true }).subscribe((response) => {
      if (response.success && response.data) {
        this.destinations.set(response.data.items);
      }
    });

    this.route.queryParamMap.subscribe((params) => {
      this.searchTerm.set(params.get('searchTerm') ?? '');
      this.destinationFilter.set(params.get('destinationId') ?? '');
      this.pageNumber.set(Math.max(1, Number(params.get('page') ?? '1') || 1));
      this.load();
    });
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.packageService
      .search({
        pageNumber: this.pageNumber(),
        pageSize: PAGE_SIZE,
        status: 'Published',
        searchTerm: this.searchTerm() || undefined,
        destinationId: this.destinationFilter() || undefined
      })
      .subscribe({
        next: (response) => {
          this.loading.set(false);
          if (response.success && response.data) {
            this.packages.set(response.data.items);
            this.totalCount.set(response.data.totalCount);
          } else {
            this.packages.set([]);
            this.totalCount.set(0);
            this.error.set(response.message || 'Unable to load packages right now.');
          }
        },
        error: (error) => {
          this.loading.set(false);
          this.error.set(error?.error?.message ?? 'Unable to load packages right now. Please try again.');
        }
      });
  }

  onSearchChange(term: string): void {
    this.searchTerm.set(term);
    this.pageNumber.set(1);
    this.syncUrl();
  }

  onDestinationFilterChange(destinationId: string): void {
    this.destinationFilter.set(destinationId);
    this.pageNumber.set(1);
    this.syncUrl();
  }

  onPageChange(page: number): void {
    this.pageNumber.set(page);
    this.syncUrl();
  }

  enquiryQueryParams(): Record<string, string> {
    const params: Record<string, string> = {};
    if (this.destinationFilter()) {
      params['destinationId'] = this.destinationFilter();
    }
    if (this.searchTerm()) {
      params['packageTitle'] = this.searchTerm();
    }
    return params;
  }

  formatPrice(amount: number): string {
    return amount.toLocaleString('en-IN');
  }

  private syncUrl(): void {
    const queryParams: Record<string, string | number> = {};
    if (this.searchTerm()) {
      queryParams['searchTerm'] = this.searchTerm();
    }
    if (this.destinationFilter()) {
      queryParams['destinationId'] = this.destinationFilter();
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
