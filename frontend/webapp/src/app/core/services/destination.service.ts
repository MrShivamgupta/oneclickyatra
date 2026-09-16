import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PaginationResponse } from '../models/api.models';
import { Destination, DestinationRequest, DestinationSearchParams } from '../models/master-data.models';
import { toHttpParams } from '../http/query-params.util';

@Injectable({ providedIn: 'root' })
export class DestinationService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/destinations`;

  search(request: DestinationSearchParams): Observable<ApiResponse<PaginationResponse<Destination>>> {
    return this.http.get<ApiResponse<PaginationResponse<Destination>>>(this.baseUrl, { params: toHttpParams(request) });
  }

  getBySlug(slug: string): Observable<ApiResponse<Destination>> {
    return this.http.get<ApiResponse<Destination>>(`${this.baseUrl}/by-slug/${slug}`);
  }

  create(request: DestinationRequest): Observable<ApiResponse<Destination>> {
    return this.http.post<ApiResponse<Destination>>(this.baseUrl, request);
  }

  update(id: string, request: DestinationRequest): Observable<ApiResponse<Destination>> {
    return this.http.put<ApiResponse<Destination>>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: string): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.baseUrl}/${id}`);
  }
}
