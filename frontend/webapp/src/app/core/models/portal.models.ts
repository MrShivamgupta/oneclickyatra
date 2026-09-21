export interface FeedbackRequest {
  rating: number;
  comment?: string | null;
}

export interface Feedback {
  id: string;
  bookingId: string;
  bookingNumber?: string | null;
  customerName?: string | null;
  rating: number;
  comment?: string | null;
  createdAt: string;
}

export interface CustomerDocument {
  id: string;
  bookingId: string;
  fileName: string;
  contentType: string;
  fileSizeBytes: number;
  createdAt: string;
}
