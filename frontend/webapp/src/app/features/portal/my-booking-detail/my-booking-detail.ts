import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PortalService } from '../../../core/services/portal.service';
import { Booking, BookingAddOn, BookingPassenger } from '../../../core/models/booking.models';
import { CustomerDocument } from '../../../core/models/portal.models';

const VOUCHER_ELIGIBLE_STATUSES = ['Confirmed', 'InProgress', 'Completed'];

@Component({
  selector: 'app-my-booking-detail',
  standalone: true,
  imports: [DatePipe, DecimalPipe, RouterLink, FormsModule],
  templateUrl: './my-booking-detail.html',
  styleUrl: '../portal-shared.scss'
})
export class MyBookingDetail {
  private readonly portalService = inject(PortalService);
  private readonly route = inject(ActivatedRoute);

  readonly bookingId = signal('');
  readonly booking = signal<Booking | null>(null);
  readonly passengers = signal<BookingPassenger[]>([]);
  readonly addOns = signal<BookingAddOn[]>([]);
  readonly loading = signal(true);
  readonly notFound = signal(false);

  readonly feedbackRating = signal(5);
  readonly feedbackComment = signal('');
  readonly feedbackSubmitting = signal(false);
  readonly feedbackError = signal<string | null>(null);
  readonly feedbackSubmitted = signal(false);

  readonly ratingOptions = [1, 2, 3, 4, 5];

  readonly documents = signal<CustomerDocument[]>([]);
  readonly documentsLoadError = signal<string | null>(null);
  readonly uploading = signal(false);
  readonly uploadError = signal<string | null>(null);
  readonly downloadingDocumentId = signal<string | null>(null);
  readonly downloadingVoucher = signal(false);
  readonly voucherError = signal<string | null>(null);

  constructor() {
    const id = this.route.snapshot.paramMap.get('id') ?? '';
    this.bookingId.set(id);
    this.portalService.getBookingById(id).subscribe({
      next: (response) => {
        this.loading.set(false);
        if (response.success && response.data) {
          this.booking.set(response.data.booking);
          this.passengers.set(response.data.passengers);
          this.addOns.set(response.data.addOns);
          this.loadDocuments();
        } else {
          this.notFound.set(true);
        }
      },
      error: () => {
        this.loading.set(false);
        this.notFound.set(true);
      }
    });
  }

  get canDownloadVoucher(): boolean {
    return VOUCHER_ELIGIBLE_STATUSES.includes(this.booking()?.status ?? '');
  }

  loadDocuments(): void {
    this.documentsLoadError.set(null);
    this.portalService.getDocuments(this.bookingId()).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.documents.set(response.data);
        }
      },
      error: (error) => {
        this.documentsLoadError.set(error?.error?.message ?? 'Could not load your documents. Please try again.');
      }
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) {
      return;
    }

    this.uploading.set(true);
    this.uploadError.set(null);
    this.portalService.uploadDocument(this.bookingId(), file).subscribe({
      next: () => {
        this.uploading.set(false);
        input.value = '';
        this.loadDocuments();
      },
      error: (error) => {
        this.uploading.set(false);
        input.value = '';
        this.uploadError.set(error?.error?.message ?? 'Could not upload this document. Please try again.');
      }
    });
  }

  downloadDocument(document: CustomerDocument): void {
    this.downloadingDocumentId.set(document.id);
    this.uploadError.set(null);
    this.portalService.downloadDocument(document.id).subscribe({
      next: (blob) => {
        this.downloadingDocumentId.set(null);
        triggerBrowserDownload(blob, document.fileName);
      },
      error: (error) => {
        this.downloadingDocumentId.set(null);
        this.uploadError.set(error?.error?.message ?? 'Could not download this document. Please try again.');
      }
    });
  }

  downloadVoucher(): void {
    this.downloadingVoucher.set(true);
    this.voucherError.set(null);
    this.portalService.downloadVoucher(this.bookingId()).subscribe({
      next: (blob) => {
        this.downloadingVoucher.set(false);
        triggerBrowserDownload(blob, `Voucher-${this.booking()?.bookingNumber ?? this.bookingId()}.pdf`);
      },
      error: (error) => {
        this.downloadingVoucher.set(false);
        this.voucherError.set(error?.error?.message ?? 'Could not download the voucher. Please try again.');
      }
    });
  }

  setRating(value: number): void {
    this.feedbackRating.set(value);
  }

  submitFeedback(): void {
    this.feedbackSubmitting.set(true);
    this.feedbackError.set(null);
    this.portalService.submitFeedback(this.bookingId(), { rating: this.feedbackRating(), comment: this.feedbackComment() || null }).subscribe({
      next: () => {
        this.feedbackSubmitting.set(false);
        this.feedbackSubmitted.set(true);
      },
      error: (error) => {
        this.feedbackSubmitting.set(false);
        this.feedbackError.set(error?.error?.message ?? 'Could not submit feedback.');
      }
    });
  }
}

function triggerBrowserDownload(blob: Blob, fileName: string): void {
  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = fileName;
  link.click();
  URL.revokeObjectURL(url);
}
