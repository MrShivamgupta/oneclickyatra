import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { DataTable, DataTableColumn } from '../../../shared/components/data-table/data-table';
import { ConfirmationDialog } from '../../../shared/components/confirmation-dialog/confirmation-dialog';
import { CityService } from '../../../core/services/city.service';
import { CountryService } from '../../../core/services/country.service';
import { City, Country } from '../../../core/models/master-data.models';

@Component({
  selector: 'app-cities-tab',
  standalone: true,
  imports: [ReactiveFormsModule, DataTable, ConfirmationDialog],
  templateUrl: './cities-tab.html',
  styleUrl: '../master-data-tab.scss'
})
export class CitiesTab {
  private readonly cityService = inject(CityService);
  private readonly countryService = inject(CountryService);
  private readonly formBuilder = inject(FormBuilder);

  readonly columns: DataTableColumn[] = [
    { key: 'name', label: 'Name' },
    { key: 'country', label: 'Country' },
    { key: 'actions', label: '' }
  ];

  readonly cities = signal<City[]>([]);
  readonly countries = signal<Country[]>([]);
  readonly loading = signal(false);
  readonly loadError = signal<string | null>(null);
  readonly editingId = signal<string | null>(null);
  readonly formError = signal<string | null>(null);
  readonly isSaving = signal(false);
  readonly deleteTarget = signal<City | null>(null);
  readonly deleting = signal(false);
  readonly deleteError = signal<string | null>(null);

  readonly form = this.formBuilder.nonNullable.group({
    countryId: ['', Validators.required],
    name: ['', [Validators.required, Validators.maxLength(150)]]
  });

  constructor() {
    this.countryService.list({ pageNumber: 1, pageSize: 100 }).subscribe((response) => {
      if (response.success && response.data) {
        this.countries.set(response.data.items);
      }
    });
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.loadError.set(null);
    this.cityService.list({ pageNumber: 1, pageSize: 100 }).subscribe({
      next: (response) => {
        this.loading.set(false);
        if (response.success && response.data) {
          this.cities.set(response.data.items);
        }
      },
      error: (error) => {
        this.loading.set(false);
        this.loadError.set(error?.error?.message ?? 'Could not load cities. Please try again.');
      }
    });
  }

  edit(city: City): void {
    this.editingId.set(city.id);
    this.formError.set(null);
    this.form.setValue({ countryId: city.countryId, name: city.name });
  }

  cancelEdit(): void {
    this.editingId.set(null);
    this.formError.set(null);
    this.form.reset({ countryId: '', name: '' });
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
    const save$ = editingId ? this.cityService.update(editingId, request) : this.cityService.create(request);

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

  confirmDelete(city: City): void {
    this.deleteError.set(null);
    this.deleteTarget.set(city);
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
    this.cityService.delete(target.id).subscribe({
      next: () => {
        this.deleting.set(false);
        this.deleteTarget.set(null);
        this.load();
      },
      error: (error) => {
        this.deleting.set(false);
        this.deleteError.set(error?.error?.message ?? 'Could not delete this city. Please try again.');
      }
    });
  }
}
