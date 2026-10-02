import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { Auth } from './auth';

/** Adds the bearer token to API calls only, and sends the user back to the login screen when it expires. */
export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const auth = inject(Auth);
  const token = auth.accessToken();
  const isApiCall = request.url.startsWith('/api/') && !request.url.startsWith('/api/auth/');
  const authorized =
    token && isApiCall
      ? request.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
      : request;

  return next(authorized).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse && error.status === 401 && isApiCall) {
        auth.logout();
      }
      return throwError(() => error);
    }),
  );
};
