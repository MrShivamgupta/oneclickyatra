import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-confirmation-dialog',
  standalone: true,
  templateUrl: './confirmation-dialog.html',
  styleUrl: './confirmation-dialog.scss'
})
export class ConfirmationDialog {
  readonly open = input.required<boolean>();
  readonly title = input('Are you sure?');
  readonly message = input('This action cannot be undone.');
  readonly confirmLabel = input('Confirm');
  readonly confirmingLabel = input('Working…');
  readonly danger = input(true);
  /** Set true while the confirm action's request is in flight — disables both buttons and swaps
   * the confirm button's label to confirmingLabel(), so a slow or failing delete/action can't be
   * double-fired and gives the user visible feedback that something is happening. */
  readonly confirming = input(false);
  /** Shown inside the dialog (and keeps it open) when the last confirm attempt failed. */
  readonly errorMessage = input<string | null>(null);

  readonly confirmed = output<void>();
  readonly cancelled = output<void>();
}
