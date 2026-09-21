import { DecimalPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { DataTable, DataTableColumn } from '../../../shared/components/data-table/data-table';
import { Pagination } from '../../../shared/components/pagination/pagination';
import { FilterBar } from '../../../shared/components/filter-bar/filter-bar';
import { VendorService } from '../../../core/services/vendor.service';
import { VENDOR_TYPES, Vendor } from '../../../core/models/vendor.models';

const PAGE_SIZE = 10;

@Component({
  selector: 'app-vendors-list',
  standalone: true,
  imports: [DataTable, Pagination, FilterBar, DecimalPipe],
  templateUrl: './vendors-list.html',
  styleUrls: ['../../destinations/destinations.scss', '../../leads/leads-list/leads-list.scss', './vendors-list.scss']
})
export class VendorsList {
  private readonly vendorService = inject(VendorService);
  private readonly router = inject(Router);

  readonly vendorTypes = VENDOR_TYPES;

  readonly columns: DataTableColumn[] = [
    { key: 'name', label: 'Vendor' },
    { key: 'type', label: 'Type' },
    { key: 'location', label: 'Location' },
    { key: 'rating', label: 'Rating' },
    { key: 'status', label: 'Status' },
    { key: 'actions', label: '' }
  ];

  readonly vendors = signal<Vendor[]>([]);
  readonly loading = signal(false);
  readonly loadError = signal<string | null>(null);
  readonly pageNumber = signal(1);
  readonly totalCount = signal(0);
  readonly searchTerm = signal('');
  readonly typeFilter = signal('');
  readonly activeFilter = signal('');

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.loadError.set(null);
    this.vendorService
      .search({
        pageNumber: this.pageNumber(),
        pageSize: PAGE_SIZE,
        searchTerm: this.searchTerm() || undefined,
        vendorType: this.typeFilter() || undefined,
        isActive: this.activeFilter() === '' ? undefined : this.activeFilter() === 'true'
      })
      .subscribe({
        next: (response) => {
          this.loading.set(false);
          if (response.success && response.data) {
            this.vendors.set(response.data.items);
            this.totalCount.set(response.data.totalCount);
          } else {
            this.vendors.set([]);
            this.totalCount.set(0);
            this.loadError.set(response.message || 'Could not load vendors. Please try again.');
          }
        },
        error: (error) => {
          this.loading.set(false);
          this.loadError.set(error?.error?.message ?? 'Could not load vendors. Please try again.');
        }
      });
  }

  location(vendor: Vendor): string {
    return [vendor.city, vendor.country].filter((part) => !!part).join(', ') || '—';
  }

  onSearchChange(term: string): void {
    this.searchTerm.set(term);
    this.pageNumber.set(1);
    this.load();
  }

  onTypeFilterChange(type: string): void {
    this.typeFilter.set(type);
    this.pageNumber.set(1);
    this.load();
  }

  onActiveFilterChange(value: string): void {
    this.activeFilter.set(value);
    this.pageNumber.set(1);
    this.load();
  }

  onPageChange(page: number): void {
    this.pageNumber.set(page);
    this.load();
  }

  createVendor(): void {
    this.router.navigate(['/admin/vendors/new']);
  }

  openVendor(vendor: Vendor): void {
    this.router.navigate(['/admin/vendors', vendor.id]);
  }
}
