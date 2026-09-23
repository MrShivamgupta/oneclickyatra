import { DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { PortalService } from '../../../core/services/portal.service';
import { Quotation } from '../../../core/models/quotation.models';
import { Spinner } from '../../../shared/components/spinner/spinner';

const PAGE_SIZE = 10;

@Component({
  selector: 'app-my-quotations',
  standalone: true,
  imports: [DatePipe, Spinner],
  templateUrl: './my-quotations.html',
  styleUrl: '../portal-shared.scss'
})
export class MyQuotations {
  private readonly portalService = inject(PortalService);

  readonly quotations = signal<Quotation[]>([]);
  readonly loading = signal(true);
  readonly loadError = signal<string | null>(null);
  readonly pageNumber = signal(1);
  readonly totalCount = signal(0);

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.loadError.set(null);
    this.portalService.getQuotations({ pageNumber: this.pageNumber(), pageSize: PAGE_SIZE }).subscribe({
      next: (response) => {
        this.loading.set(false);
        if (response.success && response.data) {
          this.quotations.set(response.data.items);
          this.totalCount.set(response.data.totalCount);
        }
      },
      error: (error) => {
        this.loading.set(false);
        this.loadError.set(error?.error?.message ?? 'Could not load your quotations. Please try again.');
      }
    });
  }

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.totalCount() / PAGE_SIZE));
  }

  onPageChange(page: number): void {
    this.pageNumber.set(page);
    this.load();
  }
}
