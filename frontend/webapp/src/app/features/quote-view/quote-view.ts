import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { QuotationService } from '../../core/services/quotation.service';
import { QuotationPublic } from '../../core/models/quotation.models';
import { Spinner } from '../../shared/components/spinner/spinner';

type DecisionMode = 'none' | 'approve' | 'reject';

@Component({
  selector: 'app-quote-view',
  standalone: true,
  imports: [FormsModule, DatePipe, DecimalPipe, Spinner],
  templateUrl: './quote-view.html',
  styleUrl: './quote-view.scss'
})
export class QuoteView {
  private readonly quotationService = inject(QuotationService);
  private readonly route = inject(ActivatedRoute);

  readonly token = signal('');
  readonly quotation = signal<QuotationPublic | null>(null);
  readonly loading = signal(true);
  readonly notFound = signal(false);
  readonly loadError = signal<string | null>(null);

  readonly selectedOptionId = signal<string | null>(null);
  readonly decisionMode = signal<DecisionMode>('none');
  readonly approvedByName = signal('');
  readonly comments = signal('');
  readonly rejectedByName = signal('');
  readonly reason = signal('');

  readonly submitting = signal(false);
  readonly submitError = signal<string | null>(null);
  readonly decisionMade = signal<'approved' | 'rejected' | null>(null);
  readonly pdfDownloading = signal(false);
  readonly downloadError = signal<string | null>(null);

  constructor() {
    const token = this.route.snapshot.paramMap.get('token') ?? '';
    this.token.set(token);
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.notFound.set(false);
    this.loadError.set(null);
    this.quotationService.getPublic(this.token()).subscribe({
      next: (response) => {
        this.loading.set(false);
        if (response.success && response.data) {
          this.quotation.set(response.data);
          if (response.data.options.length > 0) {
            this.selectedOptionId.set(response.data.options.find((o) => o.isRecommended)?.id ?? response.data.options[0].id ?? null);
          }
        } else {
          this.notFound.set(true);
        }
      },
      error: (error) => {
        this.loading.set(false);
        if (error?.status === 404) {
          this.notFound.set(true);
        } else {
          this.loadError.set(error?.error?.message ?? 'Could not load this quotation. Please try again.');
        }
      }
    });
  }

  startApprove(): void {
    this.decisionMode.set('approve');
    this.submitError.set(null);
  }

  startReject(): void {
    this.decisionMode.set('reject');
    this.submitError.set(null);
  }

  cancelDecision(): void {
    this.decisionMode.set('none');
    this.submitError.set(null);
  }

  submitApprove(): void {
    const optionId = this.selectedOptionId();
    if (!optionId || !this.approvedByName().trim()) {
      this.submitError.set('Please select an option and enter your name.');
      return;
    }

    this.submitting.set(true);
    this.submitError.set(null);
    this.quotationService
      .approvePublic(this.token(), {
        selectedOptionId: optionId,
        approvedByName: this.approvedByName().trim(),
        comments: this.comments().trim() || null
      })
      .subscribe({
        next: () => {
          this.submitting.set(false);
          this.decisionMade.set('approved');
        },
        error: (error) => {
          this.submitting.set(false);
          this.submitError.set(error?.error?.message ?? 'Could not submit your approval. Please try again.');
        }
      });
  }

  submitReject(): void {
    if (!this.rejectedByName().trim()) {
      this.submitError.set('Please enter your name.');
      return;
    }

    this.submitting.set(true);
    this.submitError.set(null);
    this.quotationService
      .rejectPublic(this.token(), { rejectedByName: this.rejectedByName().trim(), reason: this.reason().trim() || null })
      .subscribe({
        next: () => {
          this.submitting.set(false);
          this.decisionMade.set('rejected');
        },
        error: (error) => {
          this.submitting.set(false);
          this.submitError.set(error?.error?.message ?? 'Could not submit your response. Please try again.');
        }
      });
  }

  downloadPdf(): void {
    this.pdfDownloading.set(true);
    this.downloadError.set(null);
    this.quotationService.downloadPublicPdf(this.token()).subscribe({
      next: (blob) => {
        this.pdfDownloading.set(false);
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `${this.quotation()?.quotationNumber ?? 'Quotation'}.pdf`;
        link.click();
        URL.revokeObjectURL(url);
      },
      error: (error) => {
        this.pdfDownloading.set(false);
        this.downloadError.set(error?.error?.message ?? 'Could not download the PDF. Please try again.');
      }
    });
  }
}
