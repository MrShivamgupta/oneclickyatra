export interface DashboardDateRange {
  fromDate?: string | null;
  toDate?: string | null;
}

export interface DashboardKpis {
  totalLeads: number;
  totalBookings: number;
  confirmedBookings: number;
  cancellations: number;
  activeQuotations: number;
  periodRevenue: number;
  pendingPayments: number;
  upcomingDepartures: number;
  openEnquiries: number;
  conversionRate: number;
  todayFollowUps: number;
  range: DashboardDateRange;
}

export interface RevenueTrendPoint {
  trendDate: string;
  revenue: number;
}

export interface LeadFunnelStage {
  status: string;
  leadCount: number;
}

export interface DestinationPerformance {
  destinationName: string;
  bookingCount: number;
  revenue: number;
}

export interface SalesPerformance {
  staffName: string;
  leadsHandled: number;
  bookingsCount: number;
  revenue: number;
}

export interface DashboardAlert {
  severity: 'info' | 'warning' | 'danger';
  message: string;
  link?: string | null;
}

export const DATE_PRESETS = ['Today', 'Yesterday', 'Last7Days', 'Last30Days', 'ThisMonth', 'LastMonth', 'Custom'] as const;
export type DatePreset = (typeof DATE_PRESETS)[number];

export const DATE_PRESET_LABELS: Record<DatePreset, string> = {
  Today: 'Today',
  Yesterday: 'Yesterday',
  Last7Days: 'Last 7 Days',
  Last30Days: 'Last 30 Days',
  ThisMonth: 'This Month',
  LastMonth: 'Last Month',
  Custom: 'Custom Range'
};
