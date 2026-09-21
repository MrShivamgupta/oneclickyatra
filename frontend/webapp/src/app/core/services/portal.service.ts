import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PaginationRequest, PaginationResponse } from '../models/api.models';
import { Customer, CustomerRequest } from '../models/crm.models';
import { Booking, BookingDetail } from '../models/booking.models';
import { Invoice, Payment } from '../models/payment.models';
import { Quotation } from '../models/quotation.models';
import { CustomerDocument, Feedback, FeedbackRequest } from '../models/portal.models';
import { toHttpParams } from '../http/query-params.util';

@Injectable({ providedIn: 'root' })
export class PortalService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/portal`;

  getProfile(): Observable<ApiResponse<Customer>> {
    return this.http.get<ApiResponse<Customer>>(`${this.baseUrl}/profile`);
  }

  updateProfile(request: CustomerRequest): Observable<ApiResponse<Customer>> {
    return this.http.put<ApiResponse<Customer>>(`${this.baseUrl}/profile`, request);
  }

  getBookings(request: PaginationRequest): Observable<ApiResponse<PaginationResponse<Booking>>> {
    return this.http.get<ApiResponse<PaginationResponse<Booking>>>(`${this.baseUrl}/bookings`, { params: toHttpParams(request) });
  }

  getBookingById(id: string): Observable<ApiResponse<BookingDetail>> {
    return this.http.get<ApiResponse<BookingDetail>>(`${this.baseUrl}/bookings/${id}`);
  }

  submitFeedback(bookingId: string, request: FeedbackRequest): Observable<ApiResponse<Feedback>> {
    return this.http.post<ApiResponse<Feedback>>(`${this.baseUrl}/bookings/${bookingId}/feedback`, request);
  }

  getPayments(request: PaginationRequest): Observable<ApiResponse<PaginationResponse<Payment>>> {
    return this.http.get<ApiResponse<PaginationResponse<Payment>>>(`${this.baseUrl}/payments`, { params: toHttpParams(request) });
  }

  getInvoices(request: PaginationRequest): Observable<ApiResponse<PaginationResponse<Invoice>>> {
    return this.http.get<ApiResponse<PaginationResponse<Invoice>>>(`${this.baseUrl}/invoices`, { params: toHttpParams(request) });
  }

  downloadInvoicePdf(id: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/invoices/${id}/pdf`, { responseType: 'blob' });
  }

  getQuotations(request: PaginationRequest): Observable<ApiResponse<PaginationResponse<Quotation>>> {
    return this.http.get<ApiResponse<PaginationResponse<Quotation>>>(`${this.baseUrl}/quotations`, { params: toHttpParams(request) });
  }

  downloadVoucher(bookingId: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/bookings/${bookingId}/voucher`, { responseType: 'blob' });
  }

  uploadDocument(bookingId: string, file: File): Observable<ApiResponse<CustomerDocument>> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<ApiResponse<CustomerDocument>>(`${this.baseUrl}/bookings/${bookingId}/documents`, formData);
  }

  getDocuments(bookingId: string): Observable<ApiResponse<CustomerDocument[]>> {
    return this.http.get<ApiResponse<CustomerDocument[]>>(`${this.baseUrl}/bookings/${bookingId}/documents`);
  }

  downloadDocument(documentId: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/documents/${documentId}/download`, { responseType: 'blob' });
  }
}
