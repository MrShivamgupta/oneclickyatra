import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { TokenService } from '../../../core/auth/token.service';

const STAFF_ROLES = ['SuperAdmin', 'TravelAgent', 'OperationsStaff', 'Finance'];

// Mirrors RegisterRequestValidator on the backend: 8+ chars, at least one upper, one lower, one digit.
// These are client-side hints only -- the server re-validates and is authoritative.
const PASSWORD_PATTERN = /^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9]).{8,}$/;
const PHONE_PATTERN = /^\+?[0-9]{7,15}$/;

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register.html',
  styleUrl: './register.scss'
})
export class Register {
  private readonly formBuilder = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly tokenService = inject(TokenService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly isSubmitting = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly fieldErrors = signal<string[]>([]);

  readonly form = this.formBuilder.nonNullable.group({
    fullName: ['', [Validators.required, Validators.maxLength(200)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
    phone: ['', [Validators.required, Validators.pattern(PHONE_PATTERN)]],
    password: ['', [Validators.required, Validators.pattern(PASSWORD_PATTERN)]]
  });

  submit(): void {
    if (this.form.invalid || this.isSubmitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);
    this.fieldErrors.set([]);

    this.authService.register(this.form.getRawValue()).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        const isStaff = this.tokenService.roles().some((role) => STAFF_ROLES.includes(role));
        const defaultUrl = isStaff ? '/admin/dashboard' : '/';
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') ?? defaultUrl;
        this.router.navigateByUrl(returnUrl);
      },
      error: (error) => {
        this.isSubmitting.set(false);
        this.errorMessage.set(error?.error?.message ?? 'Registration failed. Please try again.');
        const errors = error?.error?.errors as Record<string, string[]> | null | undefined;
        this.fieldErrors.set(errors ? Object.values(errors).flat() : []);
      }
    });
  }
}
