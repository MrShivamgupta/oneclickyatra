import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';

// Mirrors ResetPasswordRequestValidator on the backend: 8+ chars, at least one upper, one lower, one digit.
const PASSWORD_PATTERN = /^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9]).{8,}$/;

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './reset-password.html',
  styleUrl: './reset-password.scss'
})
export class ResetPassword {
  private readonly formBuilder = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly isSubmitting = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly form = this.formBuilder.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    resetToken: ['', [Validators.required]],
    newPassword: ['', [Validators.required, Validators.pattern(PASSWORD_PATTERN)]]
  });

  constructor() {
    // Dev convenience: allow the email to be prefilled via ?email= since there is no
    // clickable emailed link wired up yet in this environment.
    const queryParams = this.route.snapshot.queryParamMap;
    const emailFromQuery = queryParams.get('email');
    if (emailFromQuery) {
      this.form.controls.email.setValue(emailFromQuery);
    }

    const tokenFromQuery = queryParams.get('token');
    if (tokenFromQuery) {
      this.form.controls.resetToken.setValue(tokenFromQuery);
    }
  }

  submit(): void {
    if (this.form.invalid || this.isSubmitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    const { email, resetToken, newPassword } = this.form.getRawValue();

    this.authService.resetPassword(email, resetToken, newPassword).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.router.navigate(['/login'], { queryParams: { reset: 'success' } });
      },
      error: (error) => {
        this.isSubmitting.set(false);
        this.errorMessage.set(error?.error?.message ?? 'Could not reset your password. Please try again.');
      }
    });
  }
}
