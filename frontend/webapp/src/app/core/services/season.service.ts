import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PaginationRequest, PaginationResponse } from '../models/api.models';
import { Season, SeasonRequest } from '../models/master-data.models';
import { toHttpParams } from '../http/query-params.util';

@Injectable({ providedIn: 'root' })
export class SeasonService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/seasons`;

  list(request: PaginationRequest): Observable<ApiResponse<PaginationResponse<Season>>> {
    return this.http.get<ApiResponse<PaginationResponse<Season>>>(this.baseUrl, { params: toHttpParams(request) });
  }

  create(request: SeasonRequest): Observable<ApiResponse<Season>> {
    return this.http.post<ApiResponse<Season>>(this.baseUrl, request);
  }

  update(id: string, request: SeasonRequest): Observable<ApiResponse<Season>> {
    return this.http.put<ApiResponse<Season>>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: string): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.baseUrl}/${id}`);
  }
}
