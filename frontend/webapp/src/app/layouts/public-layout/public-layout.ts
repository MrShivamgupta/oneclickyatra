import { Component, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

const STAFF_ROLES = ['SuperAdmin', 'TravelAgent', 'OperationsStaff', 'Finance'];

@Component({
  selector: 'app-public-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './public-layout.html',
  styleUrl: './public-layout.scss'
})
export class PublicLayout {
  protected readonly authService = inject(AuthService);
  readonly mobileNavOpen = signal(false);

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
