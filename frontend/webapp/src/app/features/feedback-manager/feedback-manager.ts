import { DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { DataTable, DataTableColumn } from '../../shared/components/data-table/data-table';
import { Pagination } from '../../shared/components/pagination/pagination';
import { FilterBar } from '../../shared/components/filter-bar/filter-bar';
import { FeedbackService } from '../../core/services/feedback.service';
import { Feedback } from '../../core/models/feedback.models';

const PAGE_SIZE = 10;
const RATING_OPTIONS = [1, 2, 3, 4, 5];

@Component({
  selector: 'app-feedback-manager',
  standalone: true,
  imports: [DataTable, Pagination, FilterBar, DatePipe],
  templateUrl: './feedback-manager.html',
  styleUrls: ['../destinations/destinations.scss', '../bookings/bookings-list/bookings-list.scss', './feedback-manager.scss']
})
export class FeedbackManager {
  private readonly feedbackService = inject(FeedbackService);

  readonly ratingOptions = RATING_OPTIONS;

  readonly columns: DataTableColumn[] = [
    { key: 'booking', label: 'Booking #' },
    { key: 'customer', label: 'Customer' },
    { key: 'rating', label: 'Rating' },
    { key: 'comment', label: 'Comment' },
    { key: 'date', label: 'Date' }
  ];

  readonly feedbacks = signal<Feedback[]>([]);
  readonly loading = signal(false);
  readonly loadError = signal<string | null>(null);
  readonly pageNumber = signal(1);
  readonly totalCount = signal(0);
  readonly searchTerm = signal('');
  readonly minRatingFilter = signal('');

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.loadError.set(null);
    this.feedbackService
      .search({
        pageNumber: this.pageNumber(),
        pageSize: PAGE_SIZE,
        searchTerm: this.searchTerm() || undefined,
        minRating: this.minRatingFilter() ? Number(this.minRatingFilter()) : undefined
      })
      .subscribe({
        next: (response) => {
          this.loading.set(false);
          if (response.success && response.data) {
            this.feedbacks.set(response.data.items);
            this.totalCount.set(response.data.totalCount);
          }
        },
        error: (error) => {
          this.loading.set(false);
          this.loadError.set(error?.error?.message ?? 'Could not load feedback. Please try again.');
        }
      });
  }

  onSearchChange(term: string): void {
    this.searchTerm.set(term);
    this.pageNumber.set(1);
    this.load();
  }

  onMinRatingChange(value: string): void {
    this.minRatingFilter.set(value);
    this.pageNumber.set(1);
    this.load();
  }

  onPageChange(page: number): void {
    this.pageNumber.set(page);
    this.load();
  }

  stars(rating: number): string {
    return '★'.repeat(rating) + '☆'.repeat(5 - rating);
  }
}
