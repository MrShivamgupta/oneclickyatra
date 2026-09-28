import { Component, effect, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-filter-bar',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './filter-bar.html',
  styleUrl: './filter-bar.scss',
  host: {
    '[class.theme-public]': 'variant() === "public"'
  }
})
export class FilterBar {
  readonly placeholder = input('Search…');
  /** Pre-fills the search box, e.g. when the page was navigated to with a ?searchTerm= query param.
   * Kept in sync for the box's whole lifetime (not just on creation), so a parent can also drive the
   * search term programmatically later on -- e.g. clicking a "filter by X" control elsewhere on the
   * page -- and have the box's own text visibly reflect it. */
  readonly initialValue = input('');
  readonly variant = input<'admin' | 'public'>('admin');
  readonly searchChange = output<string>();

  private debounceHandle: ReturnType<typeof setTimeout> | undefined;
  searchTerm = '';

  constructor() {
    effect(() => {
      this.searchTerm = this.initialValue();
    });
  }

  onInput(): void {
    clearTimeout(this.debounceHandle);
    this.debounceHandle = setTimeout(() => this.searchChange.emit(this.searchTerm.trim()), 350);
  }
}
