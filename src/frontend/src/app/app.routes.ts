import { inject } from '@angular/core';
import { Router, Routes } from '@angular/router';
import { ApiAuthStore } from './api/api-auth.store';

export const routes: Routes = [
  {
    path: '',
    loadChildren: () => import('./tabs/tabs.routes').then((m) => m.routes),
    canActivate: [
      () => {
        const apiAuth = inject(ApiAuthStore);
        if (apiAuth.key() !== undefined && apiAuth.headerName() !== undefined) {
          return true;
        }

        return inject(Router).createUrlTree(['/sign-in']);
      },
    ],
  },
  {
    path: 'sign-in',
    loadComponent: () =>
      import('./sign-in/sign-in.page').then((m) => m.SignInPage),
  },
];
