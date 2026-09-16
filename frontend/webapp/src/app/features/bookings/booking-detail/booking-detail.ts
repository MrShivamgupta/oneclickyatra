import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ConfirmationDialog } from '../../../shared/components/confirmation-dialog/confirmation-dialog';
import { BookingService } from '../../../core/services/booking.service';
import { PaymentService } from '../../../core/services/payment.service';
import { CustomerService } from '../../../core/services/customer.service';
import { DestinationService } from '../../../core/services/destination.service';
import { PackageService } from '../../../core/services/package.service';
import { Customer } from '../../../core/models/crm.models';
import { Destination } from '../../../core/models/master-data.models';
import { PackageSummary } from '../../../core/models/package.models';
import {
  BOOKING_ALLOWED_TRANSITIONS,
  Booking,
  BookingAddOn,
  BookingPassenger,
  BookingStatus,
  BookingStatusHistoryEntry
} from '../../../core/models/booking.models';

const EDITABLE_STATUSES: BookingStatus[] = ['Draft', 'Quoted', 'PendingPayment', 'Confirmed', 'InProgress'];
const RAZORPAY_CHECKOUT_SCRIPT_URL = 'https://checkout.razorpay.com/v1/checkout.js';

type ReasonAction = 'cancel' | 'refund' | null;

declare const Razorpay: new (options: Record<string, unknown>) => { open(): void };

