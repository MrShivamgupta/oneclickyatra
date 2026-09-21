import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PaginationResponse } from '../models/api.models';
import { AuditLog, AuditLogSearchParams } from '../models/audit-log.models';
import { toHttpParams } from '../http/query-params.util';

@Injectable({ providedIn: 'root' })
export class AuditLogService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/audit-logs`;

  search(request: AuditLogSearchParams): Observable<ApiResponse<PaginationResponse<AuditLog>>> {
    return this.http.get<ApiResponse<PaginationResponse<AuditLog>>>(this.baseUrl, { params: toHttpParams(request) });
  }
}
