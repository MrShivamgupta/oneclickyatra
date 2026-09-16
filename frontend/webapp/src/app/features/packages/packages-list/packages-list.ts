import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { DataTable, DataTableColumn } from '../../../shared/components/data-table/data-table';
import { Pagination } from '../../../shared/components/pagination/pagination';
import { FilterBar } from '../../../shared/components/filter-bar/filter-bar';
import { ConfirmationDialog } from '../../../shared/components/confirmation-dialog/confirmation-dialog';
import { PackageService } from '../../../core/services/package.service';
import { DestinationService } from '../../../core/services/destination.service';
import { PackageSummary } from '../../../core/models/package.models';
import { Destination } from '../../../core/models/master-data.models';

const PAGE_SIZE = 10;

@Component({
  selector: 'app-packages-list',
  standalone: true,
  imports: [DataTable, Pagination, FilterBar, ConfirmationDialog],
  templateUrl: './packages-list.html',
  styleUrl: '../../destinations/destinations.scss'
})
export class PackagesList {
  private readonly packageService = inject(PackageService);
  private readonly destinationService = inject(DestinationService);
  private readonly router = inject(Router);

  readonly columns: DataTableColumn[] = [
    { key: 'title', label: 'Title' },
    { key: 'destination', label: 'Destination' },
    { key: 'duration', label: 'Duration' },
    { key: 'status', label: 'Status' },
    { key: 'actions', label: '' }
  ];

  readonly packages = signal<PackageSummary[]>([]);
  readonly destinations = signal<Destination[]>([]);
  readonly loading = signal(false);
  readonly pageNumber = signal(1);
  readonly totalCount = signal(0);
  readonly searchTerm = signal('');
  readonly destinationFilter = signal('');
  readonly statusFilter = signal('');
  readonly deleteTarget = signal<PackageSummary | null>(null);

  constructor() {
    this.destinationService.search({ pageNumber: 1, pageSize: 100 }).subscribe((response) => {
      if (response.success && response.data) {
        this.destinations.set(response.data.items);
      }
    });
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.packageService
      .search({
        pageNumber: this.pageNumber(),
        pageSize: PAGE_SIZE,
        searchTerm: this.searchTerm() || undefined,
        destinationId: this.destinationFilter() || undefined,
        status: this.statusFilter() || undefined
      })
      .subscribe({
        next: (response) => {
          this.loading.set(false);
          if (response.success && response.data) {
            this.packages.set(response.data.items);
            this.totalCount.set(response.data.totalCount);
          }
        },
        error: () => this.loading.set(false)
      });
  }

  onSearchChange(term: string): void {
    this.searchTerm.set(term);
    this.pageNumber.set(1);
    this.load();
  }

  onDestinationFilterChange(destinationId: string): void {
    this.destinationFilter.set(destinationId);
    this.pageNumber.set(1);
    this.load();
  }

  onStatusFilterChange(status: string): void {
    this.statusFilter.set(status);
    this.pageNumber.set(1);
    this.load();
  }

  onPageChange(page: number): void {
    this.pageNumber.set(page);
    this.load();
  }

  createPackage(): void {
    this.router.navigate(['/admin/packages/new']);
  }

  editPackage(pkg: PackageSummary): void {
    this.router.navigate(['/admin/packages', pkg.id]);
  }

  togglePublish(pkg: PackageSummary): void {
    const nextStatus = pkg.status === 'Published' ? 'Draft' : 'Published';
    this.packageService.updateStatus(pkg.id, nextStatus).subscribe(() => this.load());
  }

  confirmDelete(pkg: PackageSummary): void {
    this.deleteTarget.set(pkg);
  }

  cancelDelete(): void {
    this.deleteTarget.set(null);
  }

  performDelete(): void {
    const target = this.deleteTarget();
    if (!target) {
      return;
    }
    this.packageService.delete(target.id).subscribe(() => {
      this.deleteTarget.set(null);
      this.load();
    });
  }
}
