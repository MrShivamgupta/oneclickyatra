import { inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { AuthService } from './auth.service';

/** Restores an in-memory session from the stored refresh token before the first route activates. */
export function authInitializer(): Promise<boolean> {
  return firstValueFrom(inject(AuthService).initializeSession());
}
