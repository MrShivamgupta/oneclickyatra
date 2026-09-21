import { DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { DataTable, DataTableColumn } from '../../../shared/components/data-table/data-table';
import { Pagination } from '../../../shared/components/pagination/pagination';
import { FilterBar } from '../../../shared/components/filter-bar/filter-bar';
import { ConfirmationDialog } from '../../../shared/components/confirmation-dialog/confirmation-dialog';
import { WhatsAppService } from '../../../core/services/whatsapp.service';
import {
  NOTIFICATION_CHANNELS,
  NOTIFICATION_STATUSES,
  NotificationLog,
  WHATSAPP_TEMPLATE_CATEGORIES,
  WhatsAppTemplate
} from '../../../core/models/whatsapp.models';

const PAGE_SIZE = 10;

type TabKey = 'templates' | 'logs';

@Component({
  selector: 'app-whatsapp-center',
  standalone: true,
  imports: [ReactiveFormsModule, DataTable, Pagination, FilterBar, ConfirmationDialog, DatePipe],
  templateUrl: './whatsapp-center.html',
  styleUrl: './whatsapp-center.scss'
})
export class WhatsAppCenter {
  private readonly whatsAppService = inject(WhatsAppService);
  private readonly formBuilder = inject(FormBuilder);

  readonly activeTab = signal<TabKey>('templates');
  readonly tabs: { key: TabKey; label: string }[] = [
    { key: 'templates', label: 'Templates' },
    { key: 'logs', label: 'Notification Log' }
  ];

  readonly categories = WHATSAPP_TEMPLATE_CATEGORIES;
  readonly channels = NOTIFICATION_CHANNELS;
  readonly statuses = NOTIFICATION_STATUSES;

  // ---------- Templates tab ----------
  readonly templateColumns: DataTableColumn[] = [
    { key: 'name', label: 'Name' },
    { key: 'category', label: 'Category' },
    { key: 'bodyText', label: 'Body' },
    { key: 'isActive', label: 'Status' },
    { key: 'actions', label: '' }
  ];

  readonly templates = signal<WhatsAppTemplate[]>([]);
  readonly templatesLoading = signal(false);
  readonly templatesLoadError = signal<string | null>(null);
  readonly templatesPageNumber = signal(1);
  readonly templatesTotalCount = signal(0);
  readonly templatesSearchTerm = signal('');
  readonly templatesCategoryFilter = signal('');

  /** Only active templates can actually be sent (WhatsAppAppFunction rejects inactive ones), so the
   * send-test picker only ever offers those. */
  readonly activeTemplates = computed(() => this.templates().filter((template) => template.isActive));

  readonly isTemplateModalOpen = signal(false);
  readonly editingTemplateId = signal<string | null>(null);
  readonly templateFormError = signal<string | null>(null);
  readonly isSavingTemplate = signal(false);
  readonly deleteTemplateTarget = signal<WhatsAppTemplate | null>(null);
  readonly deletingTemplate = signal(false);
  readonly deleteTemplateError = signal<string | null>(null);

  readonly templateForm = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    category: ['Welcome', Validators.required],
    bodyText: ['', [Validators.required, Validators.maxLength(1000)]],
    isActive: [true]
  });

  // ---------- Notification Log tab ----------
  readonly logColumns: DataTableColumn[] = [
    { key: 'channel', label: 'Channel' },
    { key: 'recipient', label: 'Recipient' },
    { key: 'preview', label: 'Template / Message' },
    { key: 'status', label: 'Status' },
    { key: 'sentAt', label: 'Sent At' }
  ];

  readonly logs = signal<NotificationLog[]>([]);
  readonly logsLoading = signal(false);
  readonly logsLoadError = signal<string | null>(null);
  readonly logsPageNumber = signal(1);
  readonly logsTotalCount = signal(0);
  readonly logsSearchTerm = signal('');
  readonly logsChannelFilter = signal('');
  readonly logsStatusFilter = signal('');

  // ---------- Send test message ----------
  readonly sendForm = this.formBuilder.nonNullable.group({
    phoneNumber: ['', [Validators.required, Validators.maxLength(20)]],
    templateName: ['', Validators.required],
    parametersCsv: ['']
  });

  readonly isSending = signal(false);
  readonly sendResult = signal<NotificationLog | null>(null);
  readonly sendError = signal<string | null>(null);

  constructor() {
    this.loadTemplates();
    this.loadLogs();
  }

  selectTab(tab: TabKey): void {
    this.activeTab.set(tab);
  }

  // ---------- Templates ----------
  loadTemplates(): void {
    this.templatesLoading.set(true);
    this.templatesLoadError.set(null);
    this.whatsAppService
      .searchTemplates({
        pageNumber: this.templatesPageNumber(),
        pageSize: PAGE_SIZE,
        searchTerm: this.templatesSearchTerm() || undefined,
        category: this.templatesCategoryFilter() || undefined
      })
      .subscribe({
        next: (response) => {
          this.templatesLoading.set(false);
          if (response.success && response.data) {
            this.templates.set(response.data.items);
            this.templatesTotalCount.set(response.data.totalCount);
          } else {
            this.templates.set([]);
            this.templatesTotalCount.set(0);
            this.templatesLoadError.set(response.message || 'Could not load templates. Please try again.');
          }
        },
        error: (error) => {
          this.templatesLoading.set(false);
          this.templatesLoadError.set(error?.error?.message ?? 'Could not load templates. Please try again.');
        }
      });
  }

  onTemplateSearchChange(term: string): void {
    this.templatesSearchTerm.set(term);
    this.templatesPageNumber.set(1);
    this.loadTemplates();
  }

  onTemplateCategoryFilterChange(category: string): void {
    this.templatesCategoryFilter.set(category);
    this.templatesPageNumber.set(1);
    this.loadTemplates();
  }

  onTemplatesPageChange(page: number): void {
    this.templatesPageNumber.set(page);
    this.loadTemplates();
  }

  openCreateTemplateModal(): void {
    this.editingTemplateId.set(null);
    this.templateFormError.set(null);
    this.templateForm.reset({ name: '', category: 'Welcome', bodyText: '', isActive: true });
    this.isTemplateModalOpen.set(true);
  }

  openEditTemplateModal(template: WhatsAppTemplate): void {
    this.editingTemplateId.set(template.id);
    this.templateFormError.set(null);
    this.templateForm.setValue({
      name: template.name,
      category: template.category,
      bodyText: template.bodyText,
      isActive: template.isActive
    });
    this.isTemplateModalOpen.set(true);
  }

  closeTemplateModal(): void {
    this.isTemplateModalOpen.set(false);
  }

  saveTemplate(): void {
    if (this.templateForm.invalid) {
      this.templateForm.markAllAsTouched();
      return;
    }

    this.isSavingTemplate.set(true);
    this.templateFormError.set(null);
    const raw = this.templateForm.getRawValue();
    const request = {
      name: raw.name,
      category: raw.category,
      bodyText: raw.bodyText,
      isActive: raw.isActive
    };

    const editingId = this.editingTemplateId();
    const save$ = editingId
      ? this.whatsAppService.updateTemplate(editingId, request)
      : this.whatsAppService.createTemplate(request);

    save$.subscribe({
      next: () => {
        this.isSavingTemplate.set(false);
        this.isTemplateModalOpen.set(false);
        this.loadTemplates();
      },
      error: (error) => {
        this.isSavingTemplate.set(false);
        this.templateFormError.set(error?.error?.message ?? 'Something went wrong. Please try again.');
      }
    });
  }

  confirmDeleteTemplate(template: WhatsAppTemplate): void {
    this.deleteTemplateError.set(null);
    this.deleteTemplateTarget.set(template);
  }

  cancelDeleteTemplate(): void {
    this.deleteTemplateTarget.set(null);
  }

  performDeleteTemplate(): void {
    const target = this.deleteTemplateTarget();
    if (!target) {
      return;
    }
    this.deletingTemplate.set(true);
    this.deleteTemplateError.set(null);
    this.whatsAppService.deleteTemplate(target.id).subscribe({
      next: () => {
        this.deletingTemplate.set(false);
        this.deleteTemplateTarget.set(null);
        this.loadTemplates();
      },
      error: (error) => {
        this.deletingTemplate.set(false);
        this.deleteTemplateError.set(error?.error?.message ?? 'Could not delete this template. Please try again.');
      }
    });
  }

  // ---------- Notification Log ----------
  loadLogs(): void {
    this.logsLoading.set(true);
    this.logsLoadError.set(null);
    this.whatsAppService
      .searchLogs({
        pageNumber: this.logsPageNumber(),
        pageSize: PAGE_SIZE,
        searchTerm: this.logsSearchTerm() || undefined,
        channel: this.logsChannelFilter() || undefined,
        status: this.logsStatusFilter() || undefined
      })
      .subscribe({
        next: (response) => {
          this.logsLoading.set(false);
          if (response.success && response.data) {
            this.logs.set(response.data.items);
            this.logsTotalCount.set(response.data.totalCount);
          } else {
            this.logs.set([]);
            this.logsTotalCount.set(0);
            this.logsLoadError.set(response.message || 'Could not load the notification log. Please try again.');
          }
        },
        error: (error) => {
          this.logsLoading.set(false);
          this.logsLoadError.set(error?.error?.message ?? 'Could not load the notification log. Please try again.');
        }
      });
  }

  onLogsSearchChange(term: string): void {
    this.logsSearchTerm.set(term);
    this.logsPageNumber.set(1);
    this.loadLogs();
  }

  onLogsChannelFilterChange(channel: string): void {
    this.logsChannelFilter.set(channel);
    this.logsPageNumber.set(1);
    this.loadLogs();
  }

  onLogsStatusFilterChange(status: string): void {
    this.logsStatusFilter.set(status);
    this.logsPageNumber.set(1);
    this.loadLogs();
  }

  onLogsPageChange(page: number): void {
    this.logsPageNumber.set(page);
    this.loadLogs();
  }

  // ---------- Send test message ----------
  sendTestMessage(): void {
    if (this.sendForm.invalid) {
      this.sendForm.markAllAsTouched();
      return;
    }

    this.isSending.set(true);
    this.sendResult.set(null);
    this.sendError.set(null);

    const raw = this.sendForm.getRawValue();
    const parameters = raw.parametersCsv.trim()
      ? raw.parametersCsv.split(',').map((value) => value.trim())
      : [];

    this.whatsAppService
      .sendMessage({ phoneNumber: raw.phoneNumber, templateName: raw.templateName, parameters })
      .subscribe({
        next: (response) => {
          this.isSending.set(false);
          if (response.success && response.data) {
            this.sendResult.set(response.data);
            this.loadLogs();
          }
        },
        error: (error) => {
          this.isSending.set(false);
          this.sendError.set(
            error?.error?.message ?? 'Something went wrong while sending the test message.'
          );
        }
      });
  }
}
