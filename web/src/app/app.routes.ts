import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'claims' },
  {
    path: 'claims',
    title: 'Sinistres · ClaimFlow',
    loadComponent: () =>
      import('./claims/claims-dashboard/claims-dashboard').then((m) => m.ClaimsDashboard),
  },
  {
    path: 'claims/new',
    title: 'Déclarer un sinistre · ClaimFlow',
    loadComponent: () => import('./claims/claim-declare/claim-declare').then((m) => m.ClaimDeclare),
  },
  {
    path: 'claims/:id',
    title: 'Sinistre · ClaimFlow',
    loadComponent: () => import('./claims/claim-detail/claim-detail').then((m) => m.ClaimDetail),
  },
  { path: '**', redirectTo: 'claims' },
];
