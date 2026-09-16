import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

interface NavItem {
  label: string;
  route: string;
  icon: string;
}

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './admin-layout.html',
  styleUrl: './admin-layout.scss'
})
export class AdminLayout {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly currentUser = this.authService.currentUser;
  readonly sidebarCollapsed = signal(false);

  readonly navItems: NavItem[] = [
    { label: 'Dashboard', route: '/admin/dashboard', icon: '📊' },
    { label: 'Lead Management', route: '/admin/leads', icon: '👤' },
    { label: 'Enquiries', route: '/admin/enquiries', icon: '📥' },
    { label: 'Customer Database', route: '/admin/customers', icon: '🗂️' },
    { label: 'Destinations', route: '/admin/destinations', icon: '📍' },
    { label: 'Tour Packages', route: '/admin/packages', icon: '🧳' },
    { label: 'AI Package Builder', route: '/admin/ai-package-builder', icon: '✨' },
    { label: 'Quotations', route: '/admin/quotations', icon: '📄' },
    { label: 'Bookings', route: '/admin/bookings', icon: '📘' },
    { label: 'Payments', route: '/admin/payments', icon: '💳' },
    { label: 'Invoices', route: '/admin/invoices', icon: '🧾' },
    { label: 'Vendors', route: '/admin/vendors', icon: '🏬' },
    { label: 'Follow-ups', route: '/admin/followups', icon: '📞' },
    { label: 'WhatsApp Center', route: '/admin/whatsapp', icon: '💬' },
    { label: 'Reports & Analytics', route: '/admin/reports', icon: '📈' },
    { label: 'Feedback Manager', route: '/admin/feedback', icon: '⭐' },
    { label: 'Master Data', route: '/admin/master-data', icon: '🌍' },
    { label: 'Settings', route: '/admin/settings', icon: '⚙️' }
  ];

  toggleSidebar(): void {
    this.sidebarCollapsed.update((collapsed) => !collapsed);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
