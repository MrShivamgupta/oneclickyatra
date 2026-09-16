import { Component, signal } from '@angular/core';
import { CountriesTab } from './countries-tab/countries-tab';
import { CitiesTab } from './cities-tab/cities-tab';
import { CategoriesTab } from './categories-tab/categories-tab';
import { SeasonsTab } from './seasons-tab/seasons-tab';

type TabKey = 'countries' | 'cities' | 'categories' | 'seasons';

@Component({
  selector: 'app-master-data',
  standalone: true,
  imports: [CountriesTab, CitiesTab, CategoriesTab, SeasonsTab],
  templateUrl: './master-data.html',
  styleUrl: './master-data.scss'
})
export class MasterData {
  readonly activeTab = signal<TabKey>('countries');

  readonly tabs: { key: TabKey; label: string }[] = [
    { key: 'countries', label: 'Countries' },
    { key: 'cities', label: 'Cities' },
    { key: 'categories', label: 'Categories' },
    { key: 'seasons', label: 'Seasons' }
  ];

  selectTab(tab: TabKey): void {
    this.activeTab.set(tab);
  }
}
