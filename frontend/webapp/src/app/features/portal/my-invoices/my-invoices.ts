import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { PortalService } from '../../../core/services/portal.service';
import { Invoice } from '../../../core/models/payment.models';
import { Spinner } from '../../../shared/components/spinner/spinner';

const PAGE_SIZE = 10;

@Component({
  selector: 'app-my-invoices',
  standalone: true,
  imports: [DatePipe, DecimalPipe, Spinner],
  templateUrl: './my-invoices.html',
  styleUrl: '../portal-shared.scss'
})
export class MyInvoices {
  private readonly portalService = inject(PortalService);

  readonly invoices = signal<Invoice[]>([]);
  readonly loading = signal(true);
  readonly loadError = signal<string | null>(null);
  readonly pageNumber = signal(1);
  readonly totalCount = signal(0);
  readonly downloadingId = signal<string | null>(null);
  readonly downloadError = signal<string | null>(null);

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.loadError.set(null);
    this.portalService.getInvoices({ pageNumber: this.pageNumber(), pageSize: PAGE_SIZE }).subscribe({
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

  downloadPdf(invoice: Invoice): void {
    this.downloadingId.set(invoice.id);
    this.downloadError.set(null);
    this.portalService.downloadInvoicePdf(invoice.id).subscribe({
      next: (blob) => {
        this.downloadingId.set(null);
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `${invoice.invoiceNumber}.pdf`;
        link.click();
        URL.revokeObjectURL(url);
      },
      error: (error) => {
        this.downloadingId.set(null);
        this.downloadError.set(error?.error?.message ?? 'Could not download this invoice. Please try again.');
      }
    });
  }
}
