import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PaginationResponse } from '../models/api.models';
import { EnquiryRequest, EnquiryResponse, EnquirySearchParams } from '../models/enquiry.models';
import { toHttpParams } from '../http/query-params.util';

@Injectable({ providedIn: 'root' })
export class EnquiryService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/enquiries`;

  submit(request: EnquiryRequest): Observable<ApiResponse<EnquiryResponse>> {
    return this.http.post<ApiResponse<EnquiryResponse>>(this.baseUrl, request);
  }

  search(request: EnquirySearchParams): Observable<ApiResponse<PaginationResponse<EnquiryResponse>>> {
    return this.http.get<ApiResponse<PaginationResponse<EnquiryResponse>>>(this.baseUrl, { params: toHttpParams(request) });
  }

  updateStatus(id: string, status: string): Observable<ApiResponse<EnquiryResponse>> {
    return this.http.put<ApiResponse<EnquiryResponse>>(`${this.baseUrl}/${id}/status`, { status });
  }

  delete(id: string): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.baseUrl}/${id}`);
  }
}
