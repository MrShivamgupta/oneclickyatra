import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { VendorPortalService } from '../../../core/services/vendor-portal.service';
import { Vendor } from '../../../core/models/vendor.models';

@Component({
  selector: 'app-vendor-my-profile',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './my-profile.html',
  styleUrl: '../vendor-portal-shared.scss'
})
export class MyProfile {
  private readonly vendorPortalService = inject(VendorPortalService);
  private readonly formBuilder = inject(FormBuilder);

  readonly loading = signal(true);
  readonly loadError = signal<string | null>(null);
  readonly saving = signal(false);
  readonly error = signal<string | null>(null);
  readonly saved = signal(false);
  readonly vendor = signal<Vendor | null>(null);

  readonly form = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
    phone: ['', [Validators.required, Validators.maxLength(30)]],
    address: [''],
    city: [''],
    country: ['']
  });

  constructor() {
    this.vendorPortalService.getProfile().subscribe({
      next: (response) => {
        this.loading.set(false);
        if (response.success && response.data) {
          this.vendor.set(response.data);
          this.form.setValue({
            name: response.data.name,
            email: response.data.email,
            phone: response.data.phone,
            address: response.data.address ?? '',
            city: response.data.city ?? '',
            country: response.data.country ?? ''
          });
        }
      },
      error: (error) => {
        this.loading.set(false);
        this.loadError.set(error?.error?.message ?? 'Could not load your profile. Please try again.');
      }
    });
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.error.set(null);
    this.saved.set(false);
    const raw = this.form.getRawValue();
    const current = this.vendor();

    // VendorType/IsActive stay admin-controlled — the portal PUT only applies the fields below,
    // but the request still needs the DTO's current values for those two.
    this.vendorPortalService
      .updateProfile({
        name: raw.name,
        vendorType: current?.vendorType ?? 'Hotel',
        email: raw.email,
        phone: raw.phone,
        address: raw.address || null,
        city: raw.city || null,
        country: raw.country || null,
        isActive: current?.isActive ?? true
      })
      .subscribe({
        next: (response) => {
          this.saving.set(false);
          this.saved.set(true);
          if (response.success && response.data) this.vendor.set(response.data);
        },
        error: (error) => {
          this.saving.set(false);
          this.error.set(error?.error?.message ?? 'Could not update your profile.');
        }
      });
  }
}
