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
  readonly danger = input(true);

  readonly confirmed = output<void>();
  readonly cancelled = output<void>();
}
