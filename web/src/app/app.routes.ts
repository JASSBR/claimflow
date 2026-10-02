import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    title: 'ClaimFlow — démo',
    canActivate: [guestGuard],
    loadComponent: () => import('./pages/login/login').then((m) => m.Login),
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./layout/shell').then((m) => m.Shell),
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      {
        path: 'dashboard',
        title: 'Tableau de bord · ClaimFlow',
        loadComponent: () => import('./pages/dashboard/dashboard').then((m) => m.Dashboard),
      },
      {
        path: 'claims',
        title: 'Sinistres · ClaimFlow',
        loadComponent: () => import('./pages/claims-list/claims-list').then((m) => m.ClaimsList),
      },
      {
        path: 'claims/new',
        title: 'Déclarer un sinistre · ClaimFlow',
        loadComponent: () =>
          import('./pages/claim-declare/claim-declare').then((m) => m.ClaimDeclare),
      },
      {
        path: 'claims/:id',
        title: 'Sinistre · ClaimFlow',
        loadComponent: () => import('./pages/claim-detail/claim-detail').then((m) => m.ClaimDetail),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
