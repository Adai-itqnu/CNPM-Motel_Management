import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.isLoggedIn()) {
    return true;
  }

  router.navigate(['/login']);
  return false;
};

export const adminGuard: CanActivateFn = () => {
  return roleGuard('admin');
};

export const tenantGuard: CanActivateFn = () => {
  return roleGuard('tenant');
};

function roleGuard(role: 'admin' | 'tenant'): boolean {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isLoggedIn()) {
    router.navigate(['/login']);
    return false;
  }

  const hasRole =
    role === 'admin' ? auth.isAdmin() : auth.isTenant();

  if (hasRole) {
    return true;
  }

  router.navigate(['/login']);
  return false;
}
