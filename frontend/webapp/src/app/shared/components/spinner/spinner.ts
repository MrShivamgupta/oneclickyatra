import { Component, input } from '@angular/core';

/** A small, theme-agnostic loading indicator (uses currentColor, so it inherits whatever text
 * color its container already has — safe to drop into admin/public/portal pages alike with no
 * token import). Use `inline` for a spinner next to a button/label, or the default block mode for
 * a standalone loading section (e.g. a whole page/tab body while its first fetch is in flight). */
@Component({
  selector: 'app-spinner',
  standalone: true,
  templateUrl: './spinner.html',
  styleUrl: './spinner.scss'
})
export class Spinner {
  readonly inline = input(false);
  readonly label = input('Loading…');
  /** Hide the text label and keep only the spinning icon (e.g. inside an already-labelled button). */
  readonly iconOnly = input(false);
}
