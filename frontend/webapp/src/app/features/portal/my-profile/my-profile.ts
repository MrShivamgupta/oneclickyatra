import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { PortalService } from '../../../core/services/portal.service';

@Component({
  selector: 'app-my-profile',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './my-profile.html',
  styleUrl: '../portal-shared.scss'
})
export class MyProfile {
  private readonly portalService = inject(PortalService);
  private readonly formBuilder = inject(FormBuilder);

  readonly loading = signal(true);
  readonly loadError = signal<string | null>(null);
  readonly saving = signal(false);
  readonly error = signal<string | null>(null);
  readonly saved = signal(false);

  readonly form = this.formBuilder.nonNullable.group({
    fullName: ['', [Validators.required, Validators.maxLength(150)]],
    email: [''],
    phone: ['', Validators.required]
  });

  constructor() {
    this.portalService.getProfile().subscribe({
      next: (response) => {
        this.loading.set(false);
        if (response.success && response.data) {
          this.form.setValue({
            fullName: response.data.fullName,
            email: response.data.email ?? '',
            phone: response.data.phone
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

    this.portalService.updateProfile({ fullName: raw.fullName, email: raw.email || null, phone: raw.phone }).subscribe({
      next: () => {
        this.saving.set(false);
        this.saved.set(true);
      },
      error: (error) => {
        this.saving.set(false);
        this.error.set(error?.error?.message ?? 'Could not update your profile.');
      }
    });
  }
}
