import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PaginationResponse } from '../models/api.models';
import {
  PackageDetail,
  PackageInclusion,
  PackageInventoryDeparture,
  PackageItineraryDay,
  PackageMediaItem,
  PackagePricingTier,
  PackageRequest,
  PackageSearchParams,
  PackageSummary
} from '../models/package.models';
import { toHttpParams } from '../http/query-params.util';

@Injectable({ providedIn: 'root' })
export class PackageService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/packages`;

  search(request: PackageSearchParams): Observable<ApiResponse<PaginationResponse<PackageSummary>>> {
    return this.http.get<ApiResponse<PaginationResponse<PackageSummary>>>(this.baseUrl, { params: toHttpParams(request) });
  }

  getById(id: string): Observable<ApiResponse<PackageDetail>> {
    return this.http.get<ApiResponse<PackageDetail>>(`${this.baseUrl}/${id}`);
  }

  getBySlug(slug: string): Observable<ApiResponse<PackageDetail>> {
    return this.http.get<ApiResponse<PackageDetail>>(`${this.baseUrl}/by-slug/${slug}`);
  }

  create(request: PackageRequest): Observable<ApiResponse<PackageSummary>> {
    return this.http.post<ApiResponse<PackageSummary>>(this.baseUrl, request);
  }

  update(id: string, request: PackageRequest): Observable<ApiResponse<PackageSummary>> {
    return this.http.put<ApiResponse<PackageSummary>>(`${this.baseUrl}/${id}`, request);
  }

  updateStatus(id: string, status: 'Draft' | 'Published'): Observable<ApiResponse<PackageSummary>> {
    return this.http.put<ApiResponse<PackageSummary>>(`${this.baseUrl}/${id}/status`, { status });
  }

  delete(id: string): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.baseUrl}/${id}`);
  }

  replaceItinerary(id: string, days: PackageItineraryDay[]): Observable<ApiResponse<PackageItineraryDay[]>> {
    return this.http.put<ApiResponse<PackageItineraryDay[]>>(`${this.baseUrl}/${id}/itinerary`, days);
  }

  replaceInclusions(id: string, inclusions: PackageInclusion[]): Observable<ApiResponse<PackageInclusion[]>> {
    return this.http.put<ApiResponse<PackageInclusion[]>>(`${this.baseUrl}/${id}/inclusions`, inclusions);
  }

  replacePricing(id: string, tiers: PackagePricingTier[]): Observable<ApiResponse<PackagePricingTier[]>> {
    return this.http.put<ApiResponse<PackagePricingTier[]>>(`${this.baseUrl}/${id}/pricing`, tiers);
  }

  replaceInventory(id: string, departures: PackageInventoryDeparture[]): Observable<ApiResponse<PackageInventoryDeparture[]>> {
    return this.http.put<ApiResponse<PackageInventoryDeparture[]>>(`${this.baseUrl}/${id}/inventory`, departures);
  }

  replaceMedia(id: string, media: PackageMediaItem[]): Observable<ApiResponse<PackageMediaItem[]>> {
    return this.http.put<ApiResponse<PackageMediaItem[]>>(`${this.baseUrl}/${id}/media`, media);
  }
}
