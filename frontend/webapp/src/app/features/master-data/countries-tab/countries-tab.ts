import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { DataTable, DataTableColumn } from '../../../shared/components/data-table/data-table';
import { ConfirmationDialog } from '../../../shared/components/confirmation-dialog/confirmation-dialog';
import { CountryService } from '../../../core/services/country.service';
import { Country } from '../../../core/models/master-data.models';

@Component({
  selector: 'app-countries-tab',
  standalone: true,
  imports: [ReactiveFormsModule, DataTable, ConfirmationDialog],
  templateUrl: './countries-tab.html',
  styleUrl: '../master-data-tab.scss'
})
export class CountriesTab {
  private readonly countryService = inject(CountryService);
  private readonly formBuilder = inject(FormBuilder);

  readonly columns: DataTableColumn[] = [
    { key: 'name', label: 'Name' },
    { key: 'isoCode', label: 'Code' },
    { key: 'actions', label: '' }
  ];

  readonly countries = signal<Country[]>([]);
  readonly loading = signal(false);
  readonly loadError = signal<string | null>(null);
  readonly editingId = signal<string | null>(null);
  readonly formError = signal<string | null>(null);
  readonly isSaving = signal(false);
  readonly deleteTarget = signal<Country | null>(null);
  readonly deleting = signal(false);
  readonly deleteError = signal<string | null>(null);

  readonly form = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(150)]],
    isoCode: ['', [Validators.required, Validators.pattern(/^[A-Za-z]{2,3}$/)]]
  });

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.loadError.set(null);
    this.countryService.list({ pageNumber: 1, pageSize: 100 }).subscribe({
      next: (response) => {
        this.loading.set(false);
        if (response.success && response.data) {
          this.countries.set(response.data.items);
        }
      },
      error: (error) => {
        this.loading.set(false);
        this.loadError.set(error?.error?.message ?? 'Could not load countries. Please try again.');
      }
    });
  }

  edit(country: Country): void {
    this.editingId.set(country.id);
    this.formError.set(null);
    this.form.setValue({ name: country.name, isoCode: country.isoCode });
  }

  cancelEdit(): void {
    this.editingId.set(null);
    this.formError.set(null);
    this.form.reset({ name: '', isoCode: '' });
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
    const save$ = editingId ? this.countryService.update(editingId, request) : this.countryService.create(request);

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

  confirmDelete(country: Country): void {
    this.deleteError.set(null);
    this.deleteTarget.set(country);
  }

  cancelDelete(): void {
    this.deleteTarget.set(null);
  }

  performDelete(): void {
    const target = this.deleteTarget();
    if (!target) {
      return;
    }
    this.deleting.set(true);
    this.deleteError.set(null);
    this.countryService.delete(target.id).subscribe({
      next: () => {
        this.deleting.set(false);
        this.deleteTarget.set(null);
        this.load();
      },
      error: (error) => {
        this.deleting.set(false);
        this.deleteError.set(error?.error?.message ?? 'Could not delete this country. Please try again.');
      }
    });
  }
}
