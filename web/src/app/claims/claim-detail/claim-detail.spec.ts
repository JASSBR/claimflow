import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { claimDetails, fakeRealtime, settle } from '../testing';
import { ClaimDetail } from './claim-detail';

describe('ClaimDetail', () => {
  let http: HttpTestingController;
  let realtime: ReturnType<typeof fakeRealtime>['fake'];

  async function render(claim = claimDetails()) {
    const fixture = TestBed.createComponent(ClaimDetail);
    fixture.componentRef.setInput('id', claim.id);
    fixture.detectChanges();
    http.expectOne(`/api/claims/${claim.id}`).flush(claim);
    await fixture.whenStable();
    return { fixture, element: fixture.nativeElement as HTMLElement };
  }

  const buttons = (element: HTMLElement) =>
    Array.from(element.querySelectorAll<HTMLButtonElement>('section button')).map((b) =>
      b.textContent?.trim(),
    );

  beforeEach(() => {
    const realtimeStub = fakeRealtime();
    realtime = realtimeStub.fake;
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        realtimeStub.provider,
      ],
    });
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('renders one button per action the server allows, and nothing else', async () => {
    const { element } = await render();

    expect(buttons(element)).toEqual(['Demander des pièces', 'Accepter', 'Refuser']);
  });

  it('sends the version the user saw, so concurrent edits are detected', async () => {
    const { element, fixture } = await render(
      claimDetails({ status: 'Declared', allowedActions: ['StartReview'] }),
    );

    element.querySelector<HTMLButtonElement>('section button')!.click();
    const request = http.expectOne('/api/claims/c1/actions');
    expect(request.request.body).toMatchObject({ action: 'StartReview', expectedVersion: 7 });
    request.flush(claimDetails({ status: 'UnderReview', version: 8 }));
    await fixture.whenStable();

    expect(element.querySelector('app-status-badge')?.textContent).toContain('En instruction');
  });

  it('asks for a reason before rejecting', async () => {
    const { element, fixture } = await render();

    Array.from(element.querySelectorAll<HTMLButtonElement>('section button'))
      .find((b) => b.textContent?.includes('Refuser'))!
      .click();
    await fixture.whenStable();

    expect(element.querySelector('textarea')).not.toBeNull();
    http.expectNone('/api/claims/c1/actions');
  });

  it('shows the conflict and reloads when someone else changed the claim', async () => {
    const { element, fixture } = await render(
      claimDetails({ status: 'Declared', allowedActions: ['StartReview'] }),
    );

    element.querySelector<HTMLButtonElement>('section button')!.click();
    http
      .expectOne('/api/claims/c1/actions')
      .flush(
        { title: 'The claim was modified by someone else.' },
        { status: 409, statusText: 'Conflict' },
      );
    await settle();

    http.expectOne('/api/claims/c1').flush(claimDetails({ version: 9 }));
    await fixture.whenStable();
    expect(element.querySelector('[role=alert]')?.textContent).toContain(
      'modified by someone else',
    );
  });

  it('refetches when a realtime notification concerns this claim only', async () => {
    const { fixture } = await render();

    realtime.lastChange.set({ claimId: 'other', number: 'SIN-X', status: 'Approved' });
    await settle();
    http.expectNone('/api/claims/c1');

    realtime.lastChange.set({ claimId: 'c1', number: 'SIN-2026-0000ABCD', status: 'Approved' });
    await settle();
    http
      .expectOne('/api/claims/c1')
      .flush(claimDetails({ status: 'Approved', allowedActions: ['Settle'] }));
    await fixture.whenStable();
    expect(fixture.nativeElement.querySelector('app-status-badge')?.textContent).toContain(
      'Accepté',
    );
  });
});
