import { CurrencyPipe, DatePipe } from '@angular/common';
import { httpResource } from '@angular/common/http';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  input,
  linkedSignal,
  signal,
} from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ClaimsRealtime } from '../../core/realtime/claims-realtime';
import { reloadWhen } from '../../core/realtime/reload-when';
import { STATUS_LABELS, TYPE_LABELS } from '../../claims/claim-labels';
import {
  CLAIM_STATUSES,
  ClaimCapabilities,
  ClaimStats,
  ClaimStatus,
  ClaimSummary,
  PagedResponse,
} from '../../claims/claim.models';
import { CLAIMS_URL } from '../../claims/claims-api';
import { Icon } from '../../shared/icon';
import { StatusBadge } from '../../shared/status-badge';

const PAGE_SIZE = 10;

@Component({
  selector: 'app-claims-list',
  imports: [RouterLink, CurrencyPipe, DatePipe, Icon, StatusBadge],
  templateUrl: './claims-list.html',
  styleUrl: './claims-list.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ClaimsList {
  private readonly realtime = inject(ClaimsRealtime);
  private readonly router = inject(Router);

  /** ?status=… from the URL (e.g. a click in the dashboard legend), bound by withComponentInputBinding. */
  readonly status = input<ClaimStatus | undefined>();

  protected readonly statuses = CLAIM_STATUSES;
  protected readonly statusLabels = STATUS_LABELS;
  protected readonly typeLabels = TYPE_LABELS;

  // Starts from the URL, then follows the user's clicks: exactly what linkedSignal is for.
  protected readonly statusFilter = linkedSignal<ClaimStatus | null>(() => this.status() ?? null);
  protected readonly search = signal('');
  // Back to page 1 whenever the filter or the search changes, while still letting the pager move freely.
  protected readonly page = linkedSignal(() => {
    this.statusFilter();
    this.search();
    return 1;
  });

  protected readonly claims = httpResource<PagedResponse<ClaimSummary>>(() => {
    const params: Record<string, string | number> = { page: this.page(), pageSize: PAGE_SIZE };
    const status = this.statusFilter();
    const search = this.search().trim();
    if (status) params['status'] = status;
    if (search) params['search'] = search;
    return { url: CLAIMS_URL, params };
  });

  protected readonly stats = httpResource<ClaimStats>(() => `${CLAIMS_URL}/stats`);

  protected readonly capabilities = httpResource<ClaimCapabilities>(
    () => `${CLAIMS_URL}/capabilities`,
  );

  constructor() {
    reloadWhen(this.realtime.lastChange, this.claims, this.stats);
  }

  protected readonly pageCount = computed(() =>
    Math.max(1, Math.ceil((this.claims.value()?.totalCount ?? 0) / PAGE_SIZE)),
  );

  protected toggleStatus(status: ClaimStatus): void {
    const next = this.statusFilter() === status ? null : status;
    this.statusFilter.set(next);
    void this.router.navigate([], { queryParams: { status: next }, replaceUrl: true });
  }

  protected goTo(page: number): void {
    this.page.set(Math.min(Math.max(page, 1), this.pageCount()));
  }
}
