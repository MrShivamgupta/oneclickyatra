import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api.models';
import { AgencyProfile, UpdateAgencyProfileRequest } from '../models/settings.models';

@Injectable({ providedIn: 'root' })
export class AgencyProfileService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/agency-profile`;

  get(): Observable<ApiResponse<AgencyProfile>> {
    return this.http.get<ApiResponse<AgencyProfile>>(this.baseUrl);
  }

  update(request: UpdateAgencyProfileRequest): Observable<ApiResponse<AgencyProfile>> {
    return this.http.put<ApiResponse<AgencyProfile>>(this.baseUrl, request);
  }
}
