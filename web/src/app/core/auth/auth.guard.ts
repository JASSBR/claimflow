import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { Auth } from './auth';

export const authGuard: CanActivateFn = () =>
  inject(Auth).isAuthenticated() || inject(Router).createUrlTree(['/login']);

export const guestGuard: CanActivateFn = () =>
  !inject(Auth).isAuthenticated() || inject(Router).createUrlTree(['/dashboard']);
