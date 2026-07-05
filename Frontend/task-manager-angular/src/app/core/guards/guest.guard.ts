import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Blocks already-logged-in users from seeing the login/register screens —
 * sends them straight to their dashboard instead.
 */
export const guestGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.isLoggedIn()) {
    return router.createUrlTree(['/dashboard']);
  }
  return true;
};

/**
 * Root path ('/') redirect: if a session already exists, go straight to the
 * dashboard (which shows admin or employee view automatically). Otherwise,
 * go to the login page.
 */
export const rootRedirectGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  return router.createUrlTree([auth.isLoggedIn() ? '/dashboard' : '/login']);
};
