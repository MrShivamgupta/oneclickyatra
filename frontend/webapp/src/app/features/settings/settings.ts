import { Component, signal } from '@angular/core';
import { EmailTemplatesTab } from './email-templates-tab/email-templates-tab';
import { IntegrationStatusTab } from './integration-status-tab/integration-status-tab';

type TabKey = 'email-templates' | 'integrations';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [EmailTemplatesTab, IntegrationStatusTab],
  templateUrl: './settings.html',
  styleUrl: './settings.scss'
})
export class Settings {
  readonly activeTab = signal<TabKey>('email-templates');

  readonly tabs: { key: TabKey; label: string }[] = [
    { key: 'email-templates', label: 'Email Templates' },
    { key: 'integrations', label: 'Integrations' }
  ];

  selectTab(tab: TabKey): void {
    this.activeTab.set(tab);
  }
}
