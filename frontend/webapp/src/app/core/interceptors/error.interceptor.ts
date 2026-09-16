import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../auth/auth.service';
import { TokenService } from '../auth/token.service';

const isAuthEndpoint = (url: string) =>
  url.includes('/auth/login') || url.includes('/auth/register') || url.includes('/auth/refresh');

/**
 * On a 401 from a non-auth endpoint, attempts one refresh-and-retry; if that also fails (or
 * there is no refresh token to try), logs out and sends the user back to login. This is a
 * single-retry strategy, not full concurrent-request deduplication — sufficient for the traffic
 * this app sees; revisit only if multiple simultaneous 401s in flight becomes an observed issue.
 */
export const errorInterceptor: HttpInterceptorFn = (request, next) => {
  const authService = inject(AuthService);
  const tokenService = inject(TokenService);
  const router = inject(Router);

  return next(request).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status !== 401 || isAuthEndpoint(request.url)) {
        return throwError(() => error);
      }

      if (!tokenService.getStoredRefreshToken()) {
        authService.logout();
        router.navigate(['/login']);
        return throwError(() => error);
      }

      return authService.refresh().pipe(
        switchMap(() =>
          next(request.clone({ setHeaders: { Authorization: `Bearer ${tokenService.accessToken()}` } }))
        ),
        catchError((refreshError) => {
          authService.logout();
          router.navigate(['/login']);
          return throwError(() => refreshError);
        })
      );
    })
  );
};
