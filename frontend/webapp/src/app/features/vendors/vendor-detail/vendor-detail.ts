import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ConfirmationDialog } from '../../../shared/components/confirmation-dialog/confirmation-dialog';
import { Pagination } from '../../../shared/components/pagination/pagination';
import { VendorService } from '../../../core/services/vendor.service';
import { DestinationService } from '../../../core/services/destination.service';
import { Destination } from '../../../core/models/master-data.models';
import {
  VENDOR_PAYMENT_STATUSES,
  VENDOR_TYPES,
  Vendor,
  VendorContact,
  VendorInvoice,
  VendorPayment,
  VendorPerformance,
  VendorRate
} from '../../../core/models/vendor.models';

const PAGE_SIZE = 10;
const RATING_OPTIONS = [1, 2, 3, 4, 5];

@Component({
  selector: 'app-vendor-detail',
  standalone: true,
  imports: [ReactiveFormsModule, FormsModule, RouterLink, ConfirmationDialog, Pagination, DatePipe, DecimalPipe],
  templateUrl: './vendor-detail.html',
  styleUrl: './vendor-detail.scss'
})
export class VendorDetail {
  private readonly vendorService = inject(VendorService);
  private readonly destinationService = inject(DestinationService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly vendorTypes = VENDOR_TYPES;
  readonly paymentStatuses = VENDOR_PAYMENT_STATUSES;
  readonly ratingOptions = RATING_OPTIONS;

  readonly vendorId = signal<string | null>(null);
  readonly isNew = signal(false);
  readonly vendor = signal<Vendor | null>(null);
  readonly loading = signal(false);
  readonly deleteConfirmOpen = signal(false);
  readonly deleting = signal(false);
  readonly deleteError = signal<string | null>(null);

  readonly destinations = signal<Destination[]>([]);

  readonly basicError = signal<string | null>(null);
  readonly basicSaving = signal(false);

  readonly contacts = signal<VendorContact[]>([]);
  readonly contactsSaving = signal(false);
  readonly contactsError = signal<string | null>(null);

  readonly rates = signal<VendorRate[]>([]);
  readonly ratesSaving = signal(false);
  readonly ratesError = signal<string | null>(null);

  readonly payments = signal<VendorPayment[]>([]);
  readonly paymentsLoading = signal(false);
  readonly paymentsPageNumber = signal(1);
  readonly paymentsTotalCount = signal(0);
  readonly newPaymentAmount = signal(0);
  readonly newPaymentNotes = signal('');
  readonly paymentSaving = signal(false);
  readonly paymentError = signal<string | null>(null);
  readonly paymentStatusSavingId = signal<string | null>(null);
  readonly paymentStatusError = signal<string | null>(null);

  readonly invoices = signal<VendorInvoice[]>([]);
  readonly invoicesLoading = signal(false);
  readonly invoicesPageNumber = signal(1);
  readonly invoicesTotalCount = signal(0);
  readonly invoiceStatusSavingId = signal<string | null>(null);
  readonly invoiceStatusError = signal<string | null>(null);
  readonly downloadingInvoiceId = signal<string | null>(null);
  readonly invoiceDownloadError = signal<string | null>(null);

  readonly performanceHistory = signal<VendorPerformance[]>([]);
  readonly performanceLoading = signal(false);
  readonly newRating = signal(5);
  readonly newRatingNotes = signal('');
  readonly performanceSaving = signal(false);
  readonly performanceError = signal<string | null>(null);

  readonly linkUserId = signal('');
  readonly linkUserSaving = signal(false);
  readonly linkUserError = signal<string | null>(null);
  readonly linkUserSuccess = signal(false);

  readonly basicForm = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    vendorType: ['Hotel', [Validators.required]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
    phone: ['', [Validators.required, Validators.maxLength(30)]],
    address: [''],
    city: [''],
    country: [''],
    isActive: [true]
  });

  constructor() {
    this.destinationService.search({ pageNumber: 1, pageSize: 100 }).subscribe((response) => {
      if (response.success && response.data) this.destinations.set(response.data.items);
    });

    const idParam = this.route.snapshot.paramMap.get('id');
    if (!idParam || idParam === 'new') {
      this.isNew.set(true);
      return;
    }

    this.vendorId.set(idParam);
    this.load();
  }

  load(): void {
    const id = this.vendorId();
    if (!id) return;
    this.loading.set(true);
    this.vendorService.getById(id).subscribe({
      next: (response) => {
        this.loading.set(false);
        if (response.success && response.data) this.applyVendor(response.data);
      },
      error: () => this.loading.set(false)
    });
    this.loadContacts();
    this.loadRates();
    this.loadPayments();
    this.loadInvoices();
    this.loadPerformanceHistory();
  }

  private applyVendor(vendor: Vendor): void {
    this.vendor.set(vendor);
    this.basicForm.setValue({
      name: vendor.name,
      vendorType: vendor.vendorType,
      email: vendor.email,
      phone: vendor.phone,
      address: vendor.address ?? '',
      city: vendor.city ?? '',
      country: vendor.country ?? '',
      isActive: vendor.isActive
    });
  }

