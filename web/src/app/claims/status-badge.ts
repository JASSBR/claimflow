import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { STATUS_LABELS } from './claim-labels';
import { ClaimStatus } from './claim.models';

@Component({
  selector: 'app-status-badge',
  template: `<span class="badge" [attr.data-status]="status()">{{ label() }}</span>`,
  styles: `
    .badge {
      display: inline-block;
      padding: 0.15rem 0.6rem;
      border-radius: 999px;
      font-size: 0.78rem;
      font-weight: 600;
      white-space: nowrap;
      background: var(--status-bg, var(--surface-2));
      color: var(--status-fg, var(--text));
    }
    [data-status='Declared'] {
      --status-bg: #e8eefc;
      --status-fg: #2847a3;
    }
    [data-status='UnderReview'] {
      --status-bg: #fff4dc;
      --status-fg: #8a5a00;
    }
    [data-status='InformationRequested'] {
      --status-bg: #fde9df;
      --status-fg: #a03d12;
    }
    [data-status='Approved'] {
      --status-bg: #e3f6ec;
      --status-fg: #17693f;
    }
    [data-status='Rejected'] {
      --status-bg: #fbe4e6;
      --status-fg: #a1202e;
    }
    [data-status='Settled'] {
      --status-bg: #e6f1f7;
      --status-fg: #1d5672;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StatusBadge {
  readonly status = input.required<ClaimStatus>();
  protected readonly label = computed(() => STATUS_LABELS[this.status()]);
}
