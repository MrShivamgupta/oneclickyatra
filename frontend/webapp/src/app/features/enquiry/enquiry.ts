import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { EnquiryService } from '../../core/services/enquiry.service';
import { DestinationService } from '../../core/services/destination.service';
import { Destination } from '../../core/models/master-data.models';

const PHONE_PATTERN = /^[0-9+\-\s()]{7,20}$/;

@Component({
  selector: 'app-enquiry',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './enquiry.html',
  styleUrl: './enquiry.scss'
})
export class Enquiry {
  private readonly formBuilder = inject(FormBuilder);
  private readonly enquiryService = inject(EnquiryService);
  private readonly destinationService = inject(DestinationService);
  private readonly route = inject(ActivatedRoute);

  readonly destinations = signal<Destination[]>([]);
  readonly loadingDestinations = signal(false);
  readonly destinationsError = signal(false);

  readonly isSubmitting = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly submitted = signal(false);

  readonly form = this.formBuilder.nonNullable.group({
    fullName: ['', [Validators.required, Validators.maxLength(150)]],
    email: ['', [Validators.required, Validators.email]],
    phone: ['', [Validators.required, Validators.pattern(PHONE_PATTERN)]],
    destinationId: [''],
    travelDate: [''],
    message: ['', [Validators.required, Validators.maxLength(2000)]]
  });

  constructor() {
    const queryParams = this.route.snapshot.queryParamMap;
    const preselectedDestinationId = queryParams.get('destinationId');
    if (preselectedDestinationId) {
      this.form.controls.destinationId.setValue(preselectedDestinationId);
    }

    const travelDate = queryParams.get('travelDate');
    if (travelDate) {
      this.form.controls.travelDate.setValue(travelDate);
    }

    const packageTitle = queryParams.get('packageTitle');
    const travelers = queryParams.get('travelers');
    const messageParts: string[] = [];
    if (packageTitle) {
      messageParts.push(`I'm interested in the "${packageTitle}" package.`);
    }
    if (travelers) {
      messageParts.push(`We are ${travelers} traveler(s).`);
    }
    if (messageParts.length > 0) {
      this.form.controls.message.setValue(`${messageParts.join(' ')} Please share availability and pricing.`);
    }

    this.loadDestinations();
  }

  private loadDestinations(): void {
    this.loadingDestinations.set(true);
    this.destinationsError.set(false);
    this.destinationService.search({ pageNumber: 1, pageSize: 100, isPublished: true }).subscribe({
      next: (response) => {
        this.loadingDestinations.set(false);
        if (response.success && response.data) {
          this.destinations.set(response.data.items);
        }
      },
      error: () => {
        this.loadingDestinations.set(false);
        this.destinationsError.set(true);
      }
    });
  }

  submit(): void {
    if (this.form.invalid || this.isSubmitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    const raw = this.form.getRawValue();
    const request = {
      fullName: raw.fullName,
      email: raw.email,
      phone: raw.phone,
      destinationId: raw.destinationId || undefined,
      travelDate: raw.travelDate || undefined,
      message: raw.message
    };

    this.enquiryService.submit(request).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.submitted.set(true);
      },
      error: (error) => {
        this.isSubmitting.set(false);
        const status = error?.status;
        const apiMessage = error?.error?.message;
        if (status === 429) {
          this.errorMessage.set(
            apiMessage ?? 'You have sent too many enquiries in a short time. Please wait a minute and try again.'
          );
          return;
        }
        this.errorMessage.set(apiMessage ?? 'Something went wrong. Please try again.');
      }
    });
  }

  submitAnother(): void {
    this.submitted.set(false);
    this.errorMessage.set(null);
    this.form.reset({ fullName: '', email: '', phone: '', destinationId: '', travelDate: '', message: '' });
  }
}
