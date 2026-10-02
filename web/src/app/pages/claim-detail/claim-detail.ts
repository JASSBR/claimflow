import { CurrencyPipe, DatePipe } from '@angular/common';
import { httpResource } from '@angular/common/http';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  input,
  signal,
  viewChild,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { Auth } from '../../core/auth/auth';
import { ClaimsRealtime } from '../../core/realtime/claims-realtime';
import { ToastService } from '../../core/toast';
import { ACTION_META, TYPE_LABELS } from '../../claims/claim-labels';
import { ClaimCapabilities, ClaimDetails } from '../../claims/claim.models';
import { CLAIMS_URL, ClaimsApi } from '../../claims/claims-api';
import { ClaimAnalysis, ClaimDocument, DocumentSettings } from '../../documents/documents.models';
import { Icon } from '../../shared/icon';
import { isConflict, problemMessages } from '../../shared/problem-details';
import { StatusBadge } from '../../shared/status-badge';
import { AnalysisPanel } from './analysis-panel';
import { ClaimTimeline } from './claim-timeline';
import { DecisionPanel, DecisionRequest } from './decision-panel';
import { DocumentsPanel } from './documents-panel';
import { WorkflowStepper } from './workflow-stepper';

@Component({
  selector: 'app-claim-detail',
  imports: [
    RouterLink,
    CurrencyPipe,
    DatePipe,
    Icon,
    StatusBadge,
    AnalysisPanel,
    ClaimTimeline,
    DecisionPanel,
    DocumentsPanel,
    WorkflowStepper,
  ],
  templateUrl: './claim-detail.html',
  styleUrl: './claim-detail.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ClaimDetail {
  private readonly api = inject(ClaimsApi);
  private readonly realtime = inject(ClaimsRealtime);
  private readonly toasts = inject(ToastService);
  protected readonly auth = inject(Auth);

  /** Bound from the route by withComponentInputBinding(). */
  readonly id = input.required<string>();

  protected readonly typeLabels = TYPE_LABELS;
  protected readonly busy = signal(false);
  protected readonly errors = signal<string[]>([]);
  private readonly decisionPanel = viewChild(DecisionPanel);

  // Only a change to *this* claim refetches it: other claims' notifications keep returning the same undefined.
  private readonly ownChange = computed(() => {
    const change = this.realtime.lastChange();
    return change?.claimId === this.id() ? change : undefined;
  });

  protected readonly claim = httpResource<ClaimDetails>(() => {
    this.ownChange();
    return `${CLAIMS_URL}/${this.id()}`;
  });

  protected readonly documents = httpResource<ClaimDocument[]>(
    () => `${CLAIMS_URL}/${this.id()}/documents`,
    { defaultValue: [] },
  );
  protected readonly analysis = httpResource<ClaimAnalysis | null>(
    () => `${CLAIMS_URL}/${this.id()}/analysis`,
  );
  protected readonly settings = httpResource<DocumentSettings>(() => '/api/documents/settings');
  protected readonly capabilities = httpResource<ClaimCapabilities>(
    () => `${CLAIMS_URL}/capabilities`,
  );

  protected readonly canWrite = computed(() => this.capabilities.value()?.canDeclare ?? false);

  protected async decide(decision: DecisionRequest): Promise<void> {
    const current = this.claim.value();
    if (!current) return;

    this.busy.set(true);
    this.errors.set([]);
    try {
      const updated = await firstValueFrom(
        this.api.apply(current.id, { ...decision, expectedVersion: current.version }),
      );
      this.claim.value.set(updated);
      this.decisionPanel()?.reset();
      this.toasts.show({
        tone: 'success',
        title: `${ACTION_META[decision.action].label} : c'est fait`,
        message: updated.number,
      });
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
