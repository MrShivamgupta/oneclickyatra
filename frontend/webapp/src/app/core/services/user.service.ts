import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api.models';
import { UserSummary } from '../models/crm.models';

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/users`;

  listStaff(): Observable<ApiResponse<UserSummary[]>> {
    return this.http.get<ApiResponse<UserSummary[]>>(`${this.baseUrl}/staff`);
  }
}
