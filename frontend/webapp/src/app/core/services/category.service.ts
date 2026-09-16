import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PaginationRequest, PaginationResponse } from '../models/api.models';
import { Category, CategoryRequest } from '../models/master-data.models';
import { toHttpParams } from '../http/query-params.util';

@Injectable({ providedIn: 'root' })
export class CategoryService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/categories`;

  list(request: PaginationRequest): Observable<ApiResponse<PaginationResponse<Category>>> {
    return this.http.get<ApiResponse<PaginationResponse<Category>>>(this.baseUrl, { params: toHttpParams(request) });
  }

  create(request: CategoryRequest): Observable<ApiResponse<Category>> {
    return this.http.post<ApiResponse<Category>>(this.baseUrl, request);
  }

  update(id: string, request: CategoryRequest): Observable<ApiResponse<Category>> {
    return this.http.put<ApiResponse<Category>>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: string): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.baseUrl}/${id}`);
  }
}
