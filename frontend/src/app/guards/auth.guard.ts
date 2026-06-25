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

function isTokenExpired(token: string): boolean {
  try {
    const parts = token.split('.');
    if (parts.length !== 3) return true;
    const decoded = decodeBase64(parts[1]);
    const payload = JSON.parse(decoded);
    if (payload && payload.exp) {
      return Math.floor(Date.now() / 1000) >= payload.exp;
    }
    return false;
  } catch {
    return true;
  }
}

export const authGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const token = sessionStorage.getItem('token');

  if (token && !isTokenExpired(token)) {
    return true;
  }

  // Redirect to login, passing the original state URL as returnUrl query param
  return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
};
