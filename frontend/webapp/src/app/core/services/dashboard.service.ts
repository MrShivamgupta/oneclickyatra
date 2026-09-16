import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api.models';
import {
  DashboardAlert,
  DashboardDateRange,
  DashboardKpis,
  DestinationPerformance,
  LeadFunnelStage,
  RevenueTrendPoint,
  SalesPerformance
} from '../models/dashboard.models';
import { Lead } from '../models/crm.models';
import { Booking } from '../models/booking.models';
import { EnquiryResponse } from '../models/enquiry.models';
import { toHttpParams } from '../http/query-params.util';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/dashboard`;

  getStats(range: DashboardDateRange): Observable<ApiResponse<DashboardKpis>> {
    return this.http.get<ApiResponse<DashboardKpis>>(`${this.baseUrl}/stats`, { params: toHttpParams(range) });
  }

  getRevenueChart(range: DashboardDateRange): Observable<ApiResponse<RevenueTrendPoint[]>> {
    return this.http.get<ApiResponse<RevenueTrendPoint[]>>(`${this.baseUrl}/revenue-chart`, { params: toHttpParams(range) });
  }

  getLeadFunnel(range: DashboardDateRange): Observable<ApiResponse<LeadFunnelStage[]>> {
    return this.http.get<ApiResponse<LeadFunnelStage[]>>(`${this.baseUrl}/lead-funnel`, { params: toHttpParams(range) });
  }

  getDestinationPerformance(range: DashboardDateRange): Observable<ApiResponse<DestinationPerformance[]>> {
    return this.http.get<ApiResponse<DestinationPerformance[]>>(`${this.baseUrl}/destination-performance`, { params: toHttpParams(range) });
  }

  getSalesPerformance(range: DashboardDateRange): Observable<ApiResponse<SalesPerformance[]>> {
    return this.http.get<ApiResponse<SalesPerformance[]>>(`${this.baseUrl}/sales-performance`, { params: toHttpParams(range) });
  }

  getRecentLeads(): Observable<ApiResponse<Lead[]>> {
    return this.http.get<ApiResponse<Lead[]>>(`${this.baseUrl}/recent-leads`);
  }

  getRecentBookings(): Observable<ApiResponse<Booking[]>> {
    return this.http.get<ApiResponse<Booking[]>>(`${this.baseUrl}/recent-bookings`);
  }

  getPendingEnquiries(): Observable<ApiResponse<EnquiryResponse[]>> {
    return this.http.get<ApiResponse<EnquiryResponse[]>>(`${this.baseUrl}/pending-enquiries`);
  }

  getRefundRequests(): Observable<ApiResponse<Booking[]>> {
    return this.http.get<ApiResponse<Booking[]>>(`${this.baseUrl}/refund-requests`);
  }

  getUpcomingDepartures(): Observable<ApiResponse<Booking[]>> {
    return this.http.get<ApiResponse<Booking[]>>(`${this.baseUrl}/upcoming-departures`);
  }

  getAlerts(): Observable<ApiResponse<DashboardAlert[]>> {
    return this.http.get<ApiResponse<DashboardAlert[]>>(`${this.baseUrl}/alerts`);
  }
}
