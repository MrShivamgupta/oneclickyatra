import { Component, signal } from '@angular/core';
import { AgencyProfileTab } from './agency-profile-tab/agency-profile-tab';
import { EmailTemplatesTab } from './email-templates-tab/email-templates-tab';
import { IntegrationStatusTab } from './integration-status-tab/integration-status-tab';

type TabKey = 'agency-profile' | 'email-templates' | 'integrations';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [AgencyProfileTab, EmailTemplatesTab, IntegrationStatusTab],
  templateUrl: './settings.html',
  styleUrl: './settings.scss'
})
export class Settings {
  readonly activeTab = signal<TabKey>('agency-profile');

  readonly tabs: { key: TabKey; label: string }[] = [
    { key: 'agency-profile', label: 'Agency Profile' },
    { key: 'email-templates', label: 'Email Templates' },
    { key: 'integrations', label: 'Integrations' }
  ];

  selectTab(tab: TabKey): void {
    this.activeTab.set(tab);
  }
}
