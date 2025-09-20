import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';

const TOKEN_KEY = 'qm.jwt';

export const authGuard: CanActivateFn = () => {
  const router = inject(Router);
  const token = localStorage.getItem(TOKEN_KEY);

  if (token && token.length > 0) {
    return true; // permitido
  }

  // si no hay token, redirige a /login
  router.navigateByUrl('/login');
  return false;
};
