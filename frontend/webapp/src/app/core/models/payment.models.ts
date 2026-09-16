export const PAYMENT_STATUSES = ['Created', 'Pending', 'Paid', 'Failed', 'Refunded', 'PartiallyRefunded'] as const;
export type PaymentStatus = (typeof PAYMENT_STATUSES)[number];

export interface Payment {
  id: string;
  bookingId: string;
  bookingNumber?: string | null;
  customerName?: string | null;
  amount: number;
  currency: string;
  status: PaymentStatus;
  gatewayProvider: string;
  gatewayOrderId?: string | null;
  gatewayPaymentId?: string | null;
  createdAt: string;
}

export interface PaymentInitiateResult {
  paymentId: string;
  gatewayOrderId: string;
  amount: number;
  currency: string;
  gatewayKeyId: string;
}

export interface Refund {
  id: string;
  paymentId: string;
  bookingId: string;
  amount: number;
  reason?: string | null;
  status: string;
  gatewayRefundId?: string | null;
  createdAt: string;
}

export interface Invoice {
  id: string;
  bookingId: string;
  bookingNumber?: string | null;
  customerName?: string | null;
  invoiceNumber: string;
  amount: number;
  issuedAt: string;
}

export interface PaymentSearchParams {
  pageNumber: number;
  pageSize: number;
  searchTerm?: string;
  status?: string;
  bookingId?: string;
}

export interface InvoiceSearchParams {
  pageNumber: number;
  pageSize: number;
  searchTerm?: string;
  bookingId?: string;
}
