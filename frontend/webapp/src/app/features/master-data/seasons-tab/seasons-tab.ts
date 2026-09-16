import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { DataTable, DataTableColumn } from '../../../shared/components/data-table/data-table';
import { ConfirmationDialog } from '../../../shared/components/confirmation-dialog/confirmation-dialog';
import { SeasonService } from '../../../core/services/season.service';
import { Season } from '../../../core/models/master-data.models';

const MONTH_NAMES = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];

@Component({
  selector: 'app-seasons-tab',
  standalone: true,
  imports: [ReactiveFormsModule, DataTable, ConfirmationDialog],
  templateUrl: './seasons-tab.html',
  styleUrl: '../master-data-tab.scss'
})
export class SeasonsTab {
  private readonly seasonService = inject(SeasonService);
  private readonly formBuilder = inject(FormBuilder);

  readonly columns: DataTableColumn[] = [
    { key: 'name', label: 'Name' },
    { key: 'range', label: 'Months' },
    { key: 'actions', label: '' }
  ];

  readonly seasons = signal<Season[]>([]);
  readonly loading = signal(false);
  readonly editingId = signal<string | null>(null);
  readonly formError = signal<string | null>(null);
  readonly isSaving = signal(false);
  readonly deleteTarget = signal<Season | null>(null);
  readonly months = MONTH_NAMES;

  readonly form = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    startMonth: [1, [Validators.required, Validators.min(1), Validators.max(12)]],
    endMonth: [12, [Validators.required, Validators.min(1), Validators.max(12)]]
  });

  constructor() {
    this.load();
  }

  monthLabel(month: number): string {
    return MONTH_NAMES[month - 1] ?? String(month);
  }

  load(): void {
    this.loading.set(true);
    this.seasonService.list({ pageNumber: 1, pageSize: 100 }).subscribe({
      next: (response) => {
        this.loading.set(false);
        if (response.success && response.data) {
          this.seasons.set(response.data.items);
        }
      },
      error: () => this.loading.set(false)
    });
  }

  edit(season: Season): void {
    this.editingId.set(season.id);
    this.formError.set(null);
    this.form.setValue({ name: season.name, startMonth: season.startMonth, endMonth: season.endMonth });
  }

  cancelEdit(): void {
    this.editingId.set(null);
    this.formError.set(null);
    this.form.reset({ name: '', startMonth: 1, endMonth: 12 });
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSaving.set(true);
    this.formError.set(null);
    const request = this.form.getRawValue();
    const editingId = this.editingId();
    const save$ = editingId ? this.seasonService.update(editingId, request) : this.seasonService.create(request);

    save$.subscribe({
      next: () => {
        this.isSaving.set(false);
        this.cancelEdit();
        this.load();
      },
      error: (error) => {
        this.isSaving.set(false);
        this.formError.set(error?.error?.message ?? 'Something went wrong.');
      }
    });
  }

  confirmDelete(season: Season): void {
    this.deleteTarget.set(season);
  }

  cancelDelete(): void {
    this.deleteTarget.set(null);
  }

  performDelete(): void {
    const target = this.deleteTarget();
    if (!target) {
      return;
    }
    this.seasonService.delete(target.id).subscribe(() => {
      this.deleteTarget.set(null);
      this.load();
    });
  }
}
