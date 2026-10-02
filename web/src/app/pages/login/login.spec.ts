import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { KARIM, LEA, settle } from '../../testing';
import { Login } from './login';

describe('Login', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    sessionStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    });
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('lists the personas and logs in with one click', async () => {
    const fixture = TestBed.createComponent(Login);
    fixture.detectChanges();
    http.expectOne('/api/auth/personas').flush([LEA, KARIM]);
    await fixture.whenStable();
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    const element = fixture.nativeElement as HTMLElement;

    expect(
      Array.from(element.querySelectorAll('.persona .chip')).map((chip) => chip.textContent),
    ).toEqual(['Gestionnaire', 'Responsable']);
    (element.querySelector('button.persona') as HTMLButtonElement).click();
    http
      .expectOne('/api/auth/token')
      .flush({ accessToken: 'jwt', expiresAt: '2999-01-01T00:00:00Z', user: LEA });
    await settle();

    expect(navigate).toHaveBeenCalledWith(['/dashboard']);
  });

  it('offers a retry while the demo API wakes up', async () => {
    const fixture = TestBed.createComponent(Login);
    fixture.detectChanges();
    http.expectOne('/api/auth/personas').flush(null, { status: 503, statusText: 'Unavailable' });
    await fixture.whenStable();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain('se réveille');
  });
});
