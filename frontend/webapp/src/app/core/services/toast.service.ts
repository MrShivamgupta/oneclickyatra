import { Injectable, signal } from '@angular/core';

export type ToastType = 'error' | 'success' | 'info';

export interface ToastMessage {
  id: number;
  type: ToastType;
  text: string;
}

const AUTO_DISMISS_MS = 6000;

/** App-wide toast queue. This is the SAFETY NET, not the primary error-handling mechanism — pages
 * should still show their own inline error state where one is more useful (e.g. next to the field
 * or button that failed). error.interceptor.ts calls show('error', ...) for any HTTP failure a
 * page didn't already report itself, so nothing fails completely silently by default. */
@Injectable({ providedIn: 'root' })
export class ToastService {
  private readonly _toasts = signal<ToastMessage[]>([]);
  readonly toasts = this._toasts.asReadonly();
  private nextId = 1;

  show(text: string, type: ToastType = 'error'): void {
    const id = this.nextId++;
    this._toasts.update((toasts) => [...toasts, { id, type, text }]);
    setTimeout(() => this.dismiss(id), AUTO_DISMISS_MS);
  }

  dismiss(id: number): void {
    this._toasts.update((toasts) => toasts.filter((toast) => toast.id !== id));
  }
}
