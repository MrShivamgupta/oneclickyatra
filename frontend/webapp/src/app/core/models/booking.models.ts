export const BOOKING_STATUSES = [
  'Draft', 'Quoted', 'PendingPayment', 'Confirmed', 'InProgress', 'Completed', 'Cancelled', 'RefundPending', 'Refunded'
] as const;
export type BookingStatus = (typeof BOOKING_STATUSES)[number];

/** Mirrors the backend's BookingAppFunction.AllowedTransitions — the state machine is enforced
 * server-side; this copy only drives which buttons the UI offers. */
export const BOOKING_ALLOWED_TRANSITIONS: Record<BookingStatus, BookingStatus[]> = {
  Draft: ['Quoted', 'Cancelled'],
  Quoted: ['PendingPayment', 'Cancelled'],
  PendingPayment: ['Confirmed', 'Cancelled'],
  Confirmed: ['InProgress', 'Cancelled', 'RefundPending'],
  InProgress: ['Completed'],
  Completed: [],
  Cancelled: ['RefundPending'],
  RefundPending: ['Refunded'],
  Refunded: []
};

export interface Booking {
  id: string;
  bookingNumber: string;
  leadId?: string | null;
  customerId: string;
  customerName?: string | null;
  quotationId?: string | null;
  quotationOptionId?: string | null;
  packageId?: string | null;
  packageTitle?: string | null;
  destinationId?: string | null;
  destinationName?: string | null;
  destinationImageUrl?: string | null;
  travelDate?: string | null;
  returnDate?: string | null;
  numberOfAdults: number;
  numberOfChildren: number;
  totalAmount: number;
  amountPaid: number;
  notes?: string | null;
  status: BookingStatus;
  cancellationReason?: string | null;
}

export interface BookingPassenger {
  id?: string | null;
  fullName: string;
  age?: number | null;
  gender?: string | null;
  idProofType?: string | null;
  idProofNumber?: string | null;
  isLeadPassenger: boolean;
}

export interface BookingAddOn {
  id?: string | null;
  name: string;
  description?: string | null;
  price: number;
  quantity: number;
}

export interface BookingDetail {
  booking: Booking;
  passengers: BookingPassenger[];
  addOns: BookingAddOn[];
}

export interface BookingRequest {
  customerId: string;
  leadId?: string | null;
  packageId?: string | null;
  destinationId?: string | null;
  travelDate?: string | null;
  returnDate?: string | null;
  numberOfAdults: number;
  numberOfChildren: number;
  totalAmount: number;
  notes?: string | null;
}

export interface BookingSearchParams {
  pageNumber: number;
  pageSize: number;
  searchTerm?: string;
  status?: string;
  customerId?: string;
}

export interface BookingStatusHistoryEntry {
  id: string;
  oldStatus?: string | null;
  newStatus: string;
  changedByName?: string | null;
  changedAt: string;
  reason?: string | null;
}
