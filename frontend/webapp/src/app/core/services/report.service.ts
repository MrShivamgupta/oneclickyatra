import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api.models';
import { toHttpParams } from '../http/query-params.util';
import {
  ActiveBookingReportRow,
  AgentCommissionReportRow,
  CancellationReportRow,
  CollectionReportRow,
  DestinationSalesReportRow,
  EmployeeProductivityReportRow,
  ExportFormat,
  LeadConversionReportRow,
  MonthlyGrowthReportRow,
  OutstandingReportRow,
  ProfitabilityReportRow,
  ReportDateRangeParams,
  ReportGroupedDateRangeParams,
  ReportName,
  ReportRow,
  RevenueReportRow,
  SalesReportRow,
  VendorPerformanceReportRow
} from '../models/report.models';

@Injectable({ providedIn: 'root' })
export class ReportService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/reports`;

  getSales(params: ReportDateRangeParams): Observable<ApiResponse<SalesReportRow[]>> {
    return this.http.get<ApiResponse<SalesReportRow[]>>(`${this.baseUrl}/sales`, { params: toHttpParams(params) });
  }

  getRevenue(params: ReportGroupedDateRangeParams): Observable<ApiResponse<RevenueReportRow[]>> {
    return this.http.get<ApiResponse<RevenueReportRow[]>>(`${this.baseUrl}/revenue`, { params: toHttpParams(params) });
  }

  getCancellation(params: ReportDateRangeParams): Observable<ApiResponse<CancellationReportRow[]>> {
    return this.http.get<ApiResponse<CancellationReportRow[]>>(`${this.baseUrl}/cancellation`, { params: toHttpParams(params) });
  }

  getAgentCommission(params: ReportDateRangeParams): Observable<ApiResponse<AgentCommissionReportRow[]>> {
    return this.http.get<ApiResponse<AgentCommissionReportRow[]>>(`${this.baseUrl}/agent-commission`, { params: toHttpParams(params) });
  }

  getLeadConversion(params: ReportDateRangeParams): Observable<ApiResponse<LeadConversionReportRow[]>> {
    return this.http.get<ApiResponse<LeadConversionReportRow[]>>(`${this.baseUrl}/lead-conversion`, { params: toHttpParams(params) });
  }

  getDestinationSales(params: ReportDateRangeParams): Observable<ApiResponse<DestinationSalesReportRow[]>> {
    return this.http.get<ApiResponse<DestinationSalesReportRow[]>>(`${this.baseUrl}/destination-sales`, { params: toHttpParams(params) });
  }

  getCollection(params: ReportDateRangeParams): Observable<ApiResponse<CollectionReportRow[]>> {
    return this.http.get<ApiResponse<CollectionReportRow[]>>(`${this.baseUrl}/collection`, { params: toHttpParams(params) });
  }

  getOutstanding(): Observable<ApiResponse<OutstandingReportRow[]>> {
    return this.http.get<ApiResponse<OutstandingReportRow[]>>(`${this.baseUrl}/outstanding`);
  }

  getProfitability(params: ReportDateRangeParams): Observable<ApiResponse<ProfitabilityReportRow[]>> {
    return this.http.get<ApiResponse<ProfitabilityReportRow[]>>(`${this.baseUrl}/profitability`, { params: toHttpParams(params) });
  }

  getActiveBookings(): Observable<ApiResponse<ActiveBookingReportRow[]>> {
    return this.http.get<ApiResponse<ActiveBookingReportRow[]>>(`${this.baseUrl}/active-bookings`);
  }

  getEmployeeProductivity(params: ReportDateRangeParams): Observable<ApiResponse<EmployeeProductivityReportRow[]>> {
    return this.http.get<ApiResponse<EmployeeProductivityReportRow[]>>(`${this.baseUrl}/employee-productivity`, { params: toHttpParams(params) });
  }

  getMonthlyGrowth(params: ReportGroupedDateRangeParams): Observable<ApiResponse<MonthlyGrowthReportRow[]>> {
    return this.http.get<ApiResponse<MonthlyGrowthReportRow[]>>(`${this.baseUrl}/monthly-growth`, { params: toHttpParams(params) });
  }

  getVendorPerformance(params: ReportDateRangeParams): Observable<ApiResponse<VendorPerformanceReportRow[]>> {
    return this.http.get<ApiResponse<VendorPerformanceReportRow[]>>(`${this.baseUrl}/vendor-performance`, { params: toHttpParams(params) });
  }

  /** Generic entry point used by the reusable report viewer, keyed off the route's :reportName —
   * dispatches to the typed method above for that report so callers never have to switch themselves. */
  getReport(reportName: ReportName, params: ReportGroupedDateRangeParams): Observable<ApiResponse<ReportRow[]>> {
    switch (reportName) {
      case 'sales':
        return this.getSales(params);
      case 'revenue':
        return this.getRevenue(params);
      case 'cancellation':
        return this.getCancellation(params);
      case 'agent-commission':
        return this.getAgentCommission(params);
      case 'lead-conversion':
        return this.getLeadConversion(params);
      case 'destination-sales':
        return this.getDestinationSales(params);
      case 'collection':
        return this.getCollection(params);
      case 'outstanding':
        return this.getOutstanding();
      case 'profitability':
        return this.getProfitability(params);
      case 'active-bookings':
        return this.getActiveBookings();
      case 'employee-productivity':
        return this.getEmployeeProductivity(params);
      case 'monthly-growth':
        return this.getMonthlyGrowth(params);
      case 'vendor-performance':
        return this.getVendorPerformance(params);
    }
  }

  /** Mirrors InvoiceService.downloadPdf's Blob handling — the report viewer triggers the browser
   * download itself, the same way features/invoices/invoices.ts's downloadPdf() does. */
  downloadExport(reportName: ReportName, format: ExportFormat, params: ReportGroupedDateRangeParams = {}): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/${reportName}/export`, {
      params: toHttpParams({ ...params, format }),
      responseType: 'blob'
    });
  }
}
