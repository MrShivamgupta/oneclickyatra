import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { DataTable, DataTableColumn } from '../../shared/components/data-table/data-table';
import { Pagination } from '../../shared/components/pagination/pagination';
import { FilterBar } from '../../shared/components/filter-bar/filter-bar';
import { InvoiceService } from '../../core/services/invoice.service';
import { Invoice } from '../../core/models/payment.models';

const PAGE_SIZE = 10;

@Component({
  selector: 'app-invoices',
  standalone: true,
  imports: [DataTable, Pagination, FilterBar, DatePipe, DecimalPipe],
  templateUrl: './invoices.html',
  styleUrls: ['../destinations/destinations.scss', '../leads/leads-list/leads-list.scss']
})
export class Invoices {
  private readonly invoiceService = inject(InvoiceService);

  readonly columns: DataTableColumn[] = [
    { key: 'number', label: 'Invoice #' },
    { key: 'booking', label: 'Booking' },
    { key: 'customer', label: 'Customer' },
    { key: 'amount', label: 'Amount' },
    { key: 'issuedAt', label: 'Issued' },
    { key: 'actions', label: '' }
  ];

  readonly invoices = signal<Invoice[]>([]);
  readonly loading = signal(false);
  readonly loadError = signal<string | null>(null);
  readonly pageNumber = signal(1);
  readonly totalCount = signal(0);
  readonly searchTerm = signal('');
  readonly downloadingId = signal<string | null>(null);
  readonly downloadError = signal<{ id: string; message: string } | null>(null);

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.loadError.set(null);
    this.invoiceService
      .search({ pageNumber: this.pageNumber(), pageSize: PAGE_SIZE, searchTerm: this.searchTerm() || undefined })
      .subscribe({
        next: (response) => {
          this.loading.set(false);
          if (response.success && response.data) {
            this.invoices.set(response.data.items);
            this.totalCount.set(response.data.totalCount);
          } else {
            this.invoices.set([]);
            this.totalCount.set(0);
            this.loadError.set(response.message || 'Could not load invoices. Please try again.');
          }
        },
        error: (error) => {
          this.loading.set(false);
          this.loadError.set(error?.error?.message ?? 'Could not load invoices. Please try again.');
        }
      });
  }

  onSearchChange(term: string): void {
    this.searchTerm.set(term);
    this.pageNumber.set(1);
    this.load();
  }

  onPageChange(page: number): void {
    this.pageNumber.set(page);
    this.load();
  }

  downloadPdf(invoice: Invoice): void {
    this.downloadingId.set(invoice.id);
    this.downloadError.set(null);
    this.invoiceService.downloadPdf(invoice.id).subscribe({
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
        this.downloadError.set({
          id: invoice.id,
          message: error?.error?.message ?? 'Could not download this invoice. Please try again.'
        });
      }
    });
  }
}
