import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PaginationResponse } from '../models/api.models';
import { Invoice, InvoiceSearchParams } from '../models/payment.models';
import { toHttpParams } from '../http/query-params.util';

@Injectable({ providedIn: 'root' })
export class InvoiceService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/invoices`;

  search(request: InvoiceSearchParams): Observable<ApiResponse<PaginationResponse<Invoice>>> {
    return this.http.get<ApiResponse<PaginationResponse<Invoice>>>(this.baseUrl, { params: toHttpParams(request) });
  }

  downloadPdf(id: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/${id}/pdf`, { responseType: 'blob' });
  }
}
