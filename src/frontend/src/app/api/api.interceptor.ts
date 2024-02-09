import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { ApiAuthService } from './api-auth.service';

export const apiInterceptor: HttpInterceptorFn = (req, next) => {
  const apiAuth = inject(ApiAuthService);
  if (apiAuth.headerName === undefined || apiAuth.key === undefined) {
    return next(req);
  }

  const reqWithAuth = req.clone({
    setHeaders: { [apiAuth.headerName]: apiAuth.key },
  });

  return next(reqWithAuth);
};
