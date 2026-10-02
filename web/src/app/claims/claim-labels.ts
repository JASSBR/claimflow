import { ClaimAction, ClaimStatus, ClaimType } from './claim.models';

export const STATUS_LABELS: Readonly<Record<ClaimStatus, string>> = {
  Declared: 'Déclaré',
  UnderReview: 'En instruction',
  InformationRequested: 'Pièces demandées',
  Approved: 'Accepté',
  Rejected: 'Refusé',
  Settled: 'Indemnisé',
};

export const TYPE_LABELS: Readonly<Record<ClaimType, string>> = {
  Auto: 'Automobile',
  Home: 'Habitation',
  Liability: 'Responsabilité civile',
};

interface ActionMeta {
  readonly label: string;
  readonly needsReason: boolean;
  readonly needsAmount: boolean;
  readonly tone: 'primary' | 'neutral' | 'danger';
}

export const ACTION_META: Readonly<Record<ClaimAction, ActionMeta>> = {
  StartReview: {
    label: 'Prendre en charge',
    needsReason: false,
    needsAmount: false,
    tone: 'primary',
  },
  RequestInformation: {
    label: 'Demander des pièces',
    needsReason: true,
    needsAmount: false,
    tone: 'neutral',
  },
  ResumeReview: {
    label: "Reprendre l'instruction",
    needsReason: false,
    needsAmount: false,
    tone: 'primary',
  },
  Approve: { label: 'Accepter', needsReason: false, needsAmount: true, tone: 'primary' },
  Reject: { label: 'Refuser', needsReason: true, needsAmount: false, tone: 'danger' },
  Settle: { label: "Verser l'indemnité", needsReason: false, needsAmount: false, tone: 'primary' },
};
