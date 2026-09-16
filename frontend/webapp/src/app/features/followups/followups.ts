import { DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { DataTable, DataTableColumn } from '../../shared/components/data-table/data-table';
import { Pagination } from '../../shared/components/pagination/pagination';
import { FollowUpService } from '../../core/services/followup.service';
import { FollowUp } from '../../core/models/crm.models';

const PAGE_SIZE = 10;

@Component({
  selector: 'app-followups',
  standalone: true,
  imports: [DataTable, Pagination, DatePipe],
  templateUrl: './followups.html',
  styleUrls: ['../destinations/destinations.scss', '../leads/leads-list/leads-list.scss']
})
export class FollowUps {
  private readonly followUpService = inject(FollowUpService);
  private readonly router = inject(Router);

  readonly columns: DataTableColumn[] = [
    { key: 'lead', label: 'Lead' },
    { key: 'scheduledAt', label: 'Scheduled At' },
    { key: 'type', label: 'Type' },
    { key: 'notes', label: 'Notes' },
    { key: 'status', label: 'Status' },
    { key: 'actions', label: '' }
  ];

  readonly followUps = signal<FollowUp[]>([]);
  readonly loading = signal(false);
  readonly pageNumber = signal(1);
  readonly totalCount = signal(0);

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.followUpService.today({ pageNumber: this.pageNumber(), pageSize: PAGE_SIZE }).subscribe({
      next: (response) => {
        this.loading.set(false);
        if (response.success && response.data) {
          this.followUps.set(response.data.items);
          this.totalCount.set(response.data.totalCount);
        }
      },
      error: () => this.loading.set(false)
    });
  }

  onPageChange(page: number): void {
    this.pageNumber.set(page);
    this.load();
  }

  openLead(followUp: FollowUp): void {
    this.router.navigate(['/admin/leads', followUp.leadId]);
  }

  markStatus(followUp: FollowUp, status: 'Completed' | 'Cancelled'): void {
    this.followUpService.updateStatus(followUp.id, { status }).subscribe(() => this.load());
  }
}
