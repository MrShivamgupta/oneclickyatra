import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { CurrencyService, OfferedCurrency } from '../../core/services/currency.service';

const STAFF_ROLES = ['SuperAdmin', 'TravelAgent', 'OperationsStaff', 'Finance'];

@Component({
  selector: 'app-public-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, FormsModule],
  templateUrl: './public-layout.html',
  styleUrl: './public-layout.scss'
})
export class PublicLayout {
  protected readonly authService = inject(AuthService);
  protected readonly currencyService = inject(CurrencyService);
  readonly mobileNavOpen = signal(false);

  onCurrencyChange(currency: string): void {
    this.currencyService.setSelected(currency as OfferedCurrency);
  }

  isStaff(): boolean {
    const roles = this.authService.currentUser()?.roles ?? [];
    return roles.some((role) => STAFF_ROLES.includes(role));
  }

  toggleMobileNav(): void {
    this.mobileNavOpen.update((open) => !open);
  }

  closeMobileNav(): void {
    this.mobileNavOpen.set(false);
  }

  logout(): void {
    this.authService.logout();
    this.closeMobileNav();
  }
}
