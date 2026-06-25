import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

function decodeBase64(str: string): string {
  try {
    const base64 = str.replace(/-/g, '+').replace(/_/g, '/');
    return decodeURIComponent(
      atob(base64)
        .split('')
        .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    );
  } catch {
    return atob(str.replace(/-/g, '+').replace(/_/g, '/'));
  }
}

function getRoleFromToken(token: string): string | null {
  try {
    const parts = token.split('.');
    if (parts.length !== 3) return null;
    const decoded = decodeBase64(parts[1]);
    const payload = JSON.parse(decoded);
    return payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || payload['role'] || null;
  } catch {
    return null;
  }
}

function normalizeRole(role: string): string {
  if (!role) return '';
  const r = role.trim().toUpperCase();
  if (r === 'ADMIN') return 'ADMIN';
  if (r === 'VENDOR') return 'VENDOR';
  if (r === 'CUSTOMER') return 'CUSTOMER';
  return r;
}

export const roleGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const token = sessionStorage.getItem('token');

  if (!token) {
    return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
  }

  const userRole = getRoleFromToken(token);
  if (!userRole) {
    return router.createUrlTree(['/login']);
  }

  const allowedRoles = route.data?.['roles'] as string[];
  if (!allowedRoles || allowedRoles.length === 0) {
    return true;
  }

  const normalizedUserRole = normalizeRole(userRole);
  const isAuthorized = allowedRoles.some(role => normalizeRole(role) === normalizedUserRole);

  if (isAuthorized) {
    return true;
  }

  return router.createUrlTree(['/access-denied']);
};
