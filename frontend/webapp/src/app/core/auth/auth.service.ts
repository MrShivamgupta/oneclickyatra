import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject } from '@angular/core';
import { catchError, map, Observable, of, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api.models';
import { AuthenticatedUser, LoginRequest, RegisterRequest } from './models/auth.models';
import { TokenService } from './token.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly tokenService = inject(TokenService);

  readonly isAuthenticated = computed(() => this.tokenService.accessToken() !== null);
  readonly currentUser = computed(() => {
    const email = this.tokenService.email();
    if (!email) {
      return null;
    }
    return {
      email,
      fullName: this.tokenService.fullName() ?? '',
      roles: this.tokenService.roles()
    };
  });

  login(request: LoginRequest): Observable<ApiResponse<AuthenticatedUser>> {
    return this.http
      .post<ApiResponse<AuthenticatedUser>>(`${environment.apiBaseUrl}/auth/login`, request)
      .pipe(tap((response) => this.storeSession(response)));
  }

  register(request: RegisterRequest): Observable<ApiResponse<AuthenticatedUser>> {
    return this.http
      .post<ApiResponse<AuthenticatedUser>>(`${environment.apiBaseUrl}/auth/register`, request)
      .pipe(tap((response) => this.storeSession(response)));
  }

  refresh(): Observable<ApiResponse<AuthenticatedUser>> {
    const refreshToken = this.tokenService.getStoredRefreshToken();
    if (!refreshToken) {
      return of({ success: false, data: null, message: 'No refresh token.', trackingId: '' });
    }
    return this.http
      .post<ApiResponse<AuthenticatedUser>>(`${environment.apiBaseUrl}/auth/refresh`, { refreshToken })
      .pipe(tap((response) => this.storeSession(response)));
  }

  /** Called once at startup — reloads the access token when a refresh token is still valid. */
  initializeSession(): Observable<boolean> {
    if (this.isAuthenticated()) {
      return of(true);
    }

    if (!this.tokenService.getStoredRefreshToken()) {
      return of(false);
    }

    return this.refresh().pipe(
      map((response) => response.success && response.data !== null),
      catchError(() => {
        this.logout();
        return of(false);
      })
    );
  }

  forgotPassword(email: string): Observable<ApiResponse<null>> {
    return this.http.post<ApiResponse<null>>(`${environment.apiBaseUrl}/auth/forgot-password`, { email });
  }

  resetPassword(email: string, resetToken: string, newPassword: string): Observable<ApiResponse<null>> {
    return this.http.post<ApiResponse<null>>(`${environment.apiBaseUrl}/auth/reset-password`, {
      email,
      resetToken,
      newPassword
    });
  }

  logout(): void {
    this.tokenService.clearSession();
  }

  private storeSession(response: ApiResponse<AuthenticatedUser>): void {
    if (response.success && response.data) {
      this.tokenService.setSession(response.data.accessToken, response.data.refreshToken);
    }
  }
}
