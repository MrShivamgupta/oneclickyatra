import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PaginationRequest, PaginationResponse } from '../models/api.models';
import { City, CityRequest } from '../models/master-data.models';
import { toHttpParams } from '../http/query-params.util';

@Injectable({ providedIn: 'root' })
export class CityService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/cities`;

  list(request: PaginationRequest & { countryId?: string }): Observable<ApiResponse<PaginationResponse<City>>> {
    return this.http.get<ApiResponse<PaginationResponse<City>>>(this.baseUrl, { params: toHttpParams(request) });
  }

  create(request: CityRequest): Observable<ApiResponse<City>> {
    return this.http.post<ApiResponse<City>>(this.baseUrl, request);
  }

  update(id: string, request: CityRequest): Observable<ApiResponse<City>> {
    return this.http.put<ApiResponse<City>>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: string): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.baseUrl}/${id}`);
  }
}
