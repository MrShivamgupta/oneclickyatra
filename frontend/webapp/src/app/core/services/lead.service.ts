import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PaginationResponse } from '../models/api.models';
import { Customer, FollowUp, FollowUpRequest, Lead, LeadRequest, LeadSearchParams } from '../models/crm.models';
import { toHttpParams } from '../http/query-params.util';

@Injectable({ providedIn: 'root' })
export class LeadService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/leads`;

  search(request: LeadSearchParams): Observable<ApiResponse<PaginationResponse<Lead>>> {
    return this.http.get<ApiResponse<PaginationResponse<Lead>>>(this.baseUrl, { params: toHttpParams(request) });
  }

  getById(id: string): Observable<ApiResponse<Lead>> {
    return this.http.get<ApiResponse<Lead>>(`${this.baseUrl}/${id}`);
  }

  create(request: LeadRequest): Observable<ApiResponse<Lead>> {
    return this.http.post<ApiResponse<Lead>>(this.baseUrl, request);
  }

  update(id: string, request: LeadRequest): Observable<ApiResponse<Lead>> {
    return this.http.put<ApiResponse<Lead>>(`${this.baseUrl}/${id}`, request);
  }

  updateStatus(id: string, status: string): Observable<ApiResponse<Lead>> {
    return this.http.put<ApiResponse<Lead>>(`${this.baseUrl}/${id}/status`, { status });
  }

  assign(id: string, assignedToUserId: string): Observable<ApiResponse<Lead>> {
    return this.http.put<ApiResponse<Lead>>(`${this.baseUrl}/${id}/assign`, { assignedToUserId });
  }

  updateScore(id: string, leadScore: number): Observable<ApiResponse<Lead>> {
    return this.http.put<ApiResponse<Lead>>(`${this.baseUrl}/${id}/score`, { leadScore });
  }

  convertToCustomer(id: string): Observable<ApiResponse<Customer>> {
    return this.http.post<ApiResponse<Customer>>(`${this.baseUrl}/${id}/convert-to-customer`, {});
  }

  delete(id: string): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.baseUrl}/${id}`);
  }

  listFollowUps(leadId: string): Observable<ApiResponse<FollowUp[]>> {
    return this.http.get<ApiResponse<FollowUp[]>>(`${this.baseUrl}/${leadId}/followups`);
  }

  createFollowUp(leadId: string, request: FollowUpRequest): Observable<ApiResponse<FollowUp>> {
    return this.http.post<ApiResponse<FollowUp>>(`${this.baseUrl}/${leadId}/followups`, request);
  }
}
