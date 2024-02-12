import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { ToastController } from '@ionic/angular/standalone';
import { catchError } from 'rxjs';
import { ApiAuthStore } from './api-auth.store';

export const apiInterceptor: HttpInterceptorFn = (req, next) => {
  const apiAuth = inject(ApiAuthStore);
  if (apiAuth.headerName() === undefined || apiAuth.key() === undefined) {
    return next(req);
  }

  const reqWithAuth = req.clone({
    setHeaders: { [apiAuth.headerName()!]: apiAuth.key()! },
  });

  const toastService = inject(ToastController);
  const router = inject(Router);
  return next(reqWithAuth).pipe(
    catchError(async (err: HttpErrorResponse) => {
      let toast;
      if (err.status === 401) {
        toast = await toastService.create({
          message: `Unauthorized (${err.status})`,
          color: 'danger',
          duration: 5000,
          buttons: [
            {
              text: 'Set API key',
              handler: () => router.navigate(['/sign-in']),
            },
          ],
          positionAnchor: 'tab-bar',
        });
      } else {
        toast = await toastService.create({
          message: `An unknown error occurred (${err.status})`,
          color: 'danger',
          duration: 5000,
          positionAnchor: 'tab-bar',
        });
      }

      await toast.present();

      throw err;
    }),
  );
};
