import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { TokenService } from '../auth/token.service';

/** Usage in routes: canActivate: [permissionGuard('booking.view')] */
export function permissionGuard(requiredPermission: string): CanActivateFn {
  return () => {
    const tokenService = inject(TokenService);
    const router = inject(Router);

    if (tokenService.hasPermission(requiredPermission)) {
      return true;
    }

    return router.createUrlTree(['/forbidden']);
  };
}
