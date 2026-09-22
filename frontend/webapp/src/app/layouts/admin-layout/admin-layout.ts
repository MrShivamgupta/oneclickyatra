import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { forkJoin } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';
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
