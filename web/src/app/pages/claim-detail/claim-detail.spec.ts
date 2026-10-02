import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { claimDetails, fakeAuth, fakeRealtime, settle } from '../../testing';
import { ClaimDetail } from './claim-detail';

describe('ClaimDetail', () => {
  let http: HttpTestingController;
  let realtime: ReturnType<typeof fakeRealtime>['fake'];

  async function render(claim = claimDetails()) {
    const fixture = TestBed.createComponent(ClaimDetail);
    fixture.componentRef.setInput('id', claim.id);
    fixture.detectChanges();
    http.expectOne(`/api/claims/${claim.id}`).flush(claim);
    http.expectOne(`/api/claims/${claim.id}/documents`).flush([]);
    http.expectOne(`/api/claims/${claim.id}/analysis`).flush(null);
    http
      .expectOne('/api/documents/settings')
      .flush({
        aiEnabled: true,
        aiModel: 'claude-opus-5-5',
        maxSizeBytes: 1,
        maxDocumentsPerClaim: 12,
        acceptedContentTypes: [],
      });
    http
      .expectOne('/api/claims/capabilities')
      .flush({ canDeclare: true, approvalLimit: 10_000, roles: ['claims.handler'] });
    await fixture.whenStable();
    return { fixture, element: fixture.nativeElement as HTMLElement };
  }

  const click = (element: HTMLElement, label: string) =>
    Array.from(element.querySelectorAll('button'))
      .find((b) => b.textContent?.includes(label))!
      .click();

  beforeEach(() => {
    const realtimeStub = fakeRealtime();
    realtime = realtimeStub.fake;
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        realtimeStub.provider,
        fakeAuth().provider,
      ],
    });
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('sends the version the user saw, so concurrent edits are detected', async () => {
    const { element, fixture } = await render(
      claimDetails({ status: 'Declared', allowedActions: ['StartReview'] }),
    );

    click(element, 'Prendre en charge');
    const request = http.expectOne('/api/claims/c1/actions');
    expect(request.request.body).toEqual({ action: 'StartReview', expectedVersion: 7 });
    request.flush(claimDetails({ status: 'UnderReview', version: 8 }));
    await fixture.whenStable();

    expect(element.querySelector('h1 app-status-badge')?.textContent).toContain('En instruction');
  });

  it('shows the conflict and reloads when someone else changed the claim', async () => {
    const { element, fixture } = await render(
      claimDetails({ status: 'Declared', allowedActions: ['StartReview'] }),
    );

    click(element, 'Prendre en charge');
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

  it('refetches only when a realtime notification concerns this claim', async () => {
    const { fixture } = await render();

    realtime.lastChange.set({
      claimId: 'other',
      number: 'SIN-X',
      status: 'Approved',
      actorName: 'K',
      occurredAt: '',
    });
    await settle();
    http.expectNone('/api/claims/c1');

    realtime.lastChange.set({
      claimId: 'c1',
      number: 'SIN-2026-000001',
      status: 'Approved',
      actorName: 'K',
      occurredAt: '',
    });
    await settle();
    http
      .expectOne('/api/claims/c1')
      .flush(claimDetails({ status: 'Approved', allowedActions: [] }));
    await fixture.whenStable();
    expect(fixture.nativeElement.querySelector('h1 app-status-badge')?.textContent).toContain(
      'Accepté',
    );
  });
});
