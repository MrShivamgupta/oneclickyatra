import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api.models';
import { CmsPage } from '../models/cms.models';

@Injectable({ providedIn: 'root' })
export class CmsPageService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/pages`;

  getBySlug(slug: string): Observable<ApiResponse<CmsPage>> {
    return this.http.get<ApiResponse<CmsPage>>(`${this.baseUrl}/public/${slug}`);
  }
}
