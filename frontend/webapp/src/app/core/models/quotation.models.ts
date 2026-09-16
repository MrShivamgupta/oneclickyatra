export const QUOTATION_STATUSES = ['Draft', 'Sent', 'Approved', 'Rejected', 'Expired', 'Converted'] as const;
export type QuotationStatus = (typeof QUOTATION_STATUSES)[number];

export interface QuotationItem {
  id?: string | null;
  description: string;
  category?: string | null;
  amount: number;
  sortOrder: number;
}

export interface QuotationOption {
  id?: string | null;
  packageId?: string | null;
  optionName: string;
  destinationId?: string | null;
  destinationName?: string | null;
  durationDays?: number | null;
  durationNights?: number | null;
  hotelCategory?: string | null;
  numberOfPeople: number;
  pricePerPerson: number;
  totalPrice: number;
  isRecommended: boolean;
  sortOrder: number;
  items: QuotationItem[];
}

export interface QuotationItemRequest {
  description: string;
  category?: string | null;
  amount: number;
  sortOrder: number;
}

export interface QuotationOptionRequest {
  packageId?: string | null;
  optionName: string;
  destinationId?: string | null;
  durationDays?: number | null;
  durationNights?: number | null;
  hotelCategory?: string | null;
  numberOfPeople: number;
  pricePerPerson: number;
  isRecommended: boolean;
  sortOrder: number;
  items: QuotationItemRequest[];
}

export interface Quotation {
  id: string;
  quotationNumber: string;
  leadId: string;
  leadCustomerName?: string | null;
  customerId?: string | null;
  customerName?: string | null;
  title: string;
  status: QuotationStatus;
  validUntil?: string | null;
  notes?: string | null;
  selectedOptionId?: string | null;
  approvedAt?: string | null;
  approvedByName?: string | null;
  rejectionReason?: string | null;
  publicToken?: string | null;
}

export interface QuotationDetail {
  quotation: Quotation;
  options: QuotationOption[];
}

export interface QuotationRequest {
  leadId: string;
  title: string;
  validUntil?: string | null;
  notes?: string | null;
}

export interface QuotationSearchParams {
  pageNumber: number;
  pageSize: number;
  searchTerm?: string;
  status?: string;
  leadId?: string;
}

export interface QuotationPublic {
  quotationNumber: string;
  title: string;
  status: QuotationStatus;
  validUntil?: string | null;
  notes?: string | null;
  leadCustomerName?: string | null;
  selectedOptionId?: string | null;
  options: QuotationOption[];
}

export interface QuotationPublicApproveRequest {
  selectedOptionId: string;
  approvedByName: string;
  comments?: string | null;
}

export interface QuotationPublicRejectRequest {
  rejectedByName: string;
  reason?: string | null;
}
