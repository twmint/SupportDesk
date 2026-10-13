import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-confirm-dialog',
  styleUrl: './confirm-dialog.scss',
  templateUrl: './confirm-dialog.html',
  host: {
    '(document:keydown.escape)': 'onEscape()',
  },
})
export class ConfirmDialog {
  open = input(false);
  title = input('Are you sure?');
  message = input('');
  confirmLabel = input('Confirm');
  busy = input(false);

  confirmed = output<void>();
  cancelled = output<void>();

  protected onEscape() {
    if (this.open() && !this.busy()) this.cancelled.emit();
  }
}
