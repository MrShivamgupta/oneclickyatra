import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { Icon, IconName } from '../../shared/components/icon/icon';

interface NavItem {
  label: string;
  route: string;
  icon: IconName;
}

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, Icon],
  templateUrl: './admin-layout.html',
  styleUrl: './admin-layout.scss'
})
export class AdminLayout {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly currentUser = this.authService.currentUser;
  readonly sidebarCollapsed = signal(false);

  // Icons are a small, hand-picked SVG set (shared/components/icon), not emoji -- emoji render
  // inconsistently across platforms/fonts and can't be sized/colored via CSS the way a structural
  // UI icon needs to be.
  readonly navItems: NavItem[] = [
    { label: 'Dashboard', route: '/admin/dashboard', icon: 'grid' },
    { label: 'Lead Management', route: '/admin/leads', icon: 'user' },
    { label: 'Enquiries', route: '/admin/enquiries', icon: 'inbox' },
    { label: 'Customer Database', route: '/admin/customers', icon: 'users' },
    { label: 'Destinations', route: '/admin/destinations', icon: 'map-pin' },
    { label: 'Tour Packages', route: '/admin/packages', icon: 'package' },
    { label: 'AI Package Builder', route: '/admin/ai-package-builder', icon: 'sparkles' },
    { label: 'Quotations', route: '/admin/quotations', icon: 'file-text' },
    { label: 'Bookings', route: '/admin/bookings', icon: 'calendar-check' },
    { label: 'Payments', route: '/admin/payments', icon: 'credit-card' },
    { label: 'Invoices', route: '/admin/invoices', icon: 'receipt' },
    { label: 'Vendors', route: '/admin/vendors', icon: 'briefcase' },
    { label: 'Follow-ups', route: '/admin/followups', icon: 'phone' },
    { label: 'WhatsApp Center', route: '/admin/whatsapp', icon: 'message-circle' },
    { label: 'Reports & Analytics', route: '/admin/reports', icon: 'bar-chart' },
    { label: 'Feedback Manager', route: '/admin/feedback', icon: 'star' },
    { label: 'Master Data', route: '/admin/master-data', icon: 'database' },
    { label: 'Users', route: '/admin/users', icon: 'id-badge' },
    { label: 'Audit Logs', route: '/admin/audit-logs', icon: 'clipboard-list' },
    { label: 'Settings', route: '/admin/settings', icon: 'settings' }
  ];

  toggleSidebar(): void {
    this.sidebarCollapsed.update((collapsed) => !collapsed);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
