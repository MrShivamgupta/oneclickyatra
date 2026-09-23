import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Spinner } from '../../../shared/components/spinner/spinner';
import { AgencyProfileService } from '../../../core/services/agency-profile.service';

@Component({
  selector: 'app-agency-profile-tab',
  standalone: true,
  imports: [ReactiveFormsModule, Spinner],
  templateUrl: './agency-profile-tab.html',
  styleUrl: './agency-profile-tab.scss'
})
export class AgencyProfileTab {
  private readonly agencyProfileService = inject(AgencyProfileService);
  private readonly formBuilder = inject(FormBuilder);

  readonly loading = signal(false);
  readonly loadError = signal<string | null>(null);
  readonly saving = signal(false);
  readonly saveError = signal<string | null>(null);
  readonly saveSuccess = signal(false);
  readonly logoPreviewError = signal(false);

  readonly form = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    logoUrl: [''],
    address: [''],
    gstNumber: [''],
    currency: ['INR', [Validators.required, Validators.maxLength(10)]],
    supportEmail: ['', Validators.email],
    supportPhone: ['']
  });

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.loadError.set(null);
    this.agencyProfileService.get().subscribe({
      next: (response) => {
        this.loading.set(false);
        if (response.success && response.data) {
          const profile = response.data;
          this.form.setValue({
            name: profile.name,
            logoUrl: profile.logoUrl ?? '',
            address: profile.address ?? '',
            gstNumber: profile.gstNumber ?? '',
            currency: profile.currency,
            supportEmail: profile.supportEmail ?? '',
            supportPhone: profile.supportPhone ?? ''
          });
          this.logoPreviewError.set(false);
        } else {
          this.loadError.set(response.message || 'Could not load the agency profile.');
        }
      },
      error: (error) => {
        this.loading.set(false);
        this.loadError.set(error?.error?.message ?? 'Could not load the agency profile. Please try again.');
      }
    });
  }

  onLogoUrlChange(): void {
    this.logoPreviewError.set(false);
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.saveError.set(null);
    this.saveSuccess.set(false);
    const raw = this.form.getRawValue();

    this.agencyProfileService
      .update({
        name: raw.name,
        logoUrl: raw.logoUrl || null,
        address: raw.address || null,
        gstNumber: raw.gstNumber || null,
        currency: raw.currency,
        supportEmail: raw.supportEmail || null,
        supportPhone: raw.supportPhone || null
      })
      .subscribe({
        next: (response) => {
          this.saving.set(false);
          if (response.success) {
            this.saveSuccess.set(true);
          } else {
            this.saveError.set(response.message || 'Could not save the agency profile.');
          }
        },
        error: (error) => {
          this.saving.set(false);
          this.saveError.set(error?.error?.message ?? 'Could not save the agency profile. Please try again.');
        }
      });
  }
}
