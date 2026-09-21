import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { DataTable, DataTableColumn } from '../../shared/components/data-table/data-table';
import { Pagination } from '../../shared/components/pagination/pagination';
import { FilterBar } from '../../shared/components/filter-bar/filter-bar';
import { UserService } from '../../core/services/user.service';
import { AppUser, STAFF_ROLES } from '../../core/models/user.models';

const PAGE_SIZE = 10;

// Mirrors CreateStaffUserRequestValidator on the backend: 8+ chars, at least one upper, one lower, one digit.
// This is a client-side hint only -- the server re-validates and is authoritative.
const PASSWORD_PATTERN = /^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9]).{8,}$/;

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [ReactiveFormsModule, DataTable, Pagination, FilterBar],
  templateUrl: './users.html',
  styleUrls: ['../destinations/destinations.scss', './users.scss']
})
export class Users {
  private readonly userService = inject(UserService);
  private readonly formBuilder = inject(FormBuilder);

  readonly staffRoles = STAFF_ROLES;

  readonly columns: DataTableColumn[] = [
    { key: 'fullName', label: 'Name' },
    { key: 'email', label: 'Email' },
    { key: 'roles', label: 'Role(s)' },
    { key: 'status', label: 'Status' },
    { key: 'actions', label: '' }
  ];

  readonly users = signal<AppUser[]>([]);
  readonly loading = signal(false);
  readonly loadError = signal<string | null>(null);
  readonly pageNumber = signal(1);
  readonly totalCount = signal(0);
  readonly searchTerm = signal('');
  readonly roleFilter = signal('');
  readonly activeFilter = signal('');

  readonly isModalOpen = signal(false);
  readonly formError = signal<string | null>(null);
  readonly isSaving = signal(false);

  readonly roleUpdatingId = signal<string | null>(null);
  readonly roleError = signal<string | null>(null);

  readonly statusUpdatingId = signal<string | null>(null);
  readonly statusError = signal<string | null>(null);

  readonly form = this.formBuilder.nonNullable.group({
    fullName: ['', [Validators.required, Validators.maxLength(200)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
    password: ['', [Validators.required, Validators.pattern(PASSWORD_PATTERN)]],
    role: [this.staffRoles[0] as string, Validators.required]
  });

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.loadError.set(null);
    this.userService
      .search({
        pageNumber: this.pageNumber(),
        pageSize: PAGE_SIZE,
        searchTerm: this.searchTerm() || undefined,
        role: this.roleFilter() || undefined,
        isActive: this.activeFilter() === '' ? undefined : this.activeFilter() === 'true'
      })
      .subscribe({
        next: (response) => {
          this.loading.set(false);
          if (response.success && response.data) {
            this.users.set(response.data.items);
            this.totalCount.set(response.data.totalCount);
          } else {
            this.users.set([]);
            this.totalCount.set(0);
            this.loadError.set(response.message || 'Could not load users. Please try again.');
          }
        },
        error: (error) => {
          this.loading.set(false);
          this.loadError.set(error?.error?.message ?? 'Could not load users. Please try again.');
        }
      });
  }

  onSearchChange(term: string): void {
    this.searchTerm.set(term);
    this.pageNumber.set(1);
    this.load();
  }

  onRoleFilterChange(role: string): void {
    this.roleFilter.set(role);
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

  openCreateModal(): void {
    this.formError.set(null);
    this.form.reset({ fullName: '', email: '', password: '', role: this.staffRoles[0] });
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

    this.userService.create(this.form.getRawValue()).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.isModalOpen.set(false);
        this.pageNumber.set(1);
        this.load();
      },
      error: (error) => {
        this.isSaving.set(false);
        this.formError.set(error?.error?.message ?? 'Could not create this user. Please try again.');
      }
    });
  }

  /** The role shown as the current selection in a row's "change role" dropdown. Staff users always
   * carry exactly one staff role at a time (UpdateRoleAsync removes the old one before assigning the new). */
  primaryRole(user: AppUser): string {
    return user.roles[0] ?? '';
  }

  changeRole(user: AppUser, role: string): void {
    if (!role || role === this.primaryRole(user)) {
      return;
    }
    this.roleUpdatingId.set(user.id);
    this.roleError.set(null);
    this.userService.updateRole(user.id, { role }).subscribe({
      next: () => {
        this.roleUpdatingId.set(null);
        this.load();
      },
      error: (error) => {
        this.roleUpdatingId.set(null);
        this.roleError.set(error?.error?.message ?? "Could not change this user's role. Please try again.");
      }
    });
  }

  toggleActive(user: AppUser): void {
    this.statusUpdatingId.set(user.id);
    this.statusError.set(null);
    this.userService.updateStatus(user.id, { isActive: !user.isActive }).subscribe({
      next: () => {
        this.statusUpdatingId.set(null);
        this.load();
      },
      error: (error) => {
        this.statusUpdatingId.set(null);
        this.statusError.set(error?.error?.message ?? "Could not update this user's status. Please try again.");
      }
    });
  }
}