  private buildRequest() {
    const raw = this.basicForm.getRawValue();
    return {
      name: raw.name,
      vendorType: raw.vendorType,
      email: raw.email,
      phone: raw.phone,
      address: raw.address || null,
      city: raw.city || null,
      country: raw.country || null,
      isActive: raw.isActive
    };
  }

  saveBasic(): void {
    if (this.basicForm.invalid) {
      this.basicForm.markAllAsTouched();
      return;
    }

    this.basicSaving.set(true);
    this.basicError.set(null);
    const request = this.buildRequest();

    if (this.isNew()) {
      this.vendorService.create(request).subscribe({
        next: (response) => {
          this.basicSaving.set(false);
          if (response.success && response.data) this.router.navigate(['/admin/vendors', response.data.id]);
        },
        error: (error) => {
          this.basicSaving.set(false);
          this.basicError.set(error?.error?.message ?? 'Something went wrong. Please try again.');
        }
      });
      return;
    }

    this.vendorService.update(this.vendorId()!, request).subscribe({
      next: (response) => {
        this.basicSaving.set(false);
        if (response.success && response.data) this.vendor.set(response.data);
      },
      error: (error) => {
        this.basicSaving.set(false);
        this.basicError.set(error?.error?.message ?? 'Something went wrong. Please try again.');
      }
    });
  }

  linkUser(): void {
    const id = this.vendorId();
    if (!id || !this.linkUserId().trim()) return;

    this.linkUserSaving.set(true);
    this.linkUserError.set(null);
    this.linkUserSuccess.set(false);

    this.vendorService.linkUser(id, { userId: this.linkUserId().trim() }).subscribe({
      next: (response) => {
        this.linkUserSaving.set(false);
        this.linkUserSuccess.set(true);
        if (response.success && response.data) this.vendor.set(response.data);
      },
      error: (error) => {
        this.linkUserSaving.set(false);
        this.linkUserError.set(error?.error?.message ?? 'Could not link this user.');
      }
    });
  }

  loadContacts(): void {
    const id = this.vendorId();
    if (!id) return;
    this.vendorService.getContacts(id).subscribe((response) => {
      if (response.success && response.data) this.contacts.set(response.data.map((c) => ({ ...c })));
    });
  }

  addContact(): void {
    this.contacts.update((current) => [...current, { contactName: '', phone: '', isPrimary: current.length === 0 }]);
  }

  removeContact(index: number): void {
    this.contacts.update((current) => current.filter((_, i) => i !== index));
  }

  saveContacts(): void {
    const id = this.vendorId();
    if (!id) return;
    this.contactsSaving.set(true);
    this.contactsError.set(null);
    this.vendorService.replaceContacts(id, this.contacts()).subscribe({
      next: (response) => {
        this.contactsSaving.set(false);
        if (response.success && response.data) this.contacts.set(response.data);
      },
      error: (error) => {
        this.contactsSaving.set(false);
        this.contactsError.set(error?.error?.message ?? 'Could not save contacts. Please try again.');
      }
    });
  }

  loadRates(): void {
    const id = this.vendorId();
    if (!id) return;
    this.vendorService.getRates(id).subscribe((response) => {
      if (response.success && response.data) this.rates.set(response.data.map((r) => ({ ...r })));
    });
  }

  addRate(): void {
    this.rates.update((current) => [...current, { serviceDescription: '', rateAmount: 0, currency: 'INR' }]);
  }

  removeRate(index: number): void {
    this.rates.update((current) => current.filter((_, i) => i !== index));
  }

  saveRates(): void {
    const id = this.vendorId();
    if (!id) return;
    this.ratesSaving.set(true);
    this.ratesError.set(null);
    this.vendorService.replaceRates(id, this.rates()).subscribe({
      next: (response) => {
        this.ratesSaving.set(false);
        if (response.success && response.data) this.rates.set(response.data);
      },
      error: (error) => {
        this.ratesSaving.set(false);
        this.ratesError.set(error?.error?.message ?? 'Could not save rates. Please try again.');
      }
    });
  }

  loadPayments(): void {
    const id = this.vendorId();
    if (!id) return;
    this.paymentsLoading.set(true);
    this.vendorService.getPayments(id, { pageNumber: this.paymentsPageNumber(), pageSize: PAGE_SIZE }).subscribe({
      next: (response) => {
        this.paymentsLoading.set(false);
        if (response.success && response.data) {
          this.payments.set(response.data.items);
          this.paymentsTotalCount.set(response.data.totalCount);
        }
      },
      error: () => this.paymentsLoading.set(false)
    });
  }

  onPaymentsPageChange(page: number): void {
    this.paymentsPageNumber.set(page);
    this.loadPayments();
  }

