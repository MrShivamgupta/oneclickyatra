import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api.models';
import { PermissionMatrix, UpdateRolePermissionsRequest } from '../models/role.models';

@Injectable({ providedIn: 'root' })
export class RoleService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/roles`;

  getPermissionMatrix(): Observable<ApiResponse<PermissionMatrix>> {
    return this.http.get<ApiResponse<PermissionMatrix>>(`${this.baseUrl}/permissions-matrix`);
  }

  updateRolePermissions(roleId: string, request: UpdateRolePermissionsRequest): Observable<ApiResponse<PermissionMatrix>> {
    return this.http.put<ApiResponse<PermissionMatrix>>(`${this.baseUrl}/${roleId}/permissions`, request);
  }
}
