import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { DataTable, DataTableColumn } from '../../shared/components/data-table/data-table';
import { Pagination } from '../../shared/components/pagination/pagination';
import { FilterBar } from '../../shared/components/filter-bar/filter-bar';
import { ConfirmationDialog } from '../../shared/components/confirmation-dialog/confirmation-dialog';
import { DestinationService } from '../../core/services/destination.service';
import { CountryService } from '../../core/services/country.service';
import { CityService } from '../../core/services/city.service';
import { Country, City, Destination } from '../../core/models/master-data.models';

const PAGE_SIZE = 10;

@Component({
  selector: 'app-destinations',
  standalone: true,
  imports: [ReactiveFormsModule, DataTable, Pagination, FilterBar, ConfirmationDialog],
  templateUrl: './destinations.html',
  styleUrl: './destinations.scss'
})
export class Destinations {
  private readonly destinationService = inject(DestinationService);
  private readonly countryService = inject(CountryService);
  private readonly cityService = inject(CityService);
  private readonly formBuilder = inject(FormBuilder);

  readonly columns: DataTableColumn[] = [
    { key: 'name', label: 'Name' },
    { key: 'country', label: 'Country' },
    { key: 'city', label: 'City' },
    { key: 'status', label: 'Status' },
    { key: 'actions', label: '' }
  ];

  readonly destinations = signal<Destination[]>([]);
  readonly countries = signal<Country[]>([]);
  readonly formCities = signal<City[]>([]);
  readonly loading = signal(false);
  readonly loadError = signal<string | null>(null);
  readonly pageNumber = signal(1);
  readonly totalCount = signal(0);
  readonly searchTerm = signal('');
  readonly countryFilter = signal('');

  readonly isModalOpen = signal(false);
  readonly editingId = signal<string | null>(null);
  readonly formError = signal<string | null>(null);
  readonly formFieldErrors = signal<string[]>([]);
  readonly isSaving = signal(false);

  readonly deleteTarget = signal<Destination | null>(null);
  readonly deleting = signal(false);
  readonly deleteError = signal<string | null>(null);

  readonly form = this.formBuilder.nonNullable.group({
    countryId: ['', Validators.required],
    cityId: [''],
    name: ['', [Validators.required, Validators.maxLength(150)]],
    slug: ['', [Validators.required, Validators.pattern(/^[a-z0-9]+(-[a-z0-9]+)*$/)]],
    shortDescription: [''],
    latitude: [null as number | null, [Validators.min(-90), Validators.max(90)]],
    longitude: [null as number | null, [Validators.min(-180), Validators.max(180)]],
    isFeatured: [false],
    isPublished: [true]
  });

  constructor() {
    this.countryService.list({ pageNumber: 1, pageSize: 100 }).subscribe((response) => {
      if (response.success && response.data) {
        this.countries.set(response.data.items);
      }
    });

    this.form.controls.countryId.valueChanges.subscribe((countryId) => {
      this.form.controls.cityId.setValue('');
      this.formCities.set([]);
      if (countryId) {
        this.cityService.list({ pageNumber: 1, pageSize: 100, countryId }).subscribe((response) => {
          if (response.success && response.data) {
            this.formCities.set(response.data.items);
          }
        });
      }
    });

    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.loadError.set(null);
    this.destinationService
      .search({
        pageNumber: this.pageNumber(),
        pageSize: PAGE_SIZE,
        searchTerm: this.searchTerm() || undefined,
        countryId: this.countryFilter() || undefined
      })
      .subscribe({
        next: (response) => {
          this.loading.set(false);
          if (response.success && response.data) {
            this.destinations.set(response.data.items);
            this.totalCount.set(response.data.totalCount);
          }
        },
        error: (error) => {
          this.loading.set(false);
          this.loadError.set(error?.error?.message ?? 'Could not load destinations. Please try again.');
        }
      });
  }

  onSearchChange(term: string): void {
    this.searchTerm.set(term);
    this.pageNumber.set(1);
    this.load();
  }

  onCountryFilterChange(countryId: string): void {
    this.countryFilter.set(countryId);
    this.pageNumber.set(1);
    this.load();
  }

  onPageChange(page: number): void {
    this.pageNumber.set(page);
    this.load();
  }

  openCreateModal(): void {
    this.editingId.set(null);
    this.formError.set(null);
    this.formFieldErrors.set([]);
    this.form.reset({ countryId: '', cityId: '', name: '', slug: '', shortDescription: '', latitude: null, longitude: null, isFeatured: false, isPublished: true });
    this.isModalOpen.set(true);
  }

  openEditModal(destination: Destination): void {
    this.editingId.set(destination.id);
    this.formError.set(null);
    this.formFieldErrors.set([]);
    this.form.setValue({
      countryId: destination.countryId,
      cityId: destination.cityId ?? '',
      name: destination.name,
      slug: destination.slug,
      shortDescription: destination.shortDescription ?? '',
      latitude: destination.latitude ?? null,
      longitude: destination.longitude ?? null,
      isFeatured: destination.isFeatured,
      isPublished: destination.isPublished
    });
    this.isModalOpen.set(true);
  }

  closeModal(): void {
    this.isModalOpen.set(false);
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSaving.set(true);
    this.formError.set(null);
    this.formFieldErrors.set([]);
    const raw = this.form.getRawValue();
    const request = {
      countryId: raw.countryId,
      cityId: raw.cityId || null,
      name: raw.name,
      slug: raw.slug,
      shortDescription: raw.shortDescription || null,
      latitude: raw.latitude,
      longitude: raw.longitude,
      isFeatured: raw.isFeatured,
      isPublished: raw.isPublished
    };

    const editingId = this.editingId();
    const save$ = editingId ? this.destinationService.update(editingId, request) : this.destinationService.create(request);

    save$.subscribe({
      next: () => {
        this.isSaving.set(false);
        this.isModalOpen.set(false);
        this.load();
      },
      error: (error) => {
        this.isSaving.set(false);
        this.formError.set(error?.error?.message ?? 'Something went wrong. Please try again.');
        const fieldErrors = error?.error?.errors as Record<string, string[]> | null | undefined;
        this.formFieldErrors.set(fieldErrors ? Object.values(fieldErrors).flat() : []);
      }
    });
  }

  confirmDelete(destination: Destination): void {
    this.deleteError.set(null);
    this.deleteTarget.set(destination);
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
    this.destinationService.delete(target.id).subscribe({
      next: () => {
        this.deleting.set(false);
        this.deleteTarget.set(null);
        this.load();
      },
      error: (error) => {
        this.deleting.set(false);
        this.deleteError.set(error?.error?.message ?? 'Could not delete this destination. Please try again.');
      }
    });
  }
}
