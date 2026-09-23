import { DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { DataTable, DataTableColumn } from '../../../shared/components/data-table/data-table';
import { ConfirmationDialog } from '../../../shared/components/confirmation-dialog/confirmation-dialog';
import { EmailTemplateService } from '../../../core/services/email-template.service';
import { EmailTemplate } from '../../../core/models/settings.models';

@Component({
  selector: 'app-email-templates-tab',
  standalone: true,
  imports: [ReactiveFormsModule, DataTable, ConfirmationDialog, DatePipe],
  templateUrl: './email-templates-tab.html',
  styleUrl: './email-templates-tab.scss'
})
export class EmailTemplatesTab {
  private readonly emailTemplateService = inject(EmailTemplateService);
  private readonly formBuilder = inject(FormBuilder);

  // Built as a plain string, not written literally in the template, so Angular's template parser
  // never sees a real {{...}} to try (and fail) to interpolate as an expression.
  readonly placeholderHint =
    'Use {{PlaceholderName}} tokens — e.g. {{CustomerName}}, {{BookingReference}}. Check an existing template of the same name-family for the exact tokens that trigger provides.';

  readonly columns: DataTableColumn[] = [
    { key: 'name', label: 'Name' },
    { key: 'subject', label: 'Subject' },
    { key: 'isActive', label: 'Active' },
    { key: 'updatedAt', label: 'Updated' },
    { key: 'actions', label: '' }
  ];

  readonly templates = signal<EmailTemplate[]>([]);
  readonly loading = signal(false);
  readonly loadError = signal<string | null>(null);
  readonly editingId = signal<string | null>(null);
  readonly formError = signal<string | null>(null);
  readonly isSaving = signal(false);
  readonly deleteTarget = signal<EmailTemplate | null>(null);
  readonly deleting = signal(false);
  readonly deleteError = signal<string | null>(null);
  readonly showForm = signal(false);

  readonly form = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    subject: ['', [Validators.required, Validators.maxLength(200)]],
    bodyHtml: ['', Validators.required],
    isActive: [true]
  });

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.loadError.set(null);
    this.emailTemplateService.search({ pageNumber: 1, pageSize: 100 }).subscribe({
      next: (response) => {
        this.loading.set(false);
        if (response.success && response.data) {
          this.templates.set(response.data.items);
        }
      },
      error: (error) => {
        this.loading.set(false);
        this.loadError.set(error?.error?.message ?? 'Could not load email templates. Please try again.');
      }
    });
  }

  addNew(): void {
    this.editingId.set(null);
    this.formError.set(null);
    this.form.reset({ name: '', subject: '', bodyHtml: '', isActive: true });
    this.form.controls.name.enable();
    this.showForm.set(true);
  }

  edit(template: EmailTemplate): void {
    this.editingId.set(template.id);
    this.formError.set(null);
    this.form.setValue({
      name: template.name,
      subject: template.subject,
      bodyHtml: template.bodyHtml,
      isActive: template.isActive
    });
    // Name is a real key trigger-wiring code looks templates up by (see the backend request's doc
    // comment) -- renaming a live template would silently break whichever email it feeds, so editing
    // only ever touches Subject/BodyHtml/IsActive, never Name.
    this.form.controls.name.disable();
    this.showForm.set(true);
  }

  cancelEdit(): void {
    this.showForm.set(false);
    this.editingId.set(null);
    this.formError.set(null);
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
    const save$ = editingId ? this.emailTemplateService.update(editingId, request) : this.emailTemplateService.create(request);

    save$.subscribe({
      next: () => {
        this.isSaving.set(false);
        this.cancelEdit();
        this.load();
      },
      error: (error) => {
        this.isSaving.set(false);
        this.formError.set(error?.error?.message ?? 'Something went wrong. Please try again.');
      }
    });
  }

  confirmDelete(template: EmailTemplate): void {
    this.deleteError.set(null);
    this.deleteTarget.set(template);
  }

  cancelDelete(): void {
    this.deleteTarget.set(null);
  }

  performDelete(): void {
    const target = this.deleteTarget();
    if (!target) return;
    this.deleting.set(true);
    this.deleteError.set(null);
    this.emailTemplateService.delete(target.id).subscribe({
      next: () => {
        this.deleting.set(false);
        this.deleteTarget.set(null);
        this.load();
      },
      error: (error) => {
        this.deleting.set(false);
        this.deleteError.set(error?.error?.message ?? 'Could not delete this template. Please try again.');
      }
    });
  }
}
