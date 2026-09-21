import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../auth/auth.service';
import { TokenService } from '../auth/token.service';
import { ToastService } from '../services/toast.service';

const isAuthEndpoint = (url: string) =>
  url.includes('/auth/login') || url.includes('/auth/register') || url.includes('/auth/refresh');

/** 401 is handled by the refresh-and-retry flow below; 422 (validation) is always shown inline
 * next to the offending form field by the page itself — a toast for either would be redundant or,
 * for 401, would flash right before the redirect to /login. Everything else falls through to the
 * toast safety net so a page that forgot its own error handler still shows the user *something*. */
const shouldToast = (status: number) => status !== 401 && status !== 422;

const toastMessageFor = (error: HttpErrorResponse): string => {
  const backendMessage = (error.error as { message?: string } | null)?.message;
  if (backendMessage) {
    return backendMessage;
  }
  if (error.status === 0) {
    return 'Could not reach the server. Check your connection and try again.';
  }
  return 'Something went wrong. Please try again.';
};

/**
 * On a 401 from a non-auth endpoint, attempts one refresh-and-retry; if that also fails (or
 * there is no refresh token to try), logs out and sends the user back to login.
 *
 * Uses AuthService.refreshOnce() rather than refresh() so that when a single page load fires
 * several parallel API calls and the access token has expired, all of the resulting 401s share
 * one refresh instead of each triggering its own — refresh tokens are single-use/rotated with
 * reuse-detection server-side, so N concurrent refresh() calls for the same expired token used to
 * mean only the first succeeded while the rest were treated as token reuse and revoked the whole
 * session, forcibly logging the user out moments after a perfectly valid login.
 */
export const errorInterceptor: HttpInterceptorFn = (request, next) => {
  const authService = inject(AuthService);
  const tokenService = inject(TokenService);
  const toastService = inject(ToastService);
  const router = inject(Router);

  return next(request).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status !== 401 || isAuthEndpoint(request.url)) {
        if (shouldToast(error.status)) {
          toastService.show(toastMessageFor(error));
        }
        return throwError(() => error);
      }

      if (!tokenService.getStoredRefreshToken()) {
        authService.logout();
        router.navigate(['/login']);
        return throwError(() => error);
      }

      return authService.refreshOnce().pipe(
        switchMap((response) => {
          if (!response.success) {
            throw response;
          }
          return next(request.clone({ setHeaders: { Authorization: `Bearer ${tokenService.accessToken()}` } }));
        }),
        catchError((refreshError) => {
          authService.logout();
          router.navigate(['/login']);
          return throwError(() => refreshError);
        })
      );
    })
  );
};
