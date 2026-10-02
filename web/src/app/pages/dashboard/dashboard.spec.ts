import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { ClaimStats } from '../../claims/claim.models';
import { claimDetails, fakeAuth, fakeRealtime, settle } from '../../testing';
import { Dashboard } from './dashboard';

const STATS: ClaimStats = {
  countByStatus: {
    Declared: 2,
    UnderReview: 1,
    InformationRequested: 1,
    Approved: 2,
    Rejected: 1,
    Settled: 1,
  },
  byType: {
    Auto: { count: 3, claimedAmount: 6_100 },
    Home: { count: 3, claimedAmount: 26_400 },
    Liability: { count: 2, claimedAmount: 7_880 },
  },
  totalClaimedAmount: 40_380,
  totalApprovedAmount: 9_207,
};

describe('Dashboard', () => {
  let http: HttpTestingController;
  let realtime: ReturnType<typeof fakeRealtime>['fake'];

  beforeEach(() => {
    const stub = fakeRealtime();
    realtime = stub.fake;
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        stub.provider,
        fakeAuth().provider,
      ],
    });
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  async function render() {
    const fixture = TestBed.createComponent(Dashboard);
    fixture.detectChanges();
    http.expectOne('/api/claims/stats').flush(STATS);
    http
      .expectOne((r) => r.url === '/api/claims')
      .flush({ items: [claimDetails()], page: 1, pageSize: 6, totalCount: 1 });
    await fixture.whenStable();
    return { fixture, element: fixture.nativeElement as HTMLElement };
  }

  it('derives the KPIs from the portfolio statistics', async () => {
    const { element } = await render();
    const kpis = Array.from(element.querySelectorAll('.kpi strong')).map((k) =>
      k.textContent?.replace(/\s/g, ''),
    );
    // 4 open of 8; 3 accepted out of 4 decisions.
    expect(kpis[0]).toBe('4');
    expect(kpis[3]).toBe('75%');
    expect(element.querySelectorAll('.legend li')).toHaveLength(6);
  });

  it('shows colleagues’ decisions in the live feed', async () => {
    const { fixture, element } = await render();
    expect(element.querySelector('.empty')).not.toBeNull();

    realtime.activity.set([
      {
        claimId: 'c1',
        number: 'SIN-2026-000001',
        status: 'Approved',
        actorName: 'Karim Benali',
        occurredAt: new Date().toISOString(),
      },
    ]);
    realtime.lastChange.set(realtime.activity()[0]!);
    await settle();
    http.expectOne('/api/claims/stats').flush(STATS);
    http
      .expectOne((r) => r.url === '/api/claims')
      .flush({ items: [], page: 1, pageSize: 6, totalCount: 0 });
    await fixture.whenStable();

    expect(element.querySelector('.feed li')?.textContent).toContain(
      'Karim Benali a accepté SIN-2026-000001',
    );
  });
});
