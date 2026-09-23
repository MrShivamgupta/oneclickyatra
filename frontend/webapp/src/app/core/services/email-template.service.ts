import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PaginationResponse } from '../models/api.models';
import { EmailTemplate, EmailTemplateSearchParams, UpsertEmailTemplateRequest } from '../models/settings.models';
import { toHttpParams } from '../http/query-params.util';

@Injectable({ providedIn: 'root' })
export class EmailTemplateService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/email-templates`;

  search(request: EmailTemplateSearchParams): Observable<ApiResponse<PaginationResponse<EmailTemplate>>> {
    return this.http.get<ApiResponse<PaginationResponse<EmailTemplate>>>(this.baseUrl, { params: toHttpParams(request) });
  }

  create(request: UpsertEmailTemplateRequest): Observable<ApiResponse<EmailTemplate>> {
    return this.http.post<ApiResponse<EmailTemplate>>(this.baseUrl, request);
  }

  update(id: string, request: UpsertEmailTemplateRequest): Observable<ApiResponse<EmailTemplate>> {
    return this.http.put<ApiResponse<EmailTemplate>>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: string): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.baseUrl}/${id}`);
  }
}
