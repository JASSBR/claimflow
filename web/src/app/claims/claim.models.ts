// Mirrors ClaimFlow.Claims.Contracts. Enums travel as names (JsonStringEnumConverter, integers rejected).

export const CLAIM_STATUSES = [
  'Declared',
  'UnderReview',
  'InformationRequested',
  'Approved',
  'Rejected',
  'Settled',
] as const;
export type ClaimStatus = (typeof CLAIM_STATUSES)[number];

export const CLAIM_TYPES = ['Auto', 'Home', 'Liability'] as const;
export type ClaimType = (typeof CLAIM_TYPES)[number];

export type ClaimAction =
  'StartReview' | 'RequestInformation' | 'ResumeReview' | 'Approve' | 'Reject' | 'Settle';

export interface ClaimSummary {
  readonly id: string;
  readonly number: string;
  readonly policyNumber: string;
  readonly type: ClaimType;
  readonly status: ClaimStatus;
  readonly claimedAmount: number;
  readonly approvedAmount: number | null;
  readonly incidentDate: string;
  readonly declaredAt: string;
  readonly lastUpdatedAt: string;
}

export interface ClaimHistoryEntry {
  readonly from: ClaimStatus;
  readonly to: ClaimStatus;
  readonly action: ClaimAction;
  readonly actorName: string;
  readonly reason: string | null;
  readonly occurredAt: string;
}

export interface ClaimDetails extends ClaimSummary {
  readonly description: string;
  readonly declaredBy: string;
  readonly version: number;
  /** Already filtered for the current user: workflow ∩ role ∩ four-eyes. */
  readonly allowedActions: readonly ClaimAction[];
  readonly history: readonly ClaimHistoryEntry[];
}

export interface PagedResponse<T> {
  readonly items: readonly T[];
  readonly page: number;
  readonly pageSize: number;
  readonly totalCount: number;
}

export interface ClaimStats {
  readonly countByStatus: Readonly<Record<ClaimStatus, number>>;
  readonly byType: Readonly<
    Record<ClaimType, { readonly count: number; readonly claimedAmount: number }>
  >;
  readonly totalClaimedAmount: number;
  readonly totalApprovedAmount: number;
}

export interface ClaimCapabilities {
  readonly canDeclare: boolean;
  readonly approvalLimit: number;
  readonly roles: readonly string[];
}

export interface DeclareClaimRequest {
  readonly policyNumber: string;
  readonly type: ClaimType;
  readonly incidentDate: string;
  readonly description: string;
  readonly claimedAmount: number;
}

export interface ClaimActionRequest {
  readonly action: ClaimAction;
  readonly expectedVersion: number;
  readonly reason?: string;
  readonly approvedAmount?: number;
}

export interface ClaimChangedNotification {
  readonly claimId: string;
  readonly number: string;
  readonly status: ClaimStatus;
  readonly actorName: string;
  readonly occurredAt: string;
}
