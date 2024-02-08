import { HttpInterceptorFn } from '@angular/common/http';

export const apiInterceptor: HttpInterceptorFn = (req, next) => {
  const reqWithAuth = req.clone({
    setHeaders: { 'TT-Api-Key': 'readwrite-key' },
  });

  return next(reqWithAuth);
};
