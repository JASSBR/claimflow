import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { API_BASE_URL, apiBaseUrlInterceptor } from '../api-base-url';
import { Auth } from './auth';
import { authInterceptor } from './auth.interceptor';

describe('Auth', () => {
  let http: HttpTestingController;

  function setup(baseUrl = '') {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor, apiBaseUrlInterceptor])),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: API_BASE_URL, useValue: baseUrl },
      ],
    });
    http = TestBed.inject(HttpTestingController);
    return TestBed.inject(Auth);
  }

  beforeEach(() => sessionStorage.clear());
  afterEach(() => http.verify());

  it('stores the demo session and exposes the user', () => {
    const auth = setup();
    auth.loginAs('lea').subscribe();

    const request = http.expectOne('/api/auth/token');
    expect(request.request.body).toEqual({ personaId: 'lea' });
    expect(request.request.headers.has('Authorization')).toBe(false);
    request.flush({
      accessToken: 'jwt',
      expiresAt: '2999-01-01T00:00:00Z',
      user: { id: 'lea', name: 'Léa Martin', title: 't', summary: '', roles: ['claims.auditor'] },
    });

    expect(auth.isAuthenticated()).toBe(true);
    expect(auth.isAuditor()).toBe(true);
    expect(JSON.parse(sessionStorage.getItem('claimflow.session')!).accessToken).toBe('jwt');
  });

  it('ignores an expired stored session', () => {
    sessionStorage.setItem(
      'claimflow.session',
      JSON.stringify({ accessToken: 'old', expiresAt: '2000-01-01T00:00:00Z', user: {} }),
    );
    expect(setup().isAuthenticated()).toBe(false);
  });

  it('sends the bearer token to the API only, and logs out on 401', () => {
    sessionStorage.setItem(
      'claimflow.session',
      JSON.stringify({
        accessToken: 'jwt',
        expiresAt: '2999-01-01T00:00:00Z',
        user: { name: 'L' },
      }),
    );
    const auth = setup();
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    const client = TestBed.inject(HttpClient);

    client.get('https://example.org/other').subscribe();
    expect(http.expectOne('https://example.org/other').request.headers.has('Authorization')).toBe(
      false,
    );

    client.get('/api/claims').subscribe({ error: () => undefined });
    const api = http.expectOne('/api/claims');
    expect(api.request.headers.get('Authorization')).toBe('Bearer jwt');
    api.flush(null, { status: 401, statusText: 'Unauthorized' });

    expect(auth.isAuthenticated()).toBe(false);
    expect(navigate).toHaveBeenCalledWith(['/login']);
  });

  it('prefixes relative URLs with the configured API origin', () => {
    setup('https://api.example.org');
    TestBed.inject(HttpClient).get('/api/auth/personas').subscribe();
    http.expectOne('https://api.example.org/api/auth/personas').flush([]);
  });
});
