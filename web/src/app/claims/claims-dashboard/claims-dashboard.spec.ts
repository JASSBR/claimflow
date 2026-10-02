import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  TestRequest,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { ClaimStats, ClaimSummary, PagedResponse } from '../claim.models';
import { claimDetails, fakeRealtime, settle } from '../testing';
import { ClaimsDashboard } from './claims-dashboard';

const stats: ClaimStats = {
  countByStatus: {
    Declared: 3,
    UnderReview: 2,
    InformationRequested: 1,
    Approved: 1,
    Rejected: 0,
    Settled: 4,
  },
  totalClaimedAmount: 25_000,
  totalApprovedAmount: 9_000,
};

function page(items: ClaimSummary[], totalCount = items.length): PagedResponse<ClaimSummary> {
  return { items, page: 1, pageSize: 10, totalCount };
}

describe('ClaimsDashboard', () => {
  let http: HttpTestingController;
  let realtime: ReturnType<typeof fakeRealtime>['fake'];

  const listRequest = (): TestRequest => http.expectOne((r) => r.url === '/api/claims');
  const statsRequest = (): TestRequest => http.expectOne('/api/claims/stats');

  async function render() {
    const fixture = TestBed.createComponent(ClaimsDashboard);
    fixture.detectChanges();
    listRequest().flush(page([claimDetails()], 25));
    statsRequest().flush(stats);
    await fixture.whenStable();
    return { fixture, element: fixture.nativeElement as HTMLElement };
  }

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

  it('shows the per-status counters and the claims list', async () => {
    const { element } = await render();

    const tiles = Array.from(element.querySelectorAll('.tile')).map(
      (t) =>
        `${t.querySelector('.tile-count')?.textContent?.trim()} ${t.querySelector('.tile-label')?.textContent?.trim()}`,
    );
    expect(tiles).toContain('3 Déclaré');
    expect(tiles).toContain('4 Indemnisé');
    expect(element.querySelector('tbody tr')?.textContent).toContain('SIN-2026-0000ABCD');
    expect(element.querySelector('.pager span')?.textContent).toContain('1 / 3');
  });

  it('filters by status when a counter is clicked, and clears it on second click', async () => {
    const { element } = await render();
    const tile = () =>
      Array.from(element.querySelectorAll<HTMLButtonElement>('.tile')).find((t) =>
        t.textContent?.includes('En instruction'),
      )!;

    tile().click();
    await settle();
    const filtered = listRequest();
    expect(filtered.request.params.get('status')).toBe('UnderReview');
    filtered.flush(page([]));
    await settle();
    expect(tile().getAttribute('aria-pressed')).toBe('true');

    tile().click();
    await settle();
    expect(listRequest().request.params.has('status')).toBe(false);
  });

  it('searches and goes back to the first page', async () => {
    const { element } = await render();

    element.querySelector<HTMLButtonElement>('.pager button:last-child')!.click();
    await settle();
    expect(listRequest().request.params.get('page')).toBe('2');

    const search = element.querySelector<HTMLInputElement>('input[type=search]')!;
    search.value = 'pol-1042';
    search.dispatchEvent(new Event('input'));
    await settle();

    const request = listRequest();
    expect(request.request.params.get('search')).toBe('pol-1042');
    expect(request.request.params.get('page')).toBe('1');
  });

  it('refetches list and counters when any claim changes elsewhere', async () => {
    await render();

    realtime.lastChange.set({ claimId: 'x', number: 'SIN-X', status: 'Settled' });
    await settle();

    listRequest().flush(page([]));
    statsRequest().flush(stats);
  });
});
