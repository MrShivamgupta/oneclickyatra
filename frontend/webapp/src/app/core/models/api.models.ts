export interface ApiResponse<T> {
  success: boolean;
  data: T | null;
  message: string;
  trackingId: string;
}

export interface ApiErrorResponse {
  success: false;
  data: null;
  message: string;
  trackingId: string;
  errors?: Record<string, string[]> | null;
}

export interface PaginationRequest {
  pageNumber: number;
  pageSize: number;
  sortBy?: string;
  sortDescending?: boolean;
  searchTerm?: string;
}

export interface PaginationResponse<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
