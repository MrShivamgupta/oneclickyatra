export interface Feedback {
  id: string;
  bookingNumber: string;
  customerName: string;
  rating: number;
  comment: string | null;
  createdAt: string;
}

export interface FeedbackSearchParams {
  pageNumber: number;
  pageSize: number;
  searchTerm?: string;
  minRating?: number;
  maxRating?: number;
  fromDate?: string;
  toDate?: string;
}
