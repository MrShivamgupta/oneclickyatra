import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PaginationRequest, PaginationResponse } from '../models/api.models';
import {
  Vendor,
  VendorContact,
  VendorInvoice,
  VendorInvoiceStatusRequest,
  VendorLinkUserRequest,
  VendorPayment,
  VendorPaymentRequest,
  VendorPaymentStatusRequest,
  VendorPerformance,
  VendorPerformanceRequest,
  VendorRate,
  VendorRequest,
  VendorSearchParams
} from '../models/vendor.models';
import { toHttpParams } from '../http/query-params.util';

/** Staff-facing admin CRUD for the Vendor Management domain — mirrors VendorsController
 * (/api/v1/vendors) field-for-field. */
@Injectable({ providedIn: 'root' })
export class VendorService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/vendors`;

  search(request: VendorSearchParams): Observable<ApiResponse<PaginationResponse<Vendor>>> {
    return this.http.get<ApiResponse<PaginationResponse<Vendor>>>(this.baseUrl, { params: toHttpParams(request) });
  }

  getById(id: string): Observable<ApiResponse<Vendor>> {
    return this.http.get<ApiResponse<Vendor>>(`${this.baseUrl}/${id}`);
  }

  create(request: VendorRequest): Observable<ApiResponse<Vendor>> {
    return this.http.post<ApiResponse<Vendor>>(this.baseUrl, request);
  }

  update(id: string, request: VendorRequest): Observable<ApiResponse<Vendor>> {
    return this.http.put<ApiResponse<Vendor>>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: string): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.baseUrl}/${id}`);
  }

  /** Links an existing Vendor row to a User account so that user can sign into the Vendor
   * Portal. There is no vendor self-registration flow — an admin does this once. */
  linkUser(id: string, request: VendorLinkUserRequest): Observable<ApiResponse<Vendor>> {
    return this.http.post<ApiResponse<Vendor>>(`${this.baseUrl}/${id}/link-user`, request);
  }

  getContacts(id: string): Observable<ApiResponse<VendorContact[]>> {
    return this.http.get<ApiResponse<VendorContact[]>>(`${this.baseUrl}/${id}/contacts`);
  }

  replaceContacts(id: string, contacts: VendorContact[]): Observable<ApiResponse<VendorContact[]>> {
    return this.http.put<ApiResponse<VendorContact[]>>(`${this.baseUrl}/${id}/contacts`, contacts);
  }

  getRates(id: string): Observable<ApiResponse<VendorRate[]>> {
    return this.http.get<ApiResponse<VendorRate[]>>(`${this.baseUrl}/${id}/rates`);
  }

  replaceRates(id: string, rates: VendorRate[]): Observable<ApiResponse<VendorRate[]>> {
    return this.http.put<ApiResponse<VendorRate[]>>(`${this.baseUrl}/${id}/rates`, rates);
  }

  getPayments(id: string, request: PaginationRequest): Observable<ApiResponse<PaginationResponse<VendorPayment>>> {
    return this.http.get<ApiResponse<PaginationResponse<VendorPayment>>>(`${this.baseUrl}/${id}/payments`, { params: toHttpParams(request) });
  }

  createPayment(id: string, request: VendorPaymentRequest): Observable<ApiResponse<VendorPayment>> {
    return this.http.post<ApiResponse<VendorPayment>>(`${this.baseUrl}/${id}/payments`, request);
  }

  updatePaymentStatus(id: string, paymentId: string, request: VendorPaymentStatusRequest): Observable<ApiResponse<VendorPayment>> {
    return this.http.put<ApiResponse<VendorPayment>>(`${this.baseUrl}/${id}/payments/${paymentId}/status`, request);
  }

  recordPerformance(id: string, request: VendorPerformanceRequest): Observable<ApiResponse<VendorPerformance>> {
    return this.http.post<ApiResponse<VendorPerformance>>(`${this.baseUrl}/${id}/performance`, request);
  }

  getPerformanceHistory(id: string): Observable<ApiResponse<VendorPerformance[]>> {
    return this.http.get<ApiResponse<VendorPerformance[]>>(`${this.baseUrl}/${id}/performance`);
  }

  getInvoices(id: string, request: PaginationRequest): Observable<ApiResponse<PaginationResponse<VendorInvoice>>> {
    return this.http.get<ApiResponse<PaginationResponse<VendorInvoice>>>(`${this.baseUrl}/${id}/invoices`, { params: toHttpParams(request) });
  }

  updateInvoiceStatus(id: string, invoiceId: string, request: VendorInvoiceStatusRequest): Observable<ApiResponse<VendorInvoice>> {
    return this.http.put<ApiResponse<VendorInvoice>>(`${this.baseUrl}/${id}/invoices/${invoiceId}/status`, request);
  }

  downloadInvoice(invoiceId: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/invoices/${invoiceId}/download`, { responseType: 'blob' });
  }
}
