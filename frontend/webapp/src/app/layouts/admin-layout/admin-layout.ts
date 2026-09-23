import { Component, computed, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { forkJoin } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';
import { TokenService } from '../../core/auth/token.service';
import { LeadService } from '../../core/services/lead.service';
import { CustomerService } from '../../core/services/customer.service';
import { BookingService } from '../../core/services/booking.service';
import { InvoiceService } from '../../core/services/invoice.service';
import { Lead, Customer } from '../../core/models/crm.models';
import { Booking } from '../../core/models/booking.models';
import { Invoice } from '../../core/models/payment.models';
import { Icon, IconName } from '../../shared/components/icon/icon';

interface NavItem {
  label: string;
  route: string;
  icon: IconName;
  /** Matches the PermissionConstants key gating this page's route and API calls (see
   * permission.guard.ts / [HasPermission] backend attributes) — a nav item only renders when the
   * signed-in user's role actually holds this permission, so the sidebar never links to a page
   * that would 403/redirect-to-forbidden anyway. */
  permission: string;
}

interface GlobalSearchResults {
  leads: Lead[];
  customers: Customer[];
  bookings: Booking[];
  invoices: Invoice[];
}

const EMPTY_GLOBAL_SEARCH_RESULTS: GlobalSearchResults = { leads: [], customers: [], bookings: [], invoices: [] };

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, Icon],
  templateUrl: './admin-layout.html',
  styleUrl: './admin-layout.scss',
  host: {
    '(document:click)': 'onDocumentClick($event)'
  }
})
export class AdminLayout {
  private readonly authService = inject(AuthService);
  private readonly tokenService = inject(TokenService);
  private readonly router = inject(Router);
  private readonly leadService = inject(LeadService);
  private readonly customerService = inject(CustomerService);
  private readonly bookingService = inject(BookingService);
  private readonly invoiceService = inject(InvoiceService);

  readonly currentUser = this.authService.currentUser;
  readonly sidebarCollapsed = signal(false);

  // Global topbar search -- was a purely decorative input with no binding at all before this; now
  // fans out to the 4 entity types its own placeholder already promised, reusing each one's existing
  // searchTerm-backed endpoint rather than building a new cross-entity backend search.
  readonly globalSearchTerm = signal('');
  readonly globalSearchOpen = signal(false);
  readonly globalSearchLoading = signal(false);
  readonly globalSearchResults = signal<GlobalSearchResults>(EMPTY_GLOBAL_SEARCH_RESULTS);
  private globalSearchDebounce: ReturnType<typeof setTimeout> | undefined;

  // Icons are a small, hand-picked SVG set (shared/components/icon), not emoji -- emoji render
  // inconsistently across platforms/fonts and can't be sized/colored via CSS the way a structural
  // UI icon needs to be.
  readonly navItems: NavItem[] = [
    { label: 'Dashboard', route: '/admin/dashboard', icon: 'grid', permission: 'dashboard.view' },
    { label: 'Lead Management', route: '/admin/leads', icon: 'user', permission: 'lead.view' },
    { label: 'Enquiries', route: '/admin/enquiries', icon: 'inbox', permission: 'enquiry.view' },
    { label: 'Customer Database', route: '/admin/customers', icon: 'users', permission: 'customer.view' },
    { label: 'Destinations', route: '/admin/destinations', icon: 'map-pin', permission: 'destination.view' },
    { label: 'Tour Packages', route: '/admin/packages', icon: 'package', permission: 'package.view' },
    { label: 'AI Package Builder', route: '/admin/ai-package-builder', icon: 'sparkles', permission: 'package.view' },
    { label: 'Quotations', route: '/admin/quotations', icon: 'file-text', permission: 'quotation.view' },
    { label: 'Bookings', route: '/admin/bookings', icon: 'calendar-check', permission: 'booking.view' },
    { label: 'Payments', route: '/admin/payments', icon: 'credit-card', permission: 'payment.view' },
    { label: 'Invoices', route: '/admin/invoices', icon: 'receipt', permission: 'payment.view' },
    { label: 'Vendors', route: '/admin/vendors', icon: 'briefcase', permission: 'vendor.view' },
    { label: 'Follow-ups', route: '/admin/followups', icon: 'phone', permission: 'followup.view' },
    { label: 'WhatsApp Center', route: '/admin/whatsapp', icon: 'message-circle', permission: 'whatsapp.manage' },
    { label: 'Reports & Analytics', route: '/admin/reports', icon: 'bar-chart', permission: 'report.view' },
    { label: 'Feedback Manager', route: '/admin/feedback', icon: 'star', permission: 'feedback.view' },
    { label: 'Master Data', route: '/admin/master-data', icon: 'database', permission: 'masterdata.view' },
    { label: 'Users', route: '/admin/users', icon: 'id-badge', permission: 'user.manage' },
    { label: 'Roles & Permissions', route: '/admin/roles', icon: 'shield', permission: 'role.manage' },
    { label: 'Audit Logs', route: '/admin/audit-logs', icon: 'clipboard-list', permission: 'audit.view' },
    { label: 'Settings', route: '/admin/settings', icon: 'settings', permission: 'settings.manage' }
  ];

