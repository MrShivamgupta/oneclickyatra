import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api.models';

export interface CurrencyRates {
  base: string;
  asOf: string;
  rates: Record<string, number>;
}

export const OFFERED_CURRENCIES = ['INR', 'USD', 'EUR', 'GBP', 'AUD', 'CAD', 'SGD', 'AED', 'JPY'] as const;
export type OfferedCurrency = (typeof OFFERED_CURRENCIES)[number];

const STORAGE_KEY = 'ocy.publicCurrency';

/** Public-site currency switcher, display-only -- every price in this app is stored and charged
 * in INR; this only shows an approximate converted amount alongside it. */
@Injectable({ providedIn: 'root' })
export class CurrencyService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/currency`;

  readonly currencies = OFFERED_CURRENCIES;
  readonly selected = signal<OfferedCurrency>(this.readStoredCurrency());
  readonly rates = signal<CurrencyRates | null>(null);

  constructor() {
    this.http.get<ApiResponse<CurrencyRates>>(`${this.baseUrl}/rates`).subscribe({
      next: (response) => {
        if (response.success && response.data) this.rates.set(response.data);
      },
      error: () => {
        // A public display convenience -- if it fails to load, prices simply show INR only.
      }
    });
  }

  private readStoredCurrency(): OfferedCurrency {
    try {
      const stored = localStorage.getItem(STORAGE_KEY);
      return stored && (OFFERED_CURRENCIES as readonly string[]).includes(stored) ? (stored as OfferedCurrency) : 'INR';
    } catch {
      return 'INR';
    }
  }

  setSelected(currency: OfferedCurrency): void {
    this.selected.set(currency);
    try {
      localStorage.setItem(STORAGE_KEY, currency);
    } catch {
      // A per-viewer convenience, not critical state -- fine to skip persisting it.
    }
  }

  /** Converts an INR amount to the selected currency, or null if INR is selected (nothing extra
   * to show) or rates haven't loaded yet. */
  convert(amountInInr: number): number | null {
    const currency = this.selected();
    if (currency === 'INR') return null;
    const rate = this.rates()?.rates[currency];
    return rate == null ? null : amountInInr * rate;
  }

  /** Same as convert(), pre-formatted with the right currency symbol (e.g. "$96.40"). */
  format(amountInInr: number): string | null {
    const converted = this.convert(amountInInr);
    if (converted == null) return null;
    const currency = this.selected();
    try {
      return new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency,
        maximumFractionDigits: currency === 'JPY' ? 0 : 2
      }).format(converted);
    } catch {
      return `${currency} ${converted.toFixed(2)}`;
    }
  }
}
