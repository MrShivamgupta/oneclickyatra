import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PaginationRequest, PaginationResponse } from '../models/api.models';
import { Vendor, VendorBookingRequestItem, VendorInvoice, VendorPayment, VendorRate, VendorRequest } from '../models/vendor.models';
import { toHttpParams } from '../http/query-params.util';

/** The vendor-facing self-service portal — mirrors VendorPortalController
 * (/api/v1/vendor-portal). Gated by [Authorize] only on the backend; every method resolves the
 * vendor by the signed-in user's ownership, not by permission claim (Vendor accounts hold no
 * RBAC permissions at all). */
@Injectable({ providedIn: 'root' })
export class VendorPortalService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/vendor-portal`;

  getProfile(): Observable<ApiResponse<Vendor>> {
    return this.http.get<ApiResponse<Vendor>>(`${this.baseUrl}/profile`);
  }

  updateProfile(request: VendorRequest): Observable<ApiResponse<Vendor>> {
    return this.http.put<ApiResponse<Vendor>>(`${this.baseUrl}/profile`, request);
  }

  getRates(): Observable<ApiResponse<VendorRate[]>> {
    return this.http.get<ApiResponse<VendorRate[]>>(`${this.baseUrl}/rates`);
  }

  getPayments(request: PaginationRequest): Observable<ApiResponse<PaginationResponse<VendorPayment>>> {
    return this.http.get<ApiResponse<PaginationResponse<VendorPayment>>>(`${this.baseUrl}/payments`, { params: toHttpParams(request) });
  }

  /** Best-effort: see VendorBookingRequestItem for the scope note on how this approximates a
   * real "requests assigned to you" feed via a DestinationId join. */
  getBookingRequests(request: PaginationRequest): Observable<ApiResponse<PaginationResponse<VendorBookingRequestItem>>> {
    return this.http.get<ApiResponse<PaginationResponse<VendorBookingRequestItem>>>(`${this.baseUrl}/booking-requests`, { params: toHttpParams(request) });
  }

  getMyInvoices(request: PaginationRequest): Observable<ApiResponse<PaginationResponse<VendorInvoice>>> {
    return this.http.get<ApiResponse<PaginationResponse<VendorInvoice>>>(`${this.baseUrl}/invoices`, { params: toHttpParams(request) });
  }

  submitInvoice(file: File, amount: number, notes: string | null, bookingId: string | null): Observable<ApiResponse<VendorInvoice>> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('amount', amount.toString());
    if (notes) {
      formData.append('notes', notes);
    }
    if (bookingId) {
      formData.append('bookingId', bookingId);
    }
    return this.http.post<ApiResponse<VendorInvoice>>(`${this.baseUrl}/invoices`, formData);
  }
}
