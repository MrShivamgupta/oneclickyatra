import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PaginationResponse } from '../models/api.models';
import { Feedback, FeedbackSearchParams, Testimonial } from '../models/feedback.models';
import { toHttpParams } from '../http/query-params.util';

@Injectable({ providedIn: 'root' })
export class FeedbackService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/feedbacks`;

  search(request: FeedbackSearchParams): Observable<ApiResponse<PaginationResponse<Feedback>>> {
    return this.http.get<ApiResponse<PaginationResponse<Feedback>>>(this.baseUrl, { params: toHttpParams(request) });
  }

  /** Public, no auth required -- a handful of recent well-rated testimonials for the home page. */
  getPublicTestimonials(): Observable<ApiResponse<Testimonial[]>> {
    return this.http.get<ApiResponse<Testimonial[]>>(`${this.baseUrl}/testimonials`);
  }
}
