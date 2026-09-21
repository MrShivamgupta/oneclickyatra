import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PaginationResponse } from '../models/api.models';
import { UserSummary } from '../models/crm.models';
import { AppUser, CreateStaffUserRequest, UpdateUserRoleRequest, UpdateUserStatusRequest, UserSearchParams } from '../models/user.models';
import { toHttpParams } from '../http/query-params.util';

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/users`;

  listStaff(): Observable<ApiResponse<UserSummary[]>> {
    return this.http.get<ApiResponse<UserSummary[]>>(`${this.baseUrl}/staff`);
  }

  /** Admin "Users" management screen search — TravelAgent/OperationsStaff/Finance/SuperAdmin only. */
  search(request: UserSearchParams): Observable<ApiResponse<PaginationResponse<AppUser>>> {
    return this.http.get<ApiResponse<PaginationResponse<AppUser>>>(this.baseUrl, { params: toHttpParams(request) });
  }

  create(request: CreateStaffUserRequest): Observable<ApiResponse<AppUser>> {
    return this.http.post<ApiResponse<AppUser>>(this.baseUrl, request);
  }

  updateRole(id: string, request: UpdateUserRoleRequest): Observable<ApiResponse<AppUser>> {
    return this.http.put<ApiResponse<AppUser>>(`${this.baseUrl}/${id}/role`, request);
  }

  updateStatus(id: string, request: UpdateUserStatusRequest): Observable<ApiResponse<AppUser>> {
    return this.http.put<ApiResponse<AppUser>>(`${this.baseUrl}/${id}/status`, request);
  }
}
