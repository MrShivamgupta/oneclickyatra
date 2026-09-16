export const LEAD_STATUSES = ['New', 'Contacted', 'QuotationSent', 'Negotiation', 'Confirmed', 'Lost'] as const;
export type LeadStatus = (typeof LEAD_STATUSES)[number];

export const FOLLOW_UP_TYPES = ['Call', 'WhatsApp', 'Email', 'Visit'] as const;
export type FollowUpType = (typeof FOLLOW_UP_TYPES)[number];

export const FOLLOW_UP_STATUSES = ['Pending', 'Completed', 'Cancelled'] as const;
export type FollowUpStatus = (typeof FOLLOW_UP_STATUSES)[number];

export interface Customer {
  id: string;
  fullName: string;
  email?: string | null;
  phone: string;
  userId?: string | null;
}

export interface CustomerRequest {
  fullName: string;
  email?: string | null;
  phone: string;
}

export interface Lead {
  id: string;
  customerName: string;
  mobile: string;
  email?: string | null;
  destinationId?: string | null;
  destinationName?: string | null;
  travelDate?: string | null;
  budget?: number | null;
  source?: string | null;
  assignedToUserId?: string | null;
  assignedToName?: string | null;
  leadScore: number;
  status: LeadStatus;
  customerId?: string | null;
}

export interface LeadRequest {
  customerName: string;
  mobile: string;
  email?: string | null;
  destinationId?: string | null;
  travelDate?: string | null;
  budget?: number | null;
  source?: string | null;
  assignedToUserId?: string | null;
}

export interface LeadSearchParams {
  pageNumber: number;
  pageSize: number;
  searchTerm?: string;
  status?: string;
  assignedToUserId?: string;
  destinationId?: string;
}

export interface FollowUp {
  id: string;
  leadId: string;
  leadCustomerName?: string | null;
  scheduledAt: string;
  type: FollowUpType;
  notes?: string | null;
  status: FollowUpStatus;
  completedAt?: string | null;
}

export interface FollowUpRequest {
  scheduledAt: string;
  type: FollowUpType;
  notes?: string | null;
}

export interface FollowUpStatusRequest {
  status: FollowUpStatus;
  notes?: string | null;
}

export interface UserSummary {
  id: string;
  fullName: string;
  email: string;
}
