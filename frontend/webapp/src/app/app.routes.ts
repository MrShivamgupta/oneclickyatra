import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login').then((m) => m.Login)
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register').then((m) => m.Register)
  },
  {
    path: 'forgot-password',
    loadComponent: () => import('./features/auth/forgot-password/forgot-password').then((m) => m.ForgotPassword)
  },
  {
    path: 'reset-password',
    loadComponent: () => import('./features/auth/reset-password/reset-password').then((m) => m.ResetPassword)
  },
  {
    path: 'portal',
    loadComponent: () => import('./layouts/portal-layout/portal-layout').then((m) => m.PortalLayout),
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/portal/portal-dashboard/portal-dashboard').then((m) => m.PortalDashboard)
      },
      {
        path: 'bookings',
        loadComponent: () => import('./features/portal/my-bookings/my-bookings').then((m) => m.MyBookings)
      },
      {
        path: 'bookings/:id',
        loadComponent: () => import('./features/portal/my-booking-detail/my-booking-detail').then((m) => m.MyBookingDetail)
      },
      {
        path: 'payments',
        loadComponent: () => import('./features/portal/my-payments/my-payments').then((m) => m.MyPayments)
      },
      {
        path: 'invoices',
        loadComponent: () => import('./features/portal/my-invoices/my-invoices').then((m) => m.MyInvoices)
      },
      {
        path: 'quotations',
        loadComponent: () => import('./features/portal/my-quotations/my-quotations').then((m) => m.MyQuotations)
      },
      {
        path: 'profile',
        loadComponent: () => import('./features/portal/my-profile/my-profile').then((m) => m.MyProfile)
      }
    ]
  },
  {
    path: 'vendor-portal',
    loadComponent: () => import('./layouts/vendor-portal-layout/vendor-portal-layout').then((m) => m.VendorPortalLayout),
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/vendor-portal/vendor-portal-dashboard/vendor-portal-dashboard').then((m) => m.VendorPortalDashboard)
      },
      {
        path: 'rates',
        loadComponent: () => import('./features/vendor-portal/my-rates/my-rates').then((m) => m.MyRates)
      },
      {
        path: 'payments',
        loadComponent: () => import('./features/vendor-portal/my-payments/my-payments').then((m) => m.MyPayments)
      },
      {
        path: 'booking-requests',
        loadComponent: () => import('./features/vendor-portal/booking-requests/booking-requests').then((m) => m.BookingRequests)
      },
      {
        path: 'invoices',
        loadComponent: () => import('./features/vendor-portal/my-invoices/my-invoices').then((m) => m.MyInvoices)
      },
      {
        path: 'profile',
        loadComponent: () => import('./features/vendor-portal/my-profile/my-profile').then((m) => m.MyProfile)
      }
    ]
  },
  {
    path: 'admin',
    loadComponent: () => import('./layouts/admin-layout/admin-layout').then((m) => m.AdminLayout),
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard').then((m) => m.Dashboard)
      },
      {
        path: 'destinations',
        loadComponent: () => import('./features/destinations/destinations').then((m) => m.Destinations)
      },
      {
        path: 'master-data',
        loadComponent: () => import('./features/master-data/master-data').then((m) => m.MasterData)
      },
      {
        path: 'packages',
        loadComponent: () => import('./features/packages/packages-list/packages-list').then((m) => m.PackagesList)
      },
      {
        path: 'packages/:id',
        loadComponent: () => import('./features/packages/package-editor/package-editor').then((m) => m.PackageEditor)
      },
      {
        path: 'leads',
        loadComponent: () => import('./features/leads/leads-list/leads-list').then((m) => m.LeadsList)
      },
      {
        path: 'leads/:id',
        loadComponent: () => import('./features/leads/lead-detail/lead-detail').then((m) => m.LeadDetail)
      },
      {
        path: 'customers',
        loadComponent: () => import('./features/customers/customers').then((m) => m.Customers)
      },
      {
        path: 'followups',
        loadComponent: () => import('./features/followups/followups').then((m) => m.FollowUps)
      },
      {
        path: 'quotations',
        loadComponent: () => import('./features/quotations/quotations-list/quotations-list').then((m) => m.QuotationsList)
      },
      {
        path: 'quotations/:id',
        loadComponent: () => import('./features/quotations/quotation-detail/quotation-detail').then((m) => m.QuotationDetailPage)
      },
      {
        path: 'bookings',
        loadComponent: () => import('./features/bookings/bookings-list/bookings-list').then((m) => m.BookingsList)
      },
      {
        path: 'bookings/:id',
        loadComponent: () => import('./features/bookings/booking-detail/booking-detail').then((m) => m.BookingDetail)
      },
      {
        path: 'enquiries',
        loadComponent: () => import('./features/enquiries-admin/enquiries-admin').then((m) => m.EnquiriesAdmin)
      },
      {
        path: 'payments',
        loadComponent: () => import('./features/payments/payments').then((m) => m.Payments)
      },
      {
        path: 'invoices',
        loadComponent: () => import('./features/invoices/invoices').then((m) => m.Invoices)
      },
      {
        path: 'ai-package-builder',
        loadComponent: () => import('./features/admin-placeholder/admin-placeholder').then((m) => m.AdminPlaceholder),
        data: {
          title: 'AI Package Builder',
          icon: '✨',
          description: 'Generate tailored itineraries, costing and quotation drafts from traveler requirements.',
          phase: 'Phase 3 extension'
        }
      },
      {
        path: 'vendors',
        loadComponent: () => import('./features/vendors/vendors-list/vendors-list').then((m) => m.VendorsList)
      },
      {
        path: 'vendors/:id',
        loadComponent: () => import('./features/vendors/vendor-detail/vendor-detail').then((m) => m.VendorDetail)
      },
      {
        path: 'whatsapp',
        loadComponent: () => import('./features/whatsapp/whatsapp-center/whatsapp-center').then((m) => m.WhatsAppCenter)
      },
      {
        path: 'reports',
        loadComponent: () => import('./features/reports/reports-hub/reports-hub').then((m) => m.ReportsHub)
      },
      {
        path: 'reports/:reportName',
        loadComponent: () => import('./features/reports/report-viewer/report-viewer').then((m) => m.ReportViewer)
      },
      {
        path: 'feedback',
        loadComponent: () => import('./features/feedback-manager/feedback-manager').then((m) => m.FeedbackManager)
      },
      {
        path: 'users',
        loadComponent: () => import('./features/users/users').then((m) => m.Users)
      },
      {
        path: 'audit-logs',
        loadComponent: () => import('./features/audit-logs/audit-logs').then((m) => m.AuditLogs)
      },
      {
        path: 'settings',
        loadComponent: () => import('./features/admin-placeholder/admin-placeholder').then((m) => m.AdminPlaceholder),
        data: {
          title: 'Settings',
          icon: '⚙️',
          description: 'Agency profile, email templates, payment gateway keys and system configuration.',
          phase: 'Phase 7'
        }
      }
    ]
  },
  {
    path: '',
    loadComponent: () => import('./layouts/public-layout/public-layout').then((m) => m.PublicLayout),
    children: [
      { path: '', loadComponent: () => import('./features/home/home').then((m) => m.Home) },
      {
        path: 'destinations',
        loadComponent: () => import('./features/destination-list/destination-list').then((m) => m.DestinationList)
      },
      {
        path: 'destinations/:slug',
        loadComponent: () => import('./features/destination-detail/destination-detail').then((m) => m.DestinationDetail)
      },
      {
        path: 'packages',
        loadComponent: () => import('./features/package-search/package-search').then((m) => m.PackageSearch)
      },
      {
        path: 'packages/:slug',
        loadComponent: () => import('./features/package-detail/package-detail').then((m) => m.PackageDetail)
      },
      {
        path: 'enquiry',
        loadComponent: () => import('./features/enquiry/enquiry').then((m) => m.Enquiry)
      },
      {
        path: 'quote/:token',
        loadComponent: () => import('./features/quote-view/quote-view').then((m) => m.QuoteView)
      },
      {
        path: 'about',
        loadComponent: () => import('./features/cms-page/cms-page').then((m) => m.CmsPage),
        data: { slug: 'about' }
      },
      {
        path: 'contact',
        loadComponent: () => import('./features/cms-page/cms-page').then((m) => m.CmsPage),
        data: { slug: 'contact' }
      },
      {
        path: 'terms',
        loadComponent: () => import('./features/cms-page/cms-page').then((m) => m.CmsPage),
        data: { slug: 'terms' }
      },
      {
        path: 'privacy',
        loadComponent: () => import('./features/cms-page/cms-page').then((m) => m.CmsPage),
        data: { slug: 'privacy' }
      }
    ]
  },
  { path: '**', redirectTo: '' }
];
