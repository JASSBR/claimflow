import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { STATUS_LABELS, WORKFLOW_STEPS } from '../../claims/claim-labels';
import { ClaimStatus } from '../../claims/claim.models';
import { Icon } from '../../shared/icon';

type StepState = 'done' | 'current' | 'todo' | 'rejected';

@Component({
  selector: 'app-workflow-stepper',
  imports: [Icon],
  template: `
    <ol class="stepper" aria-label="Avancement du dossier">
      @for (step of steps(); track step.status) {
        <li
          [attr.data-state]="step.state"
          [attr.aria-current]="step.state === 'current' ? 'step' : null"
        >
          <span class="marker">
            @switch (step.state) {
              @case ('done') {
                <app-icon name="check" [size]="14" />
              }
              @case ('rejected') {
                <app-icon name="x" [size]="14" />
              }
              @default {
                {{ $index + 1 }}
              }
            }
          </span>
          <span class="label">{{ step.label }}</span>
        </li>
      }
    </ol>
  `,
  styles: `
    .stepper {
      list-style: none;
      display: flex;
      margin: 0;
      padding: 0;
      gap: 0.25rem;
    }
    li {
      flex: 1;
      display: flex;
      align-items: center;
      gap: 0.6rem;
      position: relative;
      font-size: 0.85rem;
      color: var(--muted);
    }
    li:not(:last-child)::after {
      content: '';
      flex: 1;
      height: 2px;
      border-radius: 2px;
      background: var(--border);
      margin: 0 0.4rem;
    }
    li[data-state='done']:not(:last-child)::after {
      background: var(--success);
    }
    .marker {
      display: grid;
      place-items: center;
      width: 1.75rem;
      height: 1.75rem;
      border-radius: 50%;
      flex-shrink: 0;
      font-size: 0.78rem;
      font-weight: 700;
      border: 2px solid var(--border);
      background: var(--surface);
    }
    [data-state='done'] .marker {
      background: var(--success);
      border-color: var(--success);
      color: #fff;
    }
    [data-state='current'] {
      color: var(--text);
      font-weight: 600;
    }
    [data-state='current'] .marker {
      border-color: var(--accent);
      color: var(--accent);
      box-shadow: 0 0 0 4px var(--accent-soft);
    }
    [data-state='rejected'] {
      color: var(--danger);
      font-weight: 600;
    }
    [data-state='rejected'] .marker {
      background: var(--danger);
      border-color: var(--danger);
      color: #fff;
    }
    .label {
      white-space: nowrap;
    }
    @media (max-width: 640px) {
      .label {
        display: none;
      }
      [data-state='current'] .label {
        display: inline;
      }
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class WorkflowStepper {
  readonly status = input.required<ClaimStatus>();

  /** Information requested is still "under review" on the happy path; a rejection replaces the remaining steps. */
  protected readonly steps = computed(() => {
    const status = this.status();
    const position = status === 'InformationRequested' ? 1 : WORKFLOW_STEPS.indexOf(status);
    if (status === 'Rejected') {
      return [
        {
          status: 'Declared' as ClaimStatus,
          label: STATUS_LABELS.Declared,
          state: 'done' as StepState,
        },
        {
          status: 'UnderReview' as ClaimStatus,
          label: STATUS_LABELS.UnderReview,
          state: 'done' as StepState,
        },
        {
          status: 'Rejected' as ClaimStatus,
          label: STATUS_LABELS.Rejected,
          state: 'rejected' as StepState,
        },
      ];
    }
    return WORKFLOW_STEPS.map((step, index) => ({
      status: step,
      label:
        index === 1 && status === 'InformationRequested'
          ? STATUS_LABELS.InformationRequested
          : STATUS_LABELS[step],
      state: (index < position || status === 'Settled'
        ? 'done'
        : index === position
          ? 'current'
          : 'todo') as StepState,
    }));
  });
}
