import { Component, inject, signal } from '@angular/core';
import { Spinner } from '../../../shared/components/spinner/spinner';
import { SettingsService } from '../../../core/services/settings.service';
import { IntegrationStatus } from '../../../core/models/settings.models';

@Component({
  selector: 'app-integration-status-tab',
  standalone: true,
  imports: [Spinner],
  templateUrl: './integration-status-tab.html',
  styleUrl: './integration-status-tab.scss'
})
export class IntegrationStatusTab {
  private readonly settingsService = inject(SettingsService);

  readonly loading = signal(false);
  readonly loadError = signal<string | null>(null);
  readonly status = signal<IntegrationStatus | null>(null);

  readonly rows: { key: keyof IntegrationStatus; label: string; description: string }[] = [
    { key: 'razorpayConfigured', label: 'Razorpay (Payments)', description: 'Order creation, capture and refund calls to Razorpay.' },
    { key: 'whatsAppConfigured', label: 'WhatsApp Business', description: 'Outbound template messages via the Meta Cloud API.' },
    { key: 'emailConfigured', label: 'Email (SMTP)', description: 'Outbound notification emails rendered from the Email Templates tab.' }
  ];

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.loadError.set(null);
    this.settingsService.getIntegrationStatus().subscribe({
      next: (response) => {
        this.loading.set(false);
        if (response.success && response.data) this.status.set(response.data);
      },
      error: (error) => {
        this.loading.set(false);
        this.loadError.set(error?.error?.message ?? 'Could not load integration status. Please try again.');
      }
    });
  }
}