@Component({
  selector: 'app-booking-detail',
  standalone: true,
  imports: [ReactiveFormsModule, FormsModule, RouterLink, ConfirmationDialog, DatePipe, DecimalPipe],
  templateUrl: './booking-detail.html',
  styleUrl: './booking-detail.scss'
})
export class BookingDetail {
  private readonly bookingService = inject(BookingService);
  private readonly paymentService = inject(PaymentService);
  private readonly customerService = inject(CustomerService);
  private readonly destinationService = inject(DestinationService);
  private readonly packageService = inject(PackageService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly bookingId = signal<string | null>(null);
  readonly isNew = signal(false);
  readonly booking = signal<Booking | null>(null);
  readonly loading = signal(false);

  readonly customers = signal<Customer[]>([]);
  readonly destinations = signal<Destination[]>([]);
  readonly packages = signal<PackageSummary[]>([]);
  readonly selectedCustomerId = signal('');

  readonly passengers = signal<BookingPassenger[]>([]);
  readonly addOns = signal<BookingAddOn[]>([]);
  readonly history = signal<BookingStatusHistoryEntry[]>([]);

  readonly basicError = signal<string | null>(null);
  readonly basicSaving = signal(false);
  readonly passengersSaving = signal(false);
  readonly addOnsSaving = signal(false);
  readonly transitionSaving = signal(false);
  readonly transitionError = signal<string | null>(null);
  readonly deleteConfirmOpen = signal(false);

  readonly reasonAction = signal<ReasonAction>(null);
  readonly reasonText = signal('');

  readonly paymentSaving = signal(false);
  readonly paymentError = signal<string | null>(null);
  readonly gatewayRefundSaving = signal(false);
  readonly gatewayRefundError = signal<string | null>(null);

  readonly basicForm = this.formBuilder.nonNullable.group({
    packageId: [''],
    destinationId: [''],
    travelDate: [''],
    returnDate: [''],
    numberOfAdults: [1, [Validators.required, Validators.min(1)]],
    numberOfChildren: [0, [Validators.required, Validators.min(0)]],
    totalAmount: [0, [Validators.required, Validators.min(0)]],
    notes: ['']
  });

  constructor() {
    this.destinationService.search({ pageNumber: 1, pageSize: 100 }).subscribe((response) => {
      if (response.success && response.data) this.destinations.set(response.data.items);
    });
    this.packageService.search({ pageNumber: 1, pageSize: 100 }).subscribe((response) => {
      if (response.success && response.data) this.packages.set(response.data.items);
    });

    const idParam = this.route.snapshot.paramMap.get('id');
    if (!idParam || idParam === 'new') {
      this.isNew.set(true);
      this.customerService.list({ pageNumber: 1, pageSize: 100 }).subscribe((response) => {
        if (response.success && response.data) this.customers.set(response.data.items);
      });
      return;
    }

    this.bookingId.set(idParam);
    this.load();
  }

  get allowedNextStatuses(): BookingStatus[] {
    const status = this.booking()?.status;
    if (!status) return [];
    return BOOKING_ALLOWED_TRANSITIONS[status].filter((s) => s !== 'Cancelled' && s !== 'RefundPending');
  }

  get canCancel(): boolean {
    const status = this.booking()?.status;
    return !!status && BOOKING_ALLOWED_TRANSITIONS[status].includes('Cancelled');
  }

  get canRefund(): boolean {
    const status = this.booking()?.status;
    return !!status && BOOKING_ALLOWED_TRANSITIONS[status].includes('RefundPending');
  }

  get isEditable(): boolean {
    const status = this.booking()?.status;
    return this.isNew() || (!!status && EDITABLE_STATUSES.includes(status));
  }

  get isDeletable(): boolean {
    return this.booking()?.status === 'Draft';
  }

  load(): void {
    const id = this.bookingId();
    if (!id) return;
    this.loading.set(true);
    this.bookingService.getById(id).subscribe({
      next: (response) => {
        this.loading.set(false);
        if (response.success && response.data) {
          this.applyDetail(response.data.booking, response.data.passengers, response.data.addOns);
        }
      },
      error: () => this.loading.set(false)
    });
    this.loadHistory();
  }

  private applyDetail(booking: Booking, passengers: BookingPassenger[], addOns: BookingAddOn[]): void {
    this.booking.set(booking);
    this.basicForm.setValue({
      packageId: booking.packageId ?? '',
      destinationId: booking.destinationId ?? '',
      travelDate: booking.travelDate ?? '',
      returnDate: booking.returnDate ?? '',
      numberOfAdults: booking.numberOfAdults,
      numberOfChildren: booking.numberOfChildren,
      totalAmount: booking.totalAmount,
      notes: booking.notes ?? ''
    });
    this.passengers.set(passengers.map((p) => ({ ...p })));
    this.addOns.set(addOns.map((a) => ({ ...a })));
  }

  loadHistory(): void {
    const id = this.bookingId();
    if (!id) return;
    this.bookingService.getHistory(id).subscribe((response) => {
      if (response.success && response.data) this.history.set(response.data);
    });
  }

  private buildRequest() {
    const raw = this.basicForm.getRawValue();
    return {
      customerId: this.isNew() ? this.selectedCustomerId() : this.booking()!.customerId,
      leadId: this.isNew() ? null : this.booking()!.leadId,
      packageId: raw.packageId || null,
      destinationId: raw.destinationId || null,
      travelDate: raw.travelDate || null,
      returnDate: raw.returnDate || null,
      numberOfAdults: raw.numberOfAdults,
      numberOfChildren: raw.numberOfChildren,
      totalAmount: raw.totalAmount,
      notes: raw.notes || null
    };
  }

  saveBasic(): void {
    if (this.basicForm.invalid) {
      this.basicForm.markAllAsTouched();
      return;
    }
    if (this.isNew() && !this.selectedCustomerId()) {
      this.basicError.set('Please select a customer.');
      return;
    }

    this.basicSaving.set(true);
    this.basicError.set(null);
    const request = this.buildRequest();

    if (this.isNew()) {
      this.bookingService.create(request).subscribe({
        next: (response) => {
          this.basicSaving.set(false);
          if (response.success && response.data) this.router.navigate(['/admin/bookings', response.data.id]);
        },
        error: (error) => {
          this.basicSaving.set(false);
          this.basicError.set(error?.error?.message ?? 'Something went wrong. Please try again.');
        }
      });
      return;
    }

    this.bookingService.update(this.bookingId()!, request).subscribe({
      next: (response) => {
        this.basicSaving.set(false);
        if (response.success && response.data) this.booking.set(response.data);
      },
      error: (error) => {
        this.basicSaving.set(false);
        this.basicError.set(error?.error?.message ?? 'Something went wrong. Please try again.');
      }
    });
  }

  transitionTo(status: BookingStatus): void {
    const id = this.bookingId();
    if (!id) return;
    this.transitionSaving.set(true);
    this.transitionError.set(null);
    this.bookingService.updateStatus(id, status).subscribe({
      next: (response) => {
        this.transitionSaving.set(false);
        if (response.success && response.data) this.booking.set(response.data);
        this.loadHistory();
      },
      error: (error) => {
        this.transitionSaving.set(false);
        this.transitionError.set(error?.error?.message ?? 'Could not update the booking status.');
      }
    });
  }

  startReasonAction(action: 'cancel' | 'refund'): void {
    this.reasonAction.set(action);
    this.reasonText.set('');
    this.transitionError.set(null);
  }

  cancelReasonAction(): void {
    this.reasonAction.set(null);
  }

  confirmReasonAction(): void {
    const id = this.bookingId();
    const action = this.reasonAction();
    if (!id || !action) return;

    this.transitionSaving.set(true);
    this.transitionError.set(null);
    const call$ = action === 'cancel' ? this.bookingService.cancel(id, this.reasonText() || null) : this.bookingService.refund(id, this.reasonText() || null);

    call$.subscribe({
      next: (response) => {
        this.transitionSaving.set(false);
        this.reasonAction.set(null);
        if (response.success && response.data) this.booking.set(response.data);
        this.loadHistory();
      },
      error: (error) => {
        this.transitionSaving.set(false);
        this.transitionError.set(error?.error?.message ?? 'Could not complete this action.');
      }
    });
  }

  collectPayment(): void {
    const id = this.bookingId();
    if (!id) return;
    this.paymentSaving.set(true);
    this.paymentError.set(null);

    this.paymentService.initiate(id).subscribe({
      next: (response) => {
        if (!response.success || !response.data) {
          this.paymentSaving.set(false);
          return;
        }
        const order = response.data;
        this.loadRazorpayScript()
          .then(() => {
            this.paymentSaving.set(false);
            const checkout = new Razorpay({
              key: order.gatewayKeyId,
              amount: Math.round(order.amount * 100),
              currency: order.currency,
              order_id: order.gatewayOrderId,
              name: 'One Click Yatra',
              description: `Booking ${this.booking()?.bookingNumber ?? ''}`,
              // The booking is confirmed by the server once the webhook arrives, not by this
              // callback — reloading here just reflects whatever the server already knows.
              handler: () => this.load()
            });
            checkout.open();
          })
          .catch(() => {
            this.paymentSaving.set(false);
            this.paymentError.set('Could not load the payment widget. Please try again.');
          });
      },
      error: (error) => {
        this.paymentSaving.set(false);
        this.paymentError.set(error?.error?.message ?? 'Could not start the payment.');
      }
    });
  }

  processGatewayRefund(): void {
    const id = this.bookingId();
    if (!id) return;
    this.gatewayRefundSaving.set(true);
    this.gatewayRefundError.set(null);

    this.paymentService.refund(id, this.booking()?.cancellationReason ?? null).subscribe({
      next: () => {
        this.gatewayRefundSaving.set(false);
        this.load();
      },
      error: (error) => {
        this.gatewayRefundSaving.set(false);
        this.gatewayRefundError.set(error?.error?.message ?? 'Could not process the refund.');
      }
    });
  }

  private loadRazorpayScript(): Promise<void> {
    if (typeof Razorpay !== 'undefined') {
      return Promise.resolve();
    }
    return new Promise((resolve, reject) => {
      const script = document.createElement('script');
      script.src = RAZORPAY_CHECKOUT_SCRIPT_URL;
      script.onload = () => resolve();
      script.onerror = () => reject(new Error('Failed to load Razorpay checkout script.'));
      document.body.appendChild(script);
    });
  }

  addPassenger(): void {
    this.passengers.update((current) => [...current, { fullName: '', isLeadPassenger: current.length === 0 }]);
  }

  removePassenger(index: number): void {
    this.passengers.update((current) => current.filter((_, i) => i !== index));
  }

  savePassengers(): void {
    const id = this.bookingId();
    if (!id) return;
    this.passengersSaving.set(true);
    this.bookingService.replacePassengers(id, this.passengers()).subscribe({
      next: (response) => {
        this.passengersSaving.set(false);
        if (response.success && response.data) this.passengers.set(response.data);
      },
      error: () => this.passengersSaving.set(false)
    });
  }

  addAddOn(): void {
    this.addOns.update((current) => [...current, { name: '', price: 0, quantity: 1 }]);
  }

  removeAddOn(index: number): void {
    this.addOns.update((current) => current.filter((_, i) => i !== index));
  }

  saveAddOns(): void {
    const id = this.bookingId();
    if (!id) return;
    this.addOnsSaving.set(true);
    this.bookingService.replaceAddOns(id, this.addOns()).subscribe({
      next: (response) => {
        this.addOnsSaving.set(false);
        if (response.success && response.data) this.addOns.set(response.data);
      },
      error: () => this.addOnsSaving.set(false)
    });
  }

  confirmDelete(): void {
    this.deleteConfirmOpen.set(true);
  }

  cancelDelete(): void {
    this.deleteConfirmOpen.set(false);
  }

  performDelete(): void {
    const id = this.bookingId();
    if (!id) return;
    this.bookingService.delete(id).subscribe(() => {
      this.deleteConfirmOpen.set(false);
      this.router.navigate(['/admin/bookings']);
    });
  }
}
