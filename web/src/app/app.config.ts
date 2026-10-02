import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import {
  ApplicationConfig,
  DEFAULT_CURRENCY_CODE,
  provideBrowserGlobalErrorListeners,
} from '@angular/core';
import { provideRouter, withComponentInputBinding, withViewTransitions } from '@angular/router';
import { routes } from './app.routes';
import { apiBaseUrlInterceptor } from './core/api-base-url';
import { authInterceptor } from './core/auth/auth.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes, withComponentInputBinding(), withViewTransitions()),
    // Order matters: the auth interceptor recognises relative "/api/" URLs before the base URL is prepended.
    provideHttpClient(withFetch(), withInterceptors([authInterceptor, apiBaseUrlInterceptor])),
    // LOCALE_ID comes from the localized build (fr or en); amounts stay in euros in both languages.
    { provide: DEFAULT_CURRENCY_CODE, useValue: 'EUR' },
  ],
};
