import { CurrencyPipe, DatePipe } from '@angular/common';
import { httpResource } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, computed, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { isConflict, problemMessages } from '../../shared/problem-details';
import { ACTION_META, STATUS_LABELS, TYPE_LABELS } from '../claim-labels';
import { ClaimAction, ClaimDetails } from '../claim.models';
import { CLAIMS_URL, ClaimsApi } from '../claims-api';
import { ClaimsRealtime } from '../claims-realtime';
import { StatusBadge } from '../status-badge';

@Component({
  selector: 'app-claim-detail',
  imports: [RouterLink, CurrencyPipe, DatePipe, StatusBadge],
  templateUrl: './claim-detail.html',
  styleUrl: './claim-detail.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ClaimDetail {
  private readonly api = inject(ClaimsApi);
  private readonly realtime = inject(ClaimsRealtime);

  /** Bound from the route by withComponentInputBinding(). */
  readonly id = input.required<string>();

  protected readonly actionMeta = ACTION_META;
  protected readonly statusLabels = STATUS_LABELS;
  protected readonly typeLabels = TYPE_LABELS;

  protected readonly pendingAction = signal<ClaimAction | null>(null);
  protected readonly reason = signal('');
  protected readonly approvedAmount = signal<number | null>(null);
  protected readonly busy = signal(false);
  protected readonly errors = signal<string[]>([]);

  // Only a change to *this* claim refetches it: other claims' notifications keep returning the same undefined.
  private readonly ownChange = computed(() => {
    const change = this.realtime.lastChange();
    return change?.claimId === this.id() ? change : undefined;
  });

  protected readonly claim = httpResource<ClaimDetails>(() => {
    this.ownChange();
    return `${CLAIMS_URL}/${this.id()}`;
  });

  protected readonly pendingMeta = computed(() => {
    const action = this.pendingAction();
    return action ? ACTION_META[action] : null;
  });

  protected choose(action: ClaimAction): void {
    const meta = ACTION_META[action];
    this.errors.set([]);
    if (!meta.needsReason && !meta.needsAmount) {
      void this.confirm(action);
      return;
    }
    this.pendingAction.set(action);
    this.reason.set('');
    this.approvedAmount.set(this.claim.value()?.claimedAmount ?? null);
  }

  protected cancel(): void {
    this.pendingAction.set(null);
    this.errors.set([]);
  }

  protected async confirm(action: ClaimAction): Promise<void> {
    const current = this.claim.value();
    if (!current) return;

    this.busy.set(true);
    try {
      const updated = await firstValueFrom(
        this.api.apply(current.id, {
          action,
          expectedVersion: current.version,
          reason: this.reason() || undefined,
          approvedAmount: this.approvedAmount() ?? undefined,
        }),
      );
      this.claim.value.set(updated);
      this.pendingAction.set(null);
    } catch (error) {
      this.errors.set(problemMessages(error));
      if (isConflict(error)) {
        // Someone else moved the claim: show their version so the user decides on fresh data.
        this.claim.reload();
      }
    } finally {
      this.busy.set(false);
    }
  }
}
