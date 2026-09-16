import { Injectable, signal } from '@angular/core';

const REFRESH_TOKEN_KEY = 'ocy.refreshToken';

interface DecodedAccessToken {
  sub: string;
  email: string;
  full_name: string;
  role?: string | string[];
  permission?: string | string[];
  exp: number;
}

@Injectable({ providedIn: 'root' })
export class TokenService {
  private readonly _accessToken = signal<string | null>(null);
  private readonly _roles = signal<string[]>([]);
  private readonly _permissions = signal<string[]>([]);
  private readonly _email = signal<string | null>(null);
  private readonly _fullName = signal<string | null>(null);

  readonly accessToken = this._accessToken.asReadonly();
  readonly roles = this._roles.asReadonly();
  readonly permissions = this._permissions.asReadonly();
  readonly email = this._email.asReadonly();
  readonly fullName = this._fullName.asReadonly();

  setSession(accessToken: string, refreshToken: string): void {
    this._accessToken.set(accessToken);
    const decoded = this.decode(accessToken);
    this._roles.set(this.toArray(decoded?.role));
    this._permissions.set(this.toArray(decoded?.permission));
    this._email.set(decoded?.email ?? null);
    this._fullName.set(decoded?.full_name ?? null);

    try {
      localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
    } catch {
      // localStorage can throw in a locked-down browser context; refresh-on-reload just won't work.
    }
  }

  clearSession(): void {
    this._accessToken.set(null);
    this._roles.set([]);
    this._permissions.set([]);
    this._email.set(null);
    this._fullName.set(null);
    try {
      localStorage.removeItem(REFRESH_TOKEN_KEY);
    } catch {
      // ignore
    }
  }

  getStoredRefreshToken(): string | null {
    try {
      return localStorage.getItem(REFRESH_TOKEN_KEY);
    } catch {
      return null;
    }
  }

  hasPermission(permission: string): boolean {
    return this._roles().includes('SuperAdmin') || this._permissions().includes(permission);
  }

  isAccessTokenExpired(): boolean {
    const token = this._accessToken();
    if (!token) {
      return true;
    }
    const decoded = this.decode(token);
    if (!decoded) {
      return true;
    }
    return decoded.exp * 1000 <= Date.now();
  }

  private decode(token: string): DecodedAccessToken | null {
    try {
      const payload = token.split('.')[1];
      const json = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
      return JSON.parse(json) as DecodedAccessToken;
    } catch {
      return null;
    }
  }

  private toArray(value: string | string[] | undefined): string[] {
    if (!value) {
      return [];
    }
    return Array.isArray(value) ? value : [value];
  }
}
