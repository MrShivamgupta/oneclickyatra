import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PaginationRequest, PaginationResponse } from '../models/api.models';
import { FollowUp, FollowUpStatusRequest } from '../models/crm.models';
import { toHttpParams } from '../http/query-params.util';

@Injectable({ providedIn: 'root' })
export class FollowUpService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/followups`;

  today(request: PaginationRequest): Observable<ApiResponse<PaginationResponse<FollowUp>>> {
    return this.http.get<ApiResponse<PaginationResponse<FollowUp>>>(`${this.baseUrl}/today`, { params: toHttpParams(request) });
  }

  updateStatus(id: string, request: FollowUpStatusRequest): Observable<ApiResponse<FollowUp>> {
    return this.http.put<ApiResponse<FollowUp>>(`${this.baseUrl}/${id}/status`, request);
  }

  delete(id: string): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.baseUrl}/${id}`);
  }
}
