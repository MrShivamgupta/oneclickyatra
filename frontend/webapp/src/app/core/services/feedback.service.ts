import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PaginationResponse } from '../models/api.models';
import { Feedback, FeedbackSearchParams } from '../models/feedback.models';
import { toHttpParams } from '../http/query-params.util';

@Injectable({ providedIn: 'root' })
export class FeedbackService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/feedbacks`;

  search(request: FeedbackSearchParams): Observable<ApiResponse<PaginationResponse<Feedback>>> {
    return this.http.get<ApiResponse<PaginationResponse<Feedback>>>(this.baseUrl, { params: toHttpParams(request) });
  }
}
