import { CurrencyPipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  input,
  linkedSignal,
  output,
  signal,
} from '@angular/core';
import { ACTION_META } from '../../claims/claim-labels';
import { ClaimAction, ClaimDetails } from '../../claims/claim.models';
import { Icon } from '../../shared/icon';

export interface DecisionRequest {
  readonly action: ClaimAction;
  readonly reason?: string;
  readonly approvedAmount?: number;
}

/** Presentational: renders what the server allows, explains what it doesn't, emits the user's decision. */
@Component({
  selector: 'app-decision-panel',
  imports: [CurrencyPipe, Icon],
  templateUrl: './decision-panel.html',
  styleUrl: './decision-panel.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DecisionPanel {
  readonly claim = input.required<ClaimDetails>();
  readonly userName = input.required<string>();
  readonly approvalLimit = input<number | null>(null);
  /** Whether the user's role may release payments at all; the four-eyes notice only makes sense if it does. */
  readonly canSettle = input(false);
  readonly readOnly = input(false);
  readonly busy = input(false);
  readonly errors = input<readonly string[]>([]);
  readonly decide = output<DecisionRequest>();

  protected readonly meta = ACTION_META;
  protected readonly pending = signal<ClaimAction | null>(null);
  protected readonly reason = signal('');
  // Pre-filled with the claimed amount each time a new claim version arrives.
  protected readonly amount = linkedSignal<number | null>(() => this.claim().claimedAmount);

  protected readonly pendingMeta = computed(() => {
    const action = this.pending();
    return action ? ACTION_META[action] : null;
  });

  protected readonly exceedsLimit = computed(() => {
    const limit = this.approvalLimit();
    const amount = this.amount();
    return this.pending() === 'Approve' && limit !== null && amount !== null && amount > limit;
  });

  /** Why the "release payment" button is missing, so the rule is visible instead of feeling like a bug. */
  protected readonly fourEyesNotice = computed(() => {
    const claim = this.claim();
    if (!this.canSettle() || claim.status !== 'Approved' || claim.allowedActions.includes('Settle'))
      return false;
    const approval = [...claim.history].reverse().find((entry) => entry.action === 'Approve');
    return approval?.actorName === this.userName();
  });

  protected choose(action: ClaimAction): void {
    const meta = ACTION_META[action];
    if (!meta.needsReason && !meta.needsAmount) {
      this.decide.emit({ action });
      return;
    }
    this.reason.set('');
    this.pending.set(action);
  }

  protected confirm(): void {
    const action = this.pending();
    if (!action) return;
    this.decide.emit({
      action,
      reason: this.reason().trim() || undefined,
      approvedAmount: ACTION_META[action].needsAmount ? (this.amount() ?? undefined) : undefined,
    });
  }

  /** Called by the container once the server accepted the decision. */
  reset(): void {
    this.pending.set(null);
  }
}
