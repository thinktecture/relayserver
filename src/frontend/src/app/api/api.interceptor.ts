import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { ToastController } from '@ionic/angular/standalone';
import { catchError } from 'rxjs';
import { ApiAuthService } from './api-auth.service';

export const apiInterceptor: HttpInterceptorFn = (req, next) => {
  const apiAuth = inject(ApiAuthService);
  if (apiAuth.headerName === undefined || apiAuth.key === undefined) {
    return next(req);
  }

  const reqWithAuth = req.clone({
    setHeaders: { [apiAuth.headerName]: apiAuth.key },
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
        });
      } else {
        toast = await toastService.create({
          message: `An unknown error occurred (${err.status})`,
          color: 'danger',
          duration: 5000,
        });
      }

      await toast.present();

      throw err;
    }),
  );
};
