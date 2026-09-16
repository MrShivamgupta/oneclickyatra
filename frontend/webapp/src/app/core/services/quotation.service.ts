import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PaginationResponse } from '../models/api.models';
import {
  Quotation,
  QuotationDetail,
  QuotationOption,
  QuotationOptionRequest,
  QuotationPublic,
  QuotationPublicApproveRequest,
  QuotationPublicRejectRequest,
  QuotationRequest,
  QuotationSearchParams
} from '../models/quotation.models';
import { toHttpParams } from '../http/query-params.util';

@Injectable({ providedIn: 'root' })
export class QuotationService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/quotations`;

  search(request: QuotationSearchParams): Observable<ApiResponse<PaginationResponse<Quotation>>> {
    return this.http.get<ApiResponse<PaginationResponse<Quotation>>>(this.baseUrl, { params: toHttpParams(request) });
  }

  getById(id: string): Observable<ApiResponse<QuotationDetail>> {
    return this.http.get<ApiResponse<QuotationDetail>>(`${this.baseUrl}/${id}`);
  }

  create(request: QuotationRequest): Observable<ApiResponse<Quotation>> {
    return this.http.post<ApiResponse<Quotation>>(this.baseUrl, request);
  }

  update(id: string, request: QuotationRequest): Observable<ApiResponse<Quotation>> {
    return this.http.put<ApiResponse<Quotation>>(`${this.baseUrl}/${id}`, request);
  }

  replaceOptions(id: string, options: QuotationOptionRequest[]): Observable<ApiResponse<QuotationOption[]>> {
    return this.http.put<ApiResponse<QuotationOption[]>>(`${this.baseUrl}/${id}/options`, options);
  }

  send(id: string): Observable<ApiResponse<Quotation>> {
    return this.http.post<ApiResponse<Quotation>>(`${this.baseUrl}/${id}/send`, {});
  }

  regenerateLink(id: string): Observable<ApiResponse<Quotation>> {
    return this.http.post<ApiResponse<Quotation>>(`${this.baseUrl}/${id}/regenerate-link`, {});
  }

  delete(id: string): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.baseUrl}/${id}`);
  }

  downloadPdf(id: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/${id}/pdf`, { responseType: 'blob' });
  }

  getPublic(token: string): Observable<ApiResponse<QuotationPublic>> {
    return this.http.get<ApiResponse<QuotationPublic>>(`${this.baseUrl}/public/${token}`);
  }

  approvePublic(token: string, request: QuotationPublicApproveRequest): Observable<ApiResponse<null>> {
    return this.http.post<ApiResponse<null>>(`${this.baseUrl}/public/${token}/approve`, request);
  }

  rejectPublic(token: string, request: QuotationPublicRejectRequest): Observable<ApiResponse<null>> {
    return this.http.post<ApiResponse<null>>(`${this.baseUrl}/public/${token}/reject`, request);
  }

  downloadPublicPdf(token: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/public/${token}/pdf`, { responseType: 'blob' });
  }
}
