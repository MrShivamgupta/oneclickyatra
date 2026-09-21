import { Component, inject } from '@angular/core';
import { ToastService } from '../../../core/services/toast.service';

/** Mounted once at the app root (app.html) — renders whatever ToastService currently holds. */
@Component({
  selector: 'app-toast',
  standalone: true,
  templateUrl: './toast.html',
  styleUrl: './toast.scss'
})
export class Toast {
  protected readonly toastService = inject(ToastService);
}
