import { Component, input, output, OnInit } from '@angular/core';
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
export class FilterBar implements OnInit {
  readonly placeholder = input('Search…');
  /** Pre-fills the search box, e.g. when the page was navigated to with a ?searchTerm= query param. */
  readonly initialValue = input('');
  readonly variant = input<'admin' | 'public'>('admin');
  readonly searchChange = output<string>();

  private debounceHandle: ReturnType<typeof setTimeout> | undefined;
  searchTerm = '';

  ngOnInit(): void {
    this.searchTerm = this.initialValue();
  }

  onInput(): void {
    clearTimeout(this.debounceHandle);
    this.debounceHandle = setTimeout(() => this.searchChange.emit(this.searchTerm.trim()), 350);
  }
}
