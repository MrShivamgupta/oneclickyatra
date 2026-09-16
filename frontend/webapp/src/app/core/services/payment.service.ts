import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PaginationResponse } from '../models/api.models';
import { Payment, PaymentInitiateResult, PaymentSearchParams, Refund } from '../models/payment.models';
import { toHttpParams } from '../http/query-params.util';

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/payments`;

  search(request: PaymentSearchParams): Observable<ApiResponse<PaginationResponse<Payment>>> {
    return this.http.get<ApiResponse<PaginationResponse<Payment>>>(this.baseUrl, { params: toHttpParams(request) });
  }

  getById(id: string): Observable<ApiResponse<Payment>> {
    return this.http.get<ApiResponse<Payment>>(`${this.baseUrl}/${id}`);
  }

  initiate(bookingId: string): Observable<ApiResponse<PaymentInitiateResult>> {
    return this.http.post<ApiResponse<PaymentInitiateResult>>(`${this.baseUrl}/initiate`, { bookingId });
  }

  refund(bookingId: string, reason?: string | null): Observable<ApiResponse<Refund>> {
    return this.http.post<ApiResponse<Refund>>(`${this.baseUrl}/refund`, { bookingId, reason });
  }
}
