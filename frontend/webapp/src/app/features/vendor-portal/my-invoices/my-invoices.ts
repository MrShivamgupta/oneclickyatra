import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { VendorPortalService } from '../../../core/services/vendor-portal.service';
import { VendorInvoice } from '../../../core/models/vendor.models';
import { Spinner } from '../../../shared/components/spinner/spinner';

const PAGE_SIZE = 10;

@Component({
  selector: 'app-vendor-my-invoices',
  standalone: true,
  imports: [DatePipe, DecimalPipe, FormsModule, Spinner],
  templateUrl: './my-invoices.html',
  styleUrl: '../vendor-portal-shared.scss'
})
export class MyInvoices {
  private readonly vendorPortalService = inject(VendorPortalService);

  readonly invoices = signal<VendorInvoice[]>([]);
  readonly loading = signal(true);
  readonly loadError = signal<string | null>(null);
  readonly pageNumber = signal(1);
  readonly totalCount = signal(0);

  readonly amount = signal<number | null>(null);
  readonly notes = signal('');
  readonly submitting = signal(false);
  readonly submitError = signal<string | null>(null);
  private selectedFile: File | null = null;

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.loadError.set(null);
    this.vendorPortalService.getMyInvoices({ pageNumber: this.pageNumber(), pageSize: PAGE_SIZE }).subscribe({
      next: (response) => {
        this.loading.set(false);
        if (response.success && response.data) {
          this.invoices.set(response.data.items);
          this.totalCount.set(response.data.totalCount);
        }
      },
      error: (error) => {
        this.loading.set(false);
        this.loadError.set(error?.error?.message ?? 'Could not load your invoices. Please try again.');
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

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedFile = input.files?.[0] ?? null;
  }

  submitInvoice(): void {
    if (!this.selectedFile || !this.amount()) {
      this.submitError.set('Please choose a file and enter an amount.');
      return;
    }

    this.submitting.set(true);
    this.submitError.set(null);
    this.vendorPortalService.submitInvoice(this.selectedFile, this.amount()!, this.notes() || null, null).subscribe({
      next: () => {
        this.submitting.set(false);
        this.amount.set(null);
        this.notes.set('');
        this.selectedFile = null;
        this.load();
      },
      error: (error) => {
        this.submitting.set(false);
        this.submitError.set(error?.error?.message ?? 'Could not submit this invoice. Please try again.');
      }
    });
  }
}
