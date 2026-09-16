import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PaginationRequest, PaginationResponse } from '../models/api.models';
import { Country, CountryRequest } from '../models/master-data.models';
import { toHttpParams } from '../http/query-params.util';

@Injectable({ providedIn: 'root' })
export class CountryService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/countries`;

  list(request: PaginationRequest): Observable<ApiResponse<PaginationResponse<Country>>> {
    return this.http.get<ApiResponse<PaginationResponse<Country>>>(this.baseUrl, { params: toHttpParams(request) });
  }

  create(request: CountryRequest): Observable<ApiResponse<Country>> {
    return this.http.post<ApiResponse<Country>>(this.baseUrl, request);
  }

  update(id: string, request: CountryRequest): Observable<ApiResponse<Country>> {
    return this.http.put<ApiResponse<Country>>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: string): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.baseUrl}/${id}`);
  }
}
