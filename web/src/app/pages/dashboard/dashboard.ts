import { CurrencyPipe, DatePipe, PercentPipe } from '@angular/common';
import { httpResource } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, LOCALE_ID, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Auth } from '../../core/auth/auth';
import { ClaimsRealtime } from '../../core/realtime/claims-realtime';
import { reloadWhen } from '../../core/realtime/reload-when';
import { STATUS_LABELS, STATUS_VERBS, TYPE_LABELS } from '../../claims/claim-labels';
import {
  CLAIM_STATUSES,
  CLAIM_TYPES,
  ClaimStats,
  ClaimSummary,
  PagedResponse,
} from '../../claims/claim.models';
import { CLAIMS_URL } from '../../claims/claims-api';
import { Avatar } from '../../shared/avatar';
import { BarItem, BarList } from '../../shared/charts/bar-list';
import { DonutChart, DonutSlice } from '../../shared/charts/donut-chart';
import { Icon } from '../../shared/icon';
import { RelativeTimePipe } from '../../shared/relative-time.pipe';
import { StatusBadge } from '../../shared/status-badge';

const STATUS_COLORS: Record<string, string> = {
  Declared: 'var(--status-declared)',
  UnderReview: 'var(--status-review)',
  InformationRequested: 'var(--status-info)',
  Approved: 'var(--status-approved)',
  Rejected: 'var(--status-rejected)',
  Settled: 'var(--status-settled)',
};

@Component({
  selector: 'app-dashboard',
  imports: [
    RouterLink,
    CurrencyPipe,
    DatePipe,
    PercentPipe,
    Avatar,
    BarList,
    DonutChart,
    Icon,
    RelativeTimePipe,
    StatusBadge,
  ],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Dashboard {
  protected readonly auth = inject(Auth);
  private readonly euros = new Intl.NumberFormat(inject(LOCALE_ID), {
    style: 'currency',
    currency: 'EUR',
    maximumFractionDigits: 0,
  });
  protected readonly realtime = inject(ClaimsRealtime);
  protected readonly statusLabels = STATUS_LABELS;
  protected readonly statusVerbs = STATUS_VERBS;
  protected readonly typeLabels = TYPE_LABELS;

  protected readonly stats = httpResource<ClaimStats>(() => `${CLAIMS_URL}/stats`);
  protected readonly recent = httpResource<PagedResponse<ClaimSummary>>(() => ({
    url: CLAIMS_URL,
    params: { pageSize: 6 },
  }));

  constructor() {
    // Any committed change, by anyone, refreshes the figures.
    reloadWhen(this.realtime.lastChange, this.stats, this.recent);
  }

  protected readonly greeting = computed(() => {
    const hour = new Date().getHours();
    const name = this.auth.user()?.name.split(' ')[0] ?? '';
    return hour < 18
      ? $localize`:@@dashboard.goodMorning:Bonjour, ${name}:name:`
      : $localize`:@@dashboard.goodEvening:Bonsoir, ${name}:name:`;
  });

  protected readonly kpis = computed(() => {
    const stats = this.stats.value();
    if (!stats) return null;
    const count = stats.countByStatus;
    const open = count.Declared + count.UnderReview + count.InformationRequested;
    const decided = count.Approved + count.Settled + count.Rejected;
    return {
      open,
      total: CLAIM_STATUSES.reduce((sum, status) => sum + count[status], 0),
      claimed: stats.totalClaimedAmount,
      approved: stats.totalApprovedAmount,
      acceptance: decided === 0 ? null : (count.Approved + count.Settled) / decided,
      decided,
    };
  });

  protected readonly slices = computed<DonutSlice[]>(() => {
    const count = this.stats.value()?.countByStatus;
    return count
      ? CLAIM_STATUSES.map((status) => ({
          key: status,
          label: STATUS_LABELS[status],
          value: count[status],
          color: STATUS_COLORS[status]!,
        }))
      : [];
  });

  protected readonly byType = computed<BarItem[]>(() => {
    const byType = this.stats.value()?.byType;
    return byType
      ? CLAIM_TYPES.map((type) => ({
          key: type,
          label: `${TYPE_LABELS[type]} · ${byType[type].count}`,
          value: byType[type].claimedAmount,
          display: this.euros.format(byType[type].claimedAmount),
        }))
      : [];
  });
}
