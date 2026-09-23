import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api.models';
import { IntegrationStatus } from '../models/settings.models';

@Injectable({ providedIn: 'root' })
export class SettingsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/settings`;

  getIntegrationStatus(): Observable<ApiResponse<IntegrationStatus>> {
    return this.http.get<ApiResponse<IntegrationStatus>>(`${this.baseUrl}/integration-status`);
  }
}
