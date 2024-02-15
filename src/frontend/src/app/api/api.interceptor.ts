import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { ToastController } from '@ionic/angular/standalone';
import { catchError, throwError } from 'rxjs';
import { ApiAuthStore } from './api-auth.store';

export const apiInterceptor: HttpInterceptorFn = (req, next) => {
  const apiAuth = inject(ApiAuthStore);
  const toastService = inject(ToastController);
  const router = inject(Router);

  if (apiAuth.headerName() && apiAuth.key()) {
    req = req.clone({
      setHeaders: { [apiAuth.headerName()!]: apiAuth.key()! },
    });
  }

  return next(req).pipe(
    catchError((err: HttpErrorResponse) => {
      if (err.status === 401) {
        toastService
          .create({
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
          })
          .then((toast) => toast.present());
      } else {
        toastService
          .create({
            message: `An unknown error occurred (${err.status})`,
            color: 'danger',
            duration: 5000,
            positionAnchor: 'tab-bar',
          })
          .then((toast) => toast.present());
      }

      return throwError(() => err);
    }),
  );
};
