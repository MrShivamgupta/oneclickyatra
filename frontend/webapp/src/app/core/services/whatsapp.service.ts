import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PaginationResponse } from '../models/api.models';
import {
  NotificationLog,
  NotificationLogSearchParams,
  WhatsAppSendMessageRequest,
  WhatsAppTemplate,
  WhatsAppTemplateRequest,
  WhatsAppTemplateSearchParams
} from '../models/whatsapp.models';
import { toHttpParams } from '../http/query-params.util';

@Injectable({ providedIn: 'root' })
export class WhatsAppService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/whatsapp`;

  searchTemplates(request: WhatsAppTemplateSearchParams): Observable<ApiResponse<PaginationResponse<WhatsAppTemplate>>> {
    return this.http.get<ApiResponse<PaginationResponse<WhatsAppTemplate>>>(`${this.baseUrl}/templates`, {
      params: toHttpParams(request)
    });
  }

  getTemplateById(id: string): Observable<ApiResponse<WhatsAppTemplate>> {
    return this.http.get<ApiResponse<WhatsAppTemplate>>(`${this.baseUrl}/templates/${id}`);
  }

  createTemplate(request: WhatsAppTemplateRequest): Observable<ApiResponse<WhatsAppTemplate>> {
    return this.http.post<ApiResponse<WhatsAppTemplate>>(`${this.baseUrl}/templates`, request);
  }

  updateTemplate(id: string, request: WhatsAppTemplateRequest): Observable<ApiResponse<WhatsAppTemplate>> {
    return this.http.put<ApiResponse<WhatsAppTemplate>>(`${this.baseUrl}/templates/${id}`, request);
  }

  deleteTemplate(id: string): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.baseUrl}/templates/${id}`);
  }

  /** Expected to fail with a 503 "gateway not configured" error in any environment without real
   * Meta Business credentials set on the backend — that is correct, not a bug. */
  sendMessage(request: WhatsAppSendMessageRequest): Observable<ApiResponse<NotificationLog>> {
    return this.http.post<ApiResponse<NotificationLog>>(`${this.baseUrl}/send`, request);
  }

  searchLogs(request: NotificationLogSearchParams): Observable<ApiResponse<PaginationResponse<NotificationLog>>> {
    return this.http.get<ApiResponse<PaginationResponse<NotificationLog>>>(`${this.baseUrl}/logs`, {
      params: toHttpParams(request)
    });
  }
}
