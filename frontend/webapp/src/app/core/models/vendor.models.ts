/** Mirrors the backend's VendorRequestValidator.ValidVendorTypes (see VendorValidators.cs) and
 * the CK_Vendors_VendorType check constraint — enforced server-side, only drives the dropdown here. */
export const VENDOR_TYPES = ['Hotel', 'Airline', 'Transport', 'DMC', 'ActivityProvider'] as const;
export type VendorType = (typeof VENDOR_TYPES)[number];

/** Mirrors VendorPaymentStatusRequestValidator.ValidStatuses. */
export const VENDOR_PAYMENT_STATUSES = ['Pending', 'Paid', 'Overdue'] as const;
export type VendorPaymentStatus = (typeof VENDOR_PAYMENT_STATUSES)[number];

export interface Vendor {
  id: string;
  name: string;
  vendorType: string;
  email: string;
  phone: string;
  address?: string | null;
  city?: string | null;
  country?: string | null;
  userId?: string | null;
  rating?: number | null;
  isActive: boolean;
}

/** Also used, unchanged, as the body of the Vendor Portal's PUT profile — the server just
 * restricts which fields self-service actually applies (VendorType/IsActive stay admin-only). */
export interface VendorRequest {
  name: string;
  vendorType: string;
  email: string;
  phone: string;
  address?: string | null;
  city?: string | null;
  country?: string | null;
  isActive: boolean;
}

export interface VendorSearchParams {
  pageNumber: number;
  pageSize: number;
  searchTerm?: string;
  vendorType?: string;
  isActive?: boolean;
}

export interface VendorContact {
  id?: string | null;
  contactName: string;
  designation?: string | null;
  phone: string;
  email?: string | null;
  isPrimary: boolean;
}

export interface VendorRate {
  id?: string | null;
  destinationId?: string | null;
  destinationName?: string | null;
  serviceDescription: string;
  rateAmount: number;
  currency: string;
  validFrom?: string | null;
  validTo?: string | null;
}

export interface VendorPayment {
  id: string;
  vendorId: string;
  bookingId?: string | null;
  bookingNumber?: string | null;
  amount: number;
  status: string;
  notes?: string | null;
  paidAt?: string | null;
  createdAt: string;
}

export interface VendorPaymentRequest {
  bookingId?: string | null;
  amount: number;
  notes?: string | null;
}

export interface VendorPaymentStatusRequest {
  status: string;
}

export const VENDOR_INVOICE_STATUSES = ['Pending', 'Reviewed'] as const;

export interface VendorInvoice {
  id: string;
  vendorId: string;
  bookingId?: string | null;
  bookingNumber?: string | null;
  fileName: string;
  contentType: string;
  fileSizeBytes: number;
  amount: number;
  notes?: string | null;
  status: string;
  createdAt: string;
}

export interface VendorInvoiceStatusRequest {
  status: string;
}

export interface VendorPerformance {
  id: string;
  bookingId?: string | null;
  bookingNumber?: string | null;
  rating: number;
  notes?: string | null;
  recordedByName?: string | null;
  createdAt: string;
}

export interface VendorPerformanceRequest {
  bookingId?: string | null;
  rating: number;
  notes?: string | null;
}

export interface VendorLinkUserRequest {
  userId: string;
}

/** A best-effort, read-only projection for the Vendor Portal's "booking requests" feed — see
 * VendorBookingRequestModel on the backend. There is no direct Bookings-to-Vendor link in the
 * schema, so this joins Bookings to the vendor's own VendorRates on DestinationId as the closest
 * reasonable approximation; it surfaces bookings to ANY vendor with a rate for that destination,
 * not bookings actually assigned to this vendor. */
export interface VendorBookingRequestItem {
  bookingId: string;
  bookingNumber: string;
  status: string;
  travelDate?: string | null;
  returnDate?: string | null;
  numberOfAdults: number;
  numberOfChildren: number;
  destinationId?: string | null;
  destinationName?: string | null;
}