  createPayment(): void {
    const id = this.vendorId();
    if (!id || this.newPaymentAmount() <= 0) return;
    this.paymentSaving.set(true);
    this.paymentError.set(null);
    this.vendorService.createPayment(id, { amount: this.newPaymentAmount(), notes: this.newPaymentNotes() || null }).subscribe({
      next: () => {
        this.paymentSaving.set(false);
        this.newPaymentAmount.set(0);
        this.newPaymentNotes.set('');
        this.loadPayments();
      },
      error: (error) => {
        this.paymentSaving.set(false);
        this.paymentError.set(error?.error?.message ?? 'Could not record this payment.');
      }
    });
  }

  updatePaymentStatus(payment: VendorPayment, status: string): void {
    const id = this.vendorId();
    if (!id || status === payment.status) return;
    this.paymentStatusSavingId.set(payment.id);
    this.paymentStatusError.set(null);
    this.vendorService.updatePaymentStatus(id, payment.id, { status }).subscribe({
      next: () => {
        this.paymentStatusSavingId.set(null);
        this.loadPayments();
      },
      error: (error) => {
        this.paymentStatusSavingId.set(null);
        this.paymentStatusError.set(error?.error?.message ?? 'Could not update the payment status. Please try again.');
        // The <select> already shows the unsaved value optimistically — reload to re-sync it
        // with whatever status is actually persisted on the server.
        this.loadPayments();
      }
    });
  }

  loadInvoices(): void {
    const id = this.vendorId();
    if (!id) return;
    this.invoicesLoading.set(true);
    this.vendorService.getInvoices(id, { pageNumber: this.invoicesPageNumber(), pageSize: PAGE_SIZE }).subscribe({
      next: (response) => {
        this.invoicesLoading.set(false);
        if (response.success && response.data) {
          this.invoices.set(response.data.items);
          this.invoicesTotalCount.set(response.data.totalCount);
        }
      },
      error: () => this.invoicesLoading.set(false)
    });
  }

  onInvoicesPageChange(page: number): void {
    this.invoicesPageNumber.set(page);
    this.loadInvoices();
  }

  markInvoiceReviewed(invoice: VendorInvoice): void {
    const id = this.vendorId();
    if (!id || invoice.status === 'Reviewed') return;
    this.invoiceStatusSavingId.set(invoice.id);
    this.invoiceStatusError.set(null);
    this.vendorService.updateInvoiceStatus(id, invoice.id, { status: 'Reviewed' }).subscribe({
      next: () => {
        this.invoiceStatusSavingId.set(null);
        this.loadInvoices();
      },
      error: (error) => {
        this.invoiceStatusSavingId.set(null);
        this.invoiceStatusError.set(error?.error?.message ?? 'Could not update the invoice status. Please try again.');
      }
    });
  }

  downloadInvoice(invoice: VendorInvoice): void {
    this.downloadingInvoiceId.set(invoice.id);
    this.invoiceDownloadError.set(null);
    this.vendorService.downloadInvoice(invoice.id).subscribe({
      next: (blob) => {
        this.downloadingInvoiceId.set(null);
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = invoice.fileName;
        link.click();
        URL.revokeObjectURL(url);
      },
      error: (error) => {
        this.downloadingInvoiceId.set(null);
        this.invoiceDownloadError.set(error?.error?.message ?? 'Could not download this invoice. Please try again.');
      }
    });
  }

  loadPerformanceHistory(): void {
    const id = this.vendorId();
    if (!id) return;
    this.performanceLoading.set(true);
    this.vendorService.getPerformanceHistory(id).subscribe({
      next: (response) => {
        this.performanceLoading.set(false);
        if (response.success && response.data) this.performanceHistory.set(response.data);
      },
      error: () => this.performanceLoading.set(false)
    });
  }

  setNewRating(value: number): void {
    this.newRating.set(value);
  }

  recordPerformance(): void {
    const id = this.vendorId();
    if (!id) return;
    this.performanceSaving.set(true);
    this.performanceError.set(null);
    this.vendorService.recordPerformance(id, { rating: this.newRating(), notes: this.newRatingNotes() || null }).subscribe({
      next: () => {
        this.performanceSaving.set(false);
        this.newRating.set(5);
        this.newRatingNotes.set('');
        this.loadPerformanceHistory();
        // Rating is a recompute-on-write average on the vendor row — reload just the vendor to reflect it.
        this.vendorService.getById(id).subscribe((response) => {
          if (response.success && response.data) this.vendor.set(response.data);
        });
      },
      error: (error) => {
        this.performanceSaving.set(false);
        this.performanceError.set(error?.error?.message ?? 'Could not record this rating.');
      }
    });
  }

  confirmDelete(): void {
    this.deleteError.set(null);
    this.deleteConfirmOpen.set(true);
  }

  cancelDelete(): void {
    this.deleteConfirmOpen.set(false);
  }

  performDelete(): void {
    const id = this.vendorId();
    if (!id) return;
    this.deleting.set(true);
    this.deleteError.set(null);
    this.vendorService.delete(id).subscribe({
      next: () => {
        this.deleting.set(false);
        this.deleteConfirmOpen.set(false);
        this.router.navigate(['/admin/vendors']);
      },
      error: (error) => {
        this.deleting.set(false);
        this.deleteError.set(error?.error?.message ?? 'Could not delete this vendor. Please try again.');
      }
    });
  }
}
