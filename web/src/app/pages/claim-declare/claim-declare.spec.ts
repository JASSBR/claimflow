import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { claimDetails, settle } from '../../testing';
import { ClaimDeclare } from './claim-declare';

describe('ClaimDeclare', () => {
  let http: HttpTestingController;

  async function render() {
    const fixture = TestBed.createComponent(ClaimDeclare);
    await fixture.whenStable();
    const element = fixture.nativeElement as HTMLElement;
    const type = (selector: string, value: string) => {
      const input = element.querySelector<HTMLInputElement | HTMLTextAreaElement>(selector)!;
      input.value = value;
      input.dispatchEvent(new Event('input'));
    };
    const submit = async () => {
      element.querySelector('form')!.dispatchEvent(new Event('submit'));
      await fixture.whenStable();
    };
    const fillValid = () => {
      type('input:not([type])', 'POL-104233');
      type('input[type=number]', '850');
      type('textarea', 'Windscreen cracked by gravel.');
    };
    return { fixture, element, submit, fillValid };
  }

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    });
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('blocks submission and shows every client-side rule', async () => {
    const { element, submit } = await render();
    await submit();

    http.expectNone('/api/claims');
    const messages = Array.from(element.querySelectorAll('.field-error')).map((e) => e.textContent);
    expect(messages).toEqual(
      expect.arrayContaining([
        'Le numéro de contrat est obligatoire.',
        'Les circonstances sont obligatoires.',
        'Le montant doit être positif.',
      ]),
    );
  });

  it('posts a valid declaration and opens the created claim', async () => {
    const { submit, fillValid, fixture } = await render();
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);

    fillValid();
    await submit();
    const request = http.expectOne('/api/claims');
    expect(request.request.body).toMatchObject({
      policyNumber: 'POL-104233',
      claimedAmount: 850,
      type: 'Auto',
    });
    request.flush(claimDetails({ id: 'new-id' }));
    await fixture.whenStable();

    expect(navigate).toHaveBeenCalledWith(['/claims', 'new-id']);
  });

  it('surfaces business rules only the server knows about', async () => {
    const { element, submit, fillValid, fixture } = await render();
    fillValid();
    await submit();

    http.expectOne('/api/claims').flush(
      {
        errors: {
          'claim.incident_time_barred': ['Claims are time-barred two years after the incident.'],
        },
      },
      { status: 400, statusText: 'Bad Request' },
    );
    await settle();
    await fixture.whenStable();

    // The API sends a stable code; the UI words it in the user's language.
    expect(element.querySelector('[role=alert]')?.textContent).toContain('Sinistre prescrit');
  });
});
