export interface ClaimDocument {
  readonly id: string;
  readonly fileName: string;
  readonly contentType: string;
  readonly sizeBytes: number;
  readonly sha256: string;
  readonly uploadedBy: string;
  readonly uploadedAt: string;
}

export interface AnalysisCitation {
  readonly number: number;
  readonly documentId: string;
  readonly documentTitle: string;
  readonly citedText: string;
  readonly startPage: number | null;
  readonly endPage: number | null;
}

export interface ClaimAnalysis {
  readonly id: string;
  readonly createdAt: string;
  readonly requestedBy: string;
  readonly model: string;
  readonly refused: boolean;
  readonly content: string;
  readonly citations: readonly AnalysisCitation[];
  readonly documentCount: number;
  readonly inputTokens: number;
  readonly outputTokens: number;
}

export interface DocumentSettings {
  readonly aiEnabled: boolean;
  readonly aiModel: string | null;
  readonly maxSizeBytes: number;
  readonly maxDocumentsPerClaim: number;
  readonly acceptedContentTypes: readonly string[];
}
