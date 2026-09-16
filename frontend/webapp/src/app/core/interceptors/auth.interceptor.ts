import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { TokenService } from '../auth/token.service';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const tokenService = inject(TokenService);
  const accessToken = tokenService.accessToken();

  if (!accessToken || request.url.includes('/auth/login') || request.url.includes('/auth/register') || request.url.includes('/auth/refresh')) {
    return next(request);
  }

  return next(request.clone({ setHeaders: { Authorization: `Bearer ${accessToken}` } }));
};
