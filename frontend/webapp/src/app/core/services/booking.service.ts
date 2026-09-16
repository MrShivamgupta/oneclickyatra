import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PaginationResponse } from '../models/api.models';
import {
  Booking,
  BookingAddOn,
  BookingDetail,
  BookingPassenger,
  BookingRequest,
  BookingSearchParams,
  BookingStatusHistoryEntry
} from '../models/booking.models';
import { toHttpParams } from '../http/query-params.util';

@Injectable({ providedIn: 'root' })
export class BookingService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/bookings`;

  search(request: BookingSearchParams): Observable<ApiResponse<PaginationResponse<Booking>>> {
    return this.http.get<ApiResponse<PaginationResponse<Booking>>>(this.baseUrl, { params: toHttpParams(request) });
  }

  getById(id: string): Observable<ApiResponse<BookingDetail>> {
    return this.http.get<ApiResponse<BookingDetail>>(`${this.baseUrl}/${id}`);
  }

  getHistory(id: string): Observable<ApiResponse<BookingStatusHistoryEntry[]>> {
    return this.http.get<ApiResponse<BookingStatusHistoryEntry[]>>(`${this.baseUrl}/${id}/history`);
  }

  create(request: BookingRequest): Observable<ApiResponse<Booking>> {
    return this.http.post<ApiResponse<Booking>>(this.baseUrl, request);
  }

  convertFromQuotation(quotationId: string): Observable<ApiResponse<Booking>> {
    return this.http.post<ApiResponse<Booking>>(`${this.baseUrl}/from-quotation/${quotationId}`, {});
  }

  update(id: string, request: BookingRequest): Observable<ApiResponse<Booking>> {
    return this.http.put<ApiResponse<Booking>>(`${this.baseUrl}/${id}`, request);
  }

  updateStatus(id: string, status: string, reason?: string | null): Observable<ApiResponse<Booking>> {
    return this.http.put<ApiResponse<Booking>>(`${this.baseUrl}/${id}/status`, { status, reason });
  }

  cancel(id: string, reason?: string | null): Observable<ApiResponse<Booking>> {
    return this.http.post<ApiResponse<Booking>>(`${this.baseUrl}/${id}/cancel`, { reason });
  }

  refund(id: string, reason?: string | null): Observable<ApiResponse<Booking>> {
    return this.http.post<ApiResponse<Booking>>(`${this.baseUrl}/${id}/refund`, { reason });
  }

  replacePassengers(id: string, passengers: BookingPassenger[]): Observable<ApiResponse<BookingPassenger[]>> {
    return this.http.put<ApiResponse<BookingPassenger[]>>(`${this.baseUrl}/${id}/passengers`, passengers);
  }

  replaceAddOns(id: string, addOns: BookingAddOn[]): Observable<ApiResponse<BookingAddOn[]>> {
    return this.http.put<ApiResponse<BookingAddOn[]>>(`${this.baseUrl}/${id}/addons`, addOns);
  }

  delete(id: string): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.baseUrl}/${id}`);
  }
}
