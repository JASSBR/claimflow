import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  TestRequest,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { ClaimStats } from '../../claims/claim.models';
import { claimDetails, fakeRealtime, settle } from '../../testing';
import { ClaimsList } from './claims-list';

const STATS: ClaimStats = {
  countByStatus: {
    Declared: 3,
    UnderReview: 2,
    InformationRequested: 1,
    Approved: 1,
    Rejected: 0,
    Settled: 4,
  },
  byType: {
    Auto: { count: 5, claimedAmount: 9_000 },
    Home: { count: 4, claimedAmount: 20_000 },
    Liability: { count: 2, claimedAmount: 1_000 },
  },
  totalClaimedAmount: 30_000,
  totalApprovedAmount: 9_000,
};

describe('ClaimsList', () => {
  let http: HttpTestingController;
  let realtime: ReturnType<typeof fakeRealtime>['fake'];

  const list = (): TestRequest => http.expectOne((r) => r.url === '/api/claims');

  async function render(status?: string) {
    const fixture = TestBed.createComponent(ClaimsList);
    if (status) fixture.componentRef.setInput('status', status);
    fixture.detectChanges();
    const first = list();
    first.flush({ items: [claimDetails()], page: 1, pageSize: 10, totalCount: 25 });
    http.expectOne('/api/claims/stats').flush(STATS);
    http
      .expectOne('/api/claims/capabilities')
      .flush({ canDeclare: true, approvalLimit: 10_000, roles: [] });
    await fixture.whenStable();
    return { fixture, element: fixture.nativeElement as HTMLElement, first };
  }

  beforeEach(() => {
    const stub = fakeRealtime();
    realtime = stub.fake;
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        stub.provider,
      ],
    });
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('starts from the status in the URL (e.g. a click in the dashboard legend)', async () => {
    const { first, element } = await render('Settled');
    expect(first.request.params.get('status')).toBe('Settled');
    expect(element.querySelector('.tile.active .tile-label')?.textContent).toBe('Indemnisé');
  });

  it('filters by status, then clears it on second click', async () => {
    const { element } = await render();
    const tile = () =>
      Array.from(element.querySelectorAll<HTMLButtonElement>('.tile')).find((t) =>
        t.textContent?.includes('En instruction'),
      )!;

    tile().click();
    await settle();
    const filtered = list();
    expect(filtered.request.params.get('status')).toBe('UnderReview');
    filtered.flush({ items: [], page: 1, pageSize: 10, totalCount: 0 });
    await settle();

    tile().click();
    await settle();
    expect(list().request.params.has('status')).toBe(false);
  });

  it('searches from page one after paging', async () => {
    const { element } = await render();
    element.querySelector<HTMLButtonElement>('.pager button:last-of-type')!.click();
    await settle();
    expect(list().request.params.get('page')).toBe('2');

    const search = element.querySelector<HTMLInputElement>('input[type=search]')!;
    search.value = 'pol-1042';
    search.dispatchEvent(new Event('input'));
    await settle();
    const request = list();
    expect(request.request.params.get('search')).toBe('pol-1042');
    expect(request.request.params.get('page')).toBe('1');
  });

  it('refetches list and counters when any claim changes', async () => {
    await render();
    realtime.lastChange.set({
      claimId: 'x',
      number: 'SIN-X',
      status: 'Settled',
      actorName: 'K',
      occurredAt: '',
    });
    await settle();
    list().flush({ items: [], page: 1, pageSize: 10, totalCount: 0 });
    http.expectOne('/api/claims/stats').flush(STATS);
  });
});
