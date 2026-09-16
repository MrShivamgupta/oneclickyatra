import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { DataTable, DataTableColumn } from '../../../shared/components/data-table/data-table';
import { ConfirmationDialog } from '../../../shared/components/confirmation-dialog/confirmation-dialog';
import { CategoryService } from '../../../core/services/category.service';
import { Category } from '../../../core/models/master-data.models';

@Component({
  selector: 'app-categories-tab',
  standalone: true,
  imports: [ReactiveFormsModule, DataTable, ConfirmationDialog],
  templateUrl: './categories-tab.html',
  styleUrl: '../master-data-tab.scss'
})
export class CategoriesTab {
  private readonly categoryService = inject(CategoryService);
  private readonly formBuilder = inject(FormBuilder);

  readonly columns: DataTableColumn[] = [
    { key: 'name', label: 'Name' },
    { key: 'slug', label: 'Slug' },
    { key: 'description', label: 'Description' },
    { key: 'actions', label: '' }
  ];

  readonly categories = signal<Category[]>([]);
  readonly loading = signal(false);
  readonly editingId = signal<string | null>(null);
  readonly formError = signal<string | null>(null);
  readonly isSaving = signal(false);
  readonly deleteTarget = signal<Category | null>(null);

  readonly form = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(150)]],
    slug: ['', [Validators.required, Validators.pattern(/^[a-z0-9]+(-[a-z0-9]+)*$/)]],
    description: ['']
  });

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.categoryService.list({ pageNumber: 1, pageSize: 100 }).subscribe({
      next: (response) => {
        this.loading.set(false);
        if (response.success && response.data) {
          this.categories.set(response.data.items);
        }
      },
      error: () => this.loading.set(false)
    });
  }

  edit(category: Category): void {
    this.editingId.set(category.id);
    this.formError.set(null);
    this.form.setValue({ name: category.name, slug: category.slug, description: category.description ?? '' });
  }

  cancelEdit(): void {
    this.editingId.set(null);
    this.formError.set(null);
    this.form.reset({ name: '', slug: '', description: '' });
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSaving.set(true);
    this.formError.set(null);
    const raw = this.form.getRawValue();
    const request = { name: raw.name, slug: raw.slug, description: raw.description || null };
    const editingId = this.editingId();
    const save$ = editingId ? this.categoryService.update(editingId, request) : this.categoryService.create(request);

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

  confirmDelete(category: Category): void {
    this.deleteTarget.set(category);
  }

  cancelDelete(): void {
    this.deleteTarget.set(null);
  }

  performDelete(): void {
    const target = this.deleteTarget();
    if (!target) {
      return;
    }
    this.categoryService.delete(target.id).subscribe(() => {
      this.deleteTarget.set(null);
      this.load();
    });
  }
}
