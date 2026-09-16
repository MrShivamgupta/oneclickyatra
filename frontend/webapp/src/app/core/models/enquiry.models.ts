export interface EnquiryRequest {
  fullName: string;
  email: string;
  phone: string;
  destinationId?: string;
  travelDate?: string;
  message: string;
}

export interface EnquiryResponse {
  id: string;
  fullName: string;
  email: string;
  phone: string;
  destinationId?: string;
  destinationName?: string;
  travelDate?: string;
  message: string;
  status: string;
  createdAt: string;
}

export const ENQUIRY_STATUSES = ['New', 'Contacted', 'Converted', 'Closed'] as const;
export type EnquiryStatus = (typeof ENQUIRY_STATUSES)[number];

export interface EnquirySearchParams {
  pageNumber: number;
  pageSize: number;
  searchTerm?: string;
  status?: string;
}
