import { CurrencyPipe, DatePipe } from '@angular/common';
import { httpResource } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { STATUS_LABELS, TYPE_LABELS } from '../claim-labels';
import {
  CLAIM_STATUSES,
  ClaimStats,
  ClaimStatus,
  ClaimSummary,
  PagedResponse,
} from '../claim.models';
import { CLAIMS_URL } from '../claims-api';
import { ClaimsRealtime } from '../claims-realtime';
import { StatusBadge } from '../status-badge';

const PAGE_SIZE = 10;

@Component({
  selector: 'app-claims-dashboard',
  imports: [RouterLink, CurrencyPipe, DatePipe, StatusBadge],
  templateUrl: './claims-dashboard.html',
  styleUrl: './claims-dashboard.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ClaimsDashboard {
  private readonly realtime = inject(ClaimsRealtime);

  protected readonly statuses = CLAIM_STATUSES;
  protected readonly statusLabels = STATUS_LABELS;
  protected readonly typeLabels = TYPE_LABELS;

  protected readonly status = signal<ClaimStatus | null>(null);
  protected readonly search = signal('');
  protected readonly page = signal(1);

  // Reading lastChange() makes every committed change (from any user) refetch the list and the counters.
  protected readonly claims = httpResource<PagedResponse<ClaimSummary>>(() => {
    this.realtime.lastChange();
    const params: Record<string, string | number> = { page: this.page(), pageSize: PAGE_SIZE };
    const status = this.status();
    const search = this.search().trim();
    if (status) params['status'] = status;
    if (search) params['search'] = search;
    return { url: CLAIMS_URL, params };
  });

  protected readonly stats = httpResource<ClaimStats>(() => {
    this.realtime.lastChange();
    return `${CLAIMS_URL}/stats`;
  });

  protected readonly pageCount = computed(() => {
    const total = this.claims.value()?.totalCount ?? 0;
    return Math.max(1, Math.ceil(total / PAGE_SIZE));
  });

  protected toggleStatus(status: ClaimStatus): void {
    this.status.update((current) => (current === status ? null : status));
    this.page.set(1);
  }

  protected onSearch(value: string): void {
    this.search.set(value);
    this.page.set(1);
  }

  protected goTo(page: number): void {
    this.page.set(Math.min(Math.max(page, 1), this.pageCount()));
  }
}
