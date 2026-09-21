import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { DataTable, DataTableColumn } from '../../../shared/components/data-table/data-table';
import { Pagination } from '../../../shared/components/pagination/pagination';
import { FilterBar } from '../../../shared/components/filter-bar/filter-bar';
import { LeadService } from '../../../core/services/lead.service';
import { DestinationService } from '../../../core/services/destination.service';
import { Lead, LEAD_STATUSES } from '../../../core/models/crm.models';
import { Destination } from '../../../core/models/master-data.models';

const PAGE_SIZE = 10;

@Component({
  selector: 'app-leads-list',
  standalone: true,
  imports: [DataTable, Pagination, FilterBar],
  templateUrl: './leads-list.html',
  styleUrls: ['../../destinations/destinations.scss', './leads-list.scss']
})
export class LeadsList {
  private readonly leadService = inject(LeadService);
  private readonly destinationService = inject(DestinationService);
  private readonly router = inject(Router);

  readonly statuses = LEAD_STATUSES;

  readonly columns: DataTableColumn[] = [
    { key: 'customerName', label: 'Customer' },
    { key: 'destination', label: 'Destination' },
    { key: 'score', label: 'Score' },
    { key: 'status', label: 'Status' },
    { key: 'assignedTo', label: 'Assigned To' },
    { key: 'actions', label: '' }
  ];

  readonly leads = signal<Lead[]>([]);
  readonly destinations = signal<Destination[]>([]);
  readonly loading = signal(false);
  readonly loadError = signal<string | null>(null);
  readonly pageNumber = signal(1);
  readonly totalCount = signal(0);
  readonly searchTerm = signal('');
  readonly statusFilter = signal('');
  readonly destinationFilter = signal('');

  constructor() {
    this.destinationService.search({ pageNumber: 1, pageSize: 100 }).subscribe((response) => {
      if (response.success && response.data) this.destinations.set(response.data.items);
    });
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.loadError.set(null);
    this.leadService
      .search({
        pageNumber: this.pageNumber(),
        pageSize: PAGE_SIZE,
        searchTerm: this.searchTerm() || undefined,
        status: this.statusFilter() || undefined,
        destinationId: this.destinationFilter() || undefined
      })
      .subscribe({
        next: (response) => {
          this.loading.set(false);
          if (response.success && response.data) {
            this.leads.set(response.data.items);
            this.totalCount.set(response.data.totalCount);
          }
        },
        error: (error) => {
          this.loading.set(false);
          this.loadError.set(error?.error?.message ?? 'Could not load leads. Please try again.');
        }
      });
  }

  onSearchChange(term: string): void {
    this.searchTerm.set(term);
    this.pageNumber.set(1);
    this.load();
  }

  onStatusFilterChange(status: string): void {
    this.statusFilter.set(status);
    this.pageNumber.set(1);
    this.load();
  }

  onDestinationFilterChange(destinationId: string): void {
    this.destinationFilter.set(destinationId);
    this.pageNumber.set(1);
    this.load();
  }

  onPageChange(page: number): void {
    this.pageNumber.set(page);
    this.load();
  }

  createLead(): void {
    this.router.navigate(['/admin/leads/new']);
  }

  openLead(lead: Lead): void {
    this.router.navigate(['/admin/leads', lead.id]);
  }
}
