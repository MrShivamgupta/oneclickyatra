// Mirrors backend/OneClickYatra.Api/Models/Responses/ReportResponses.cs and
// Models/Requests/ReportRequests.cs field-for-field (camelCase). One row interface per report —
// each report has a genuinely different shape by design (see ReportsController.cs).

export type ExportFormat = 'csv' | 'xlsx' | 'pdf';

export const EXPORT_FORMATS: ExportFormat[] = ['csv', 'xlsx', 'pdf'];

export const REPORT_NAMES = [
  'sales',
  'revenue',
  'cancellation',
  'agent-commission',
  'lead-conversion',
  'destination-sales',
  'collection',
  'outstanding',
  'profitability',
  'active-bookings',
  'employee-productivity',
  'monthly-growth',
  'vendor-performance'
] as const;

export type ReportName = (typeof REPORT_NAMES)[number];

export type ReportGroupBy = 'day' | 'week' | 'month';

/** Query params for the reports that scope by a from/to date — mirrors ReportDateRangeRequest. */
export interface ReportDateRangeParams {
  fromDate?: string;
  toDate?: string;
}

/** Query params for Revenue and Monthly Growth — mirrors ReportGroupedDateRangeRequest. */
export interface ReportGroupedDateRangeParams extends ReportDateRangeParams {
  groupBy?: ReportGroupBy;
}

/** A row of any report result, keyed by whatever fields that report's response DTO carries — the
 * report viewer renders a table purely from these keys, with no per-report column definitions. */
export type ReportRow = Record<string, string | number | boolean | null>;

export interface SalesReportRow extends ReportRow {
  bookingNumber: string;
  customerName: string;
  destinationName: string | null;
  totalAmount: number;
  status: string;
  createdAt: string;
  assignedAgentName: string | null;
}

export interface RevenueReportRow extends ReportRow {
  periodStart: string;
  revenue: number;
}

export interface CancellationReportRow extends ReportRow {
  bookingNumber: string;
  customerName: string;
  totalAmount: number;
  cancellationReason: string | null;
  createdAt: string;
}

/** No AgentCommissions/rate-config table exists yet — revenue/bookings count only, never a
 * fabricated commission amount (see AgentCommissionReportResponse on the backend). */
export interface AgentCommissionReportRow extends ReportRow {
  staffName: string;
  bookingsCount: number;
  revenue: number;
}

export interface LeadConversionReportRow extends ReportRow {
  source: string;
  totalLeads: number;
  convertedLeads: number;
}

export interface DestinationSalesReportRow extends ReportRow {
  destinationName: string;
  bookingCount: number;
  revenue: number;
}

export interface CollectionReportRow extends ReportRow {
  paymentReference: string;
  bookingNumber: string;
  customerName: string;
  amount: number;
  createdAt: string;
}

export interface OutstandingReportRow extends ReportRow {
  bookingNumber: string;
  customerName: string;
  totalAmount: number;
  amountPaid: number;
  outstandingAmount: number;
}

/** Revenue-only — this schema has no cost/expense tracking table, so this is never "profit". */
export interface ProfitabilityReportRow extends ReportRow {
  destinationName: string;
  revenue: number;
}

export interface ActiveBookingReportRow extends ReportRow {
  bookingNumber: string;
  customerName: string;
  travelDate: string | null;
  status: string;
}

export interface EmployeeProductivityReportRow extends ReportRow {
  staffName: string;
  leadsAssigned: number;
  followUpsCompleted: number;
  quotationsSent: number;
}

export interface MonthlyGrowthReportRow extends ReportRow {
  monthStart: string;
  newLeads: number;
  newBookings: number;
  revenue: number;
}

/** Added after both the Vendor and Reports domains had landed. */
export interface VendorPerformanceReportRow extends ReportRow {
  vendorName: string;
  vendorType: string;
  ratingsCount: number;
  averageRating: number | null;
  paymentsCount: number;
  totalPaid: number;
}

export interface ReportDefinition {
  name: ReportName;
  label: string;
  description: string;
  hasDateRange: boolean;
  hasGroupBy: boolean;
}

/** Drives both the reports hub (cards) and the reusable report viewer (which controls to show). */
export const REPORT_DEFINITIONS: ReportDefinition[] = [
  { name: 'sales', label: 'Sales Report', description: 'Every booking with amount, status and assigned agent.', hasDateRange: true, hasGroupBy: false },
  { name: 'revenue', label: 'Revenue Report', description: 'Revenue trended by day, week or month.', hasDateRange: true, hasGroupBy: true },
  { name: 'cancellation', label: 'Cancellation Report', description: 'Cancelled bookings and their reasons.', hasDateRange: true, hasGroupBy: false },
  {
    name: 'agent-commission',
    label: 'Agent Commission Report',
    description: 'Bookings handled and revenue generated per staff member.',
    hasDateRange: true,
    hasGroupBy: false
  },
  { name: 'lead-conversion', label: 'Lead Conversion Report', description: 'Leads received and converted, by source.', hasDateRange: true, hasGroupBy: false },
  {
    name: 'destination-sales',
    label: 'Destination Sales Report',
    description: 'Bookings and revenue per destination.',
    hasDateRange: true,
    hasGroupBy: false
  },
  { name: 'collection', label: 'Collection Report', description: 'Payments collected against bookings.', hasDateRange: true, hasGroupBy: false },
  {
    name: 'outstanding',
    label: 'Outstanding Report',
    description: 'Bookings with a balance still due — a current snapshot, not scoped by date.',
    hasDateRange: false,
    hasGroupBy: false
  },
  {
    name: 'profitability',
    label: 'Profitability Report',
    description: 'Revenue by destination (revenue-only — no cost data is tracked in this schema).',
    hasDateRange: true,
    hasGroupBy: false
  },
  {
    name: 'active-bookings',
    label: 'Active Bookings Report',
    description: 'Bookings currently in progress — a current snapshot, not scoped by date.',
    hasDateRange: false,
    hasGroupBy: false
  },
  {
    name: 'employee-productivity',
    label: 'Employee Productivity Report',
    description: 'Leads assigned, follow-ups completed and quotations sent per staff member.',
    hasDateRange: true,
    hasGroupBy: false
  },
  {
    name: 'monthly-growth',
    label: 'Monthly Growth Report',
    description: 'New leads, new bookings and revenue, always grouped by calendar month.',
    hasDateRange: true,
    hasGroupBy: true
  },
  {
    name: 'vendor-performance',
    label: 'Vendor Performance Report',
    description: 'Ratings and payments recorded per vendor in the selected range.',
    hasDateRange: true,
    hasGroupBy: false
  }
];
