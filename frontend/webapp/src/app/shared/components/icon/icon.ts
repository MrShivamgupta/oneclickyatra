import { Component, input } from '@angular/core';

/** The full set of icon names this app uses. Adding a new one means adding both a value here and
 * an @case in icon.html -- deliberately a closed set (not an arbitrary string) so a typo'd name
 * fails at compile time instead of silently rendering nothing. */
export type IconName =
  | 'grid'
  | 'user'
  | 'users'
  | 'inbox'
  | 'map-pin'
  | 'package'
  | 'sparkles'
  | 'file-text'
  | 'calendar-check'
  | 'credit-card'
  | 'receipt'
  | 'briefcase'
  | 'phone'
  | 'message-circle'
  | 'bar-chart'
  | 'star'
  | 'database'
  | 'id-badge'
  | 'clipboard-list'
  | 'settings'
  | 'check-circle'
  | 'currency'
  | 'menu'
  | 'search'
  | 'bell'
  | 'logout'
  | 'x-circle'
  | 'shield';

@Component({
  selector: 'app-icon',
  standalone: true,
  templateUrl: './icon.html',
  styles: [
    `
      :host {
        display: inline-flex;
        line-height: 0;
      }
      svg {
        width: 100%;
        height: 100%;
      }
    `
  ]
})
export class Icon {
  readonly name = input.required<IconName>();
}