  readonly visibleNavItems = computed(() => this.navItems.filter((item) => this.tokenService.hasPermission(item.permission)));

  toggleSidebar(): void {
    this.sidebarCollapsed.update((collapsed) => !collapsed);
  }

  onGlobalSearchInput(term: string): void {
    this.globalSearchTerm.set(term);
    clearTimeout(this.globalSearchDebounce);

    const trimmed = term.trim();
    if (trimmed.length < 2) {
      this.globalSearchOpen.set(false);
      this.globalSearchResults.set(EMPTY_GLOBAL_SEARCH_RESULTS);
      return;
    }

    this.globalSearchDebounce = setTimeout(() => this.runGlobalSearch(trimmed), 350);
  }

  onGlobalSearchFocus(): void {
    if (this.globalSearchTerm().trim().length >= 2) {
      this.globalSearchOpen.set(true);
    }
  }

  closeGlobalSearch(): void {
    this.globalSearchOpen.set(false);
  }

  goToResult(path: (string | undefined)[]): void {
    this.globalSearchOpen.set(false);
    this.globalSearchTerm.set('');
    this.router.navigate(path);
  }

  onDocumentClick(event: MouseEvent): void {
    if (!this.globalSearchOpen()) return;
    const target = event.target as HTMLElement;
    if (!target.closest('.search-wrap')) {
      this.globalSearchOpen.set(false);
    }
  }

  private runGlobalSearch(term: string): void {
    this.globalSearchLoading.set(true);
    this.globalSearchOpen.set(true);

    forkJoin({
      leads: this.leadService.search({ pageNumber: 1, pageSize: 5, searchTerm: term }),
      customers: this.customerService.list({ pageNumber: 1, pageSize: 5, searchTerm: term }),
      bookings: this.bookingService.search({ pageNumber: 1, pageSize: 5, searchTerm: term }),
      invoices: this.invoiceService.search({ pageNumber: 1, pageSize: 5, searchTerm: term })
    }).subscribe({
      next: (r) => {
        this.globalSearchLoading.set(false);
        this.globalSearchResults.set({
          leads: r.leads.success && r.leads.data ? r.leads.data.items : [],
          customers: r.customers.success && r.customers.data ? r.customers.data.items : [],
          bookings: r.bookings.success && r.bookings.data ? r.bookings.data.items : [],
          invoices: r.invoices.success && r.invoices.data ? r.invoices.data.items : []
        });
      },
      error: () => {
        this.globalSearchLoading.set(false);
        this.globalSearchResults.set(EMPTY_GLOBAL_SEARCH_RESULTS);
      }
    });
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
