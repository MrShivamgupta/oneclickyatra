import { DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { FormsModule } from '@angular/forms';
import { ConfirmationDialog } from '../../../shared/components/confirmation-dialog/confirmation-dialog';
import { QuotationService } from '../../../core/services/quotation.service';
import { LeadService } from '../../../core/services/lead.service';
import { DestinationService } from '../../../core/services/destination.service';
import { BookingService } from '../../../core/services/booking.service';
import { Lead } from '../../../core/models/crm.models';
import { Destination } from '../../../core/models/master-data.models';
import { Quotation, QuotationItem, QuotationOption, QuotationOptionRequest } from '../../../core/models/quotation.models';

const DELETABLE_STATUSES = ['Draft', 'Rejected', 'Expired'];
const EDITABLE_STATUSES = ['Draft', 'Sent', 'Expired'];

@Component({
  selector: 'app-quotation-detail',
  standalone: true,
  imports: [ReactiveFormsModule, FormsModule, RouterLink, ConfirmationDialog, DatePipe],
  templateUrl: './quotation-detail.html',
  styleUrl: './quotation-detail.scss'
})
export class QuotationDetailPage {
  private readonly quotationService = inject(QuotationService);
  private readonly leadService = inject(LeadService);
  private readonly destinationService = inject(DestinationService);
  private readonly bookingService = inject(BookingService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly quotationId = signal<string | null>(null);
  readonly isNew = signal(false);
  readonly quotation = signal<Quotation | null>(null);
  readonly loading = signal(false);

  readonly leads = signal<Lead[]>([]);
  readonly destinations = signal<Destination[]>([]);
  readonly selectedLeadId = signal('');

  readonly options = signal<QuotationOption[]>([]);

  readonly basicError = signal<string | null>(null);
  readonly basicSaving = signal(false);
  readonly optionsError = signal<string | null>(null);
  readonly optionsSaving = signal(false);
  readonly sendError = signal<string | null>(null);
  readonly sendSaving = signal(false);
  readonly linkSaving = signal(false);
  readonly pdfDownloading = signal(false);
  readonly deleteConfirmOpen = signal(false);
  readonly convertSaving = signal(false);
  readonly convertError = signal<string | null>(null);

  /** The raw share token is only ever returned by Create/RegenerateLink — never by GetById. */
  readonly lastKnownPublicToken = signal<string | null>(null);

  readonly basicForm = this.formBuilder.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    validUntil: [''],
    notes: ['']
  });

  constructor() {
    this.destinationService.search({ pageNumber: 1, pageSize: 100 }).subscribe((response) => {
      if (response.success && response.data) this.destinations.set(response.data.items);
    });

    const idParam = this.route.snapshot.paramMap.get('id');
    if (!idParam || idParam === 'new') {
      this.isNew.set(true);
      this.leadService.search({ pageNumber: 1, pageSize: 100 }).subscribe((response) => {
        if (response.success && response.data) this.leads.set(response.data.items);
      });
      const leadIdFromQuery = this.route.snapshot.queryParamMap.get('leadId');
      if (leadIdFromQuery) this.selectedLeadId.set(leadIdFromQuery);
      return;
    }

    this.quotationId.set(idParam);
    this.load();
  }

  load(): void {
    const id = this.quotationId();
    if (!id) return;
    this.loading.set(true);
    this.quotationService.getById(id).subscribe({
      next: (response) => {
        this.loading.set(false);
        if (response.success && response.data) {
          this.quotation.set(response.data.quotation);
          this.basicForm.setValue({
            title: response.data.quotation.title,
            validUntil: response.data.quotation.validUntil ?? '',
            notes: response.data.quotation.notes ?? ''
          });
          this.options.set(response.data.options.map((option) => ({ ...option, items: [...option.items] })));
        }
      },
      error: () => this.loading.set(false)
    });
  }

  get isEditable(): boolean {
    const status = this.quotation()?.status;
    return this.isNew() || (!!status && EDITABLE_STATUSES.includes(status));
  }

  get isDeletable(): boolean {
    const status = this.quotation()?.status;
    return !!status && DELETABLE_STATUSES.includes(status);
  }

  saveBasic(): void {
    if (this.basicForm.invalid) {
      this.basicForm.markAllAsTouched();
      return;
    }

    if (this.isNew() && !this.selectedLeadId()) {
      this.basicError.set('Please select a lead for this quotation.');
      return;
    }

    this.basicSaving.set(true);
    this.basicError.set(null);
    const raw = this.basicForm.getRawValue();
    const request = {
      leadId: this.isNew() ? this.selectedLeadId() : this.quotation()!.leadId,
      title: raw.title,
      validUntil: raw.validUntil || null,
      notes: raw.notes || null
    };

    if (this.isNew()) {
      this.quotationService.create(request).subscribe({
        next: (response) => {
          this.basicSaving.set(false);
          if (response.success && response.data) {
            this.router.navigate(['/admin/quotations', response.data.id]);
          }
        },
        error: (error) => {
          this.basicSaving.set(false);
          this.basicError.set(error?.error?.message ?? 'Something went wrong. Please try again.');
        }
      });
      return;
    }

    this.quotationService.update(this.quotationId()!, request).subscribe({
      next: (response) => {
        this.basicSaving.set(false);
        if (response.success && response.data) this.quotation.set(response.data);
      },
      error: (error) => {
        this.basicSaving.set(false);
        this.basicError.set(error?.error?.message ?? 'Something went wrong. Please try again.');
      }
    });
  }

  addOption(): void {
    this.options.update((current) => [
      ...current,
      {
        optionName: '',
        destinationId: null,
        durationDays: null,
        durationNights: null,
        hotelCategory: null,
        numberOfPeople: 2,
        pricePerPerson: 0,
        totalPrice: 0,
        isRecommended: current.length === 0,
        sortOrder: current.length,
        items: []
      }
    ]);
  }

  removeOption(index: number): void {
    this.options.update((current) => current.filter((_, i) => i !== index));
  }

  addItem(optionIndex: number): void {
    this.options.update((current) => {
      const next = [...current];
      next[optionIndex] = { ...next[optionIndex], items: [...next[optionIndex].items, { description: '', amount: 0, sortOrder: next[optionIndex].items.length }] };
      return next;
    });
  }

  removeItem(optionIndex: number, itemIndex: number): void {
    this.options.update((current) => {
      const next = [...current];
      next[optionIndex] = { ...next[optionIndex], items: next[optionIndex].items.filter((_, i) => i !== itemIndex) };
      return next;
    });
  }

  optionTotal(option: QuotationOption): number {
    return (option.pricePerPerson || 0) * (option.numberOfPeople || 0);
  }

  saveOptions(): void {
    const id = this.quotationId();
    if (!id) return;

    this.optionsSaving.set(true);
    this.optionsError.set(null);

    const payload: QuotationOptionRequest[] = this.options().map((option, index) => ({
      packageId: option.packageId || null,
      optionName: option.optionName,
      destinationId: option.destinationId || null,
      durationDays: option.durationDays || null,
      durationNights: option.durationNights || null,
      hotelCategory: option.hotelCategory || null,
      numberOfPeople: option.numberOfPeople,
      pricePerPerson: option.pricePerPerson,
      isRecommended: option.isRecommended,
      sortOrder: index,
      items: option.items.map((item: QuotationItem, itemIndex: number) => ({
        description: item.description,
        category: item.category || null,
        amount: item.amount,
        sortOrder: itemIndex
      }))
    }));

    this.quotationService.replaceOptions(id, payload).subscribe({
      next: (response) => {
        this.optionsSaving.set(false);
        if (response.success && response.data) {
          this.options.set(response.data);
        }
      },
      error: (error) => {
        this.optionsSaving.set(false);
        this.optionsError.set(error?.error?.message ?? 'Could not save options. Please check the values and try again.');
      }
    });
  }

  send(): void {
    const id = this.quotationId();
    if (!id) return;
    this.sendSaving.set(true);
    this.sendError.set(null);
    this.quotationService.send(id).subscribe({
      next: (response) => {
        this.sendSaving.set(false);
        if (response.success && response.data) this.quotation.set(response.data);
      },
      error: (error) => {
        this.sendSaving.set(false);
        this.sendError.set(error?.error?.message ?? 'Could not send this quotation.');
      }
    });
  }

  regenerateLink(): void {
    const id = this.quotationId();
    if (!id) return;
    this.linkSaving.set(true);
    this.quotationService.regenerateLink(id).subscribe({
      next: (response) => {
        this.linkSaving.set(false);
        if (response.success && response.data?.publicToken) {
          this.lastKnownPublicToken.set(response.data.publicToken);
        }
      },
      error: () => this.linkSaving.set(false)
    });
  }

  get shareUrl(): string | null {
    const token = this.lastKnownPublicToken();
    return token ? `${window.location.origin}/quote/${token}` : null;
  }

  copyShareLink(): void {
    const url = this.shareUrl;
    if (url) {
      navigator.clipboard?.writeText(url);
    }
  }

  whatsAppShareUrl(): string {
    const url = this.shareUrl ?? '';
    const message = `Hi! Here is your travel quotation from One Click Yatra: ${url}`;
    return `https://wa.me/?text=${encodeURIComponent(message)}`;
  }

  downloadPdf(): void {
    const id = this.quotationId();
    if (!id) return;
    this.pdfDownloading.set(true);
    this.quotationService.downloadPdf(id).subscribe({
      next: (blob) => {
        this.pdfDownloading.set(false);
        this.triggerDownload(blob, `${this.quotation()?.quotationNumber ?? 'Quotation'}.pdf`);
      },
      error: () => this.pdfDownloading.set(false)
    });
  }

  private triggerDownload(blob: Blob, fileName: string): void {
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;
    link.click();
    URL.revokeObjectURL(url);
  }

  convertToBooking(): void {
    const id = this.quotationId();
    if (!id) return;
    this.convertSaving.set(true);
    this.convertError.set(null);
    this.bookingService.convertFromQuotation(id).subscribe({
      next: (response) => {
        this.convertSaving.set(false);
        if (response.success && response.data) {
          this.router.navigate(['/admin/bookings', response.data.id]);
        }
      },
      error: (error) => {
        this.convertSaving.set(false);
        this.convertError.set(error?.error?.message ?? 'Could not convert this quotation to a booking.');
      }
    });
  }

  confirmDelete(): void {
    this.deleteConfirmOpen.set(true);
  }

  cancelDelete(): void {
    this.deleteConfirmOpen.set(false);
  }

  performDelete(): void {
    const id = this.quotationId();
    if (!id) return;
    this.quotationService.delete(id).subscribe(() => {
      this.deleteConfirmOpen.set(false);
      this.router.navigate(['/admin/quotations']);
    });
  }
}
