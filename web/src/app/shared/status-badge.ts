import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { STATUS_LABELS } from '../claims/claim-labels';
import { ClaimStatus } from '../claims/claim.models';

@Component({
  selector: 'app-status-badge',
  template: `<span class="badge" [attr.data-status]="status()"
    ><span class="dot"></span>{{ label() }}</span
  >`,
  styles: `
    .badge {
      display: inline-flex;
      align-items: center;
      gap: 0.4rem;
      padding: 0.2rem 0.65rem 0.2rem 0.5rem;
      border-radius: 999px;
      font-size: 0.78rem;
      font-weight: 600;
      white-space: nowrap;
      color: var(--status);
      background: color-mix(in srgb, var(--status) 12%, transparent);
    }
    .dot {
      width: 0.45rem;
      height: 0.45rem;
      border-radius: 50%;
      background: currentColor;
    }
    [data-status='Declared'] {
      --status: var(--status-declared);
    }
    [data-status='UnderReview'] {
      --status: var(--status-review);
    }
    [data-status='InformationRequested'] {
      --status: var(--status-info);
    }
    [data-status='Approved'] {
      --status: var(--status-approved);
    }
    [data-status='Rejected'] {
      --status: var(--status-rejected);
    }
    [data-status='Settled'] {
      --status: var(--status-settled);
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StatusBadge {
  readonly status = input.required<ClaimStatus>();
  protected readonly label = computed(() => STATUS_LABELS[this.status()]);
}
