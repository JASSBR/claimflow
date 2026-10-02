import { ClaimAction, ClaimStatus, ClaimType } from './claim.models';

export const STATUS_LABELS: Readonly<Record<ClaimStatus, string>> = {
  Declared: 'Déclaré',
  UnderReview: 'En instruction',
  InformationRequested: 'Pièces demandées',
  Approved: 'Accepté',
  Rejected: 'Refusé',
  Settled: 'Indemnisé',
};

/** Past-tense verbs for the activity feed: "Karim Benali a accepté SIN-…". */
export const STATUS_VERBS: Readonly<Record<ClaimStatus, string>> = {
  Declared: 'a déclaré',
  UnderReview: 'instruit',
  InformationRequested: 'a demandé des pièces pour',
  Approved: 'a accepté',
  Rejected: 'a refusé',
  Settled: 'a indemnisé',
};

export const TYPE_LABELS: Readonly<Record<ClaimType, string>> = {
  Auto: 'Automobile',
  Home: 'Habitation',
  Liability: 'Responsabilité civile',
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
    label: 'Prendre en charge',
    hint: "Ouvre l'instruction du dossier.",
    needsReason: false,
    needsAmount: false,
    tone: 'primary',
  },
  RequestInformation: {
    label: 'Demander des pièces',
    hint: "Suspend l'instruction en attendant l'assuré.",
    needsReason: true,
    needsAmount: false,
    tone: 'neutral',
  },
  ResumeReview: {
    label: "Reprendre l'instruction",
    hint: 'Les pièces ont été reçues.',
    needsReason: false,
    needsAmount: false,
    tone: 'primary',
  },
  Approve: {
    label: 'Accepter',
    hint: 'Fixe le montant indemnisé.',
    needsReason: false,
    needsAmount: true,
    tone: 'success',
  },
  Reject: {
    label: 'Refuser',
    hint: "Clôt le dossier ; le motif est communiqué à l'assuré.",
    needsReason: true,
    needsAmount: false,
    tone: 'danger',
  },
  Settle: {
    label: "Verser l'indemnité",
    hint: 'Seconde signature : libère le paiement.',
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
