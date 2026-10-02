import { ClaimAction, ClaimStatus, ClaimType } from './claim.models';

export const STATUS_LABELS: Readonly<Record<ClaimStatus, string>> = {
  Declared: $localize`:@@status.declared:Déclaré`,
  UnderReview: $localize`:@@status.underReview:En instruction`,
  InformationRequested: $localize`:@@status.informationRequested:Pièces demandées`,
  Approved: $localize`:@@status.approved:Accepté`,
  Rejected: $localize`:@@status.rejected:Refusé`,
  Settled: $localize`:@@status.settled:Indemnisé`,
};

/** Past-tense verbs for the activity feed: "Karim Benali a accepté SIN-…". */
export const STATUS_VERBS: Readonly<Record<ClaimStatus, string>> = {
  Declared: $localize`:@@verb.declared:a déclaré`,
  UnderReview: $localize`:@@verb.underReview:instruit`,
  InformationRequested: $localize`:@@verb.informationRequested:a demandé des pièces pour`,
  Approved: $localize`:@@verb.approved:a accepté`,
  Rejected: $localize`:@@verb.rejected:a refusé`,
  Settled: $localize`:@@verb.settled:a indemnisé`,
};

export const TYPE_LABELS: Readonly<Record<ClaimType, string>> = {
  Auto: $localize`:@@type.auto:Automobile`,
  Home: $localize`:@@type.home:Habitation`,
  Liability: $localize`:@@type.liability:Responsabilité civile`,
};

export interface ActionMeta {
  readonly label: string;
  readonly hint: string;
  readonly needsReason: boolean;
  readonly needsAmount: boolean;
  readonly tone: 'primary' | 'neutral' | 'danger' | 'success';
}

export const ACTION_META: Readonly<Record<ClaimAction, ActionMeta>> = {
  StartReview: {
    label: $localize`:@@action.startReview:Prendre en charge`,
    hint: $localize`:@@action.startReview.hint:Ouvre l'instruction du dossier.`,
    needsReason: false,
    needsAmount: false,
    tone: 'primary',
  },
  RequestInformation: {
    label: $localize`:@@action.requestInformation:Demander des pièces`,
    hint: $localize`:@@action.requestInformation.hint:Suspend l'instruction en attendant l'assuré.`,
    needsReason: true,
    needsAmount: false,
    tone: 'neutral',
  },
  ResumeReview: {
    label: $localize`:@@action.resumeReview:Reprendre l'instruction`,
    hint: $localize`:@@action.resumeReview.hint:Les pièces ont été reçues.`,
    needsReason: false,
    needsAmount: false,
    tone: 'primary',
  },
  Approve: {
    label: $localize`:@@action.approve:Accepter`,
    hint: $localize`:@@action.approve.hint:Fixe le montant indemnisé.`,
    needsReason: false,
    needsAmount: true,
    tone: 'success',
  },
  Reject: {
    label: $localize`:@@action.reject:Refuser`,
    hint: $localize`:@@action.reject.hint:Clôt le dossier ; le motif est communiqué à l'assuré.`,
    needsReason: true,
    needsAmount: false,
    tone: 'danger',
  },
  Settle: {
    label: $localize`:@@action.settle:Verser l'indemnité`,
    hint: $localize`:@@action.settle.hint:Seconde signature : libère le paiement.`,
    needsReason: false,
    needsAmount: false,
    tone: 'success',
  },
};

/** The happy path, used by the progress stepper. Rejected is shown as a terminal branch. */
export const WORKFLOW_STEPS: readonly ClaimStatus[] = [
  'Declared',
  'UnderReview',
  'Approved',
  'Settled',
];
