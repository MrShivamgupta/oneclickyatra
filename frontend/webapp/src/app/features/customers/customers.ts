import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { DataTable, DataTableColumn } from '../../shared/components/data-table/data-table';
import { Pagination } from '../../shared/components/pagination/pagination';
import { FilterBar } from '../../shared/components/filter-bar/filter-bar';
import { ConfirmationDialog } from '../../shared/components/confirmation-dialog/confirmation-dialog';
import { CustomerService } from '../../core/services/customer.service';
import { Customer } from '../../core/models/crm.models';

const PAGE_SIZE = 10;

@Component({
  selector: 'app-customers',
  standalone: true,
  imports: [ReactiveFormsModule, DataTable, Pagination, FilterBar, ConfirmationDialog],
  templateUrl: './customers.html',
  styleUrl: '../destinations/destinations.scss'
})
export class Customers {
  private readonly customerService = inject(CustomerService);
  private readonly formBuilder = inject(FormBuilder);

  readonly columns: DataTableColumn[] = [
    { key: 'fullName', label: 'Name' },
    { key: 'email', label: 'Email' },
    { key: 'phone', label: 'Phone' },
    { key: 'actions', label: '' }
  ];

  readonly customers = signal<Customer[]>([]);
  readonly loading = signal(false);
  readonly loadError = signal<string | null>(null);
  readonly pageNumber = signal(1);
  readonly totalCount = signal(0);
  readonly searchTerm = signal('');

  readonly isModalOpen = signal(false);
  readonly editingId = signal<string | null>(null);
  readonly formError = signal<string | null>(null);
  readonly isSaving = signal(false);

  readonly deleteTarget = signal<Customer | null>(null);
  readonly deleting = signal(false);
  readonly deleteError = signal<string | null>(null);

  readonly form = this.formBuilder.nonNullable.group({
    fullName: ['', [Validators.required, Validators.maxLength(150)]],
    email: [''],
    phone: ['', Validators.required]
  });

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.loadError.set(null);
    this.customerService
      .list({ pageNumber: this.pageNumber(), pageSize: PAGE_SIZE, searchTerm: this.searchTerm() || undefined })
      .subscribe({
        next: (response) => {
          this.loading.set(false);
          if (response.success && response.data) {
            this.customers.set(response.data.items);
            this.totalCount.set(response.data.totalCount);
          }
        },
        error: (error) => {
          this.loading.set(false);
          this.loadError.set(error?.error?.message ?? 'Could not load customers. Please try again.');
        }
      });
  }

  onSearchChange(term: string): void {
    this.searchTerm.set(term);
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
    this.form.reset({ fullName: '', email: '', phone: '' });
    this.isModalOpen.set(true);
  }

  openEditModal(customer: Customer): void {
    this.editingId.set(customer.id);
    this.formError.set(null);
    this.form.setValue({
      fullName: customer.fullName,
      email: customer.email ?? '',
      phone: customer.phone
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
    const raw = this.form.getRawValue();
    const request = {
      fullName: raw.fullName,
      email: raw.email || null,
      phone: raw.phone
    };

    const editingId = this.editingId();
    const save$ = editingId ? this.customerService.update(editingId, request) : this.customerService.create(request);

    save$.subscribe({
      next: () => {
        this.isSaving.set(false);
        this.isModalOpen.set(false);
        this.load();
      },
      error: (error) => {
        this.isSaving.set(false);
        this.formError.set(error?.error?.message ?? 'Something went wrong. Please try again.');
      }
    });
  }

  confirmDelete(customer: Customer): void {
    this.deleteError.set(null);
    this.deleteTarget.set(customer);
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
    this.customerService.delete(target.id).subscribe({
      next: () => {
        this.deleting.set(false);
        this.deleteTarget.set(null);
        this.load();
      },
      error: (error) => {
        this.deleting.set(false);
        this.deleteError.set(error?.error?.message ?? 'Could not delete this customer. Please try again.');
      }
    });
  }
}
