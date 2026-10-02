import { HttpErrorResponse } from '@angular/common/http';

/** RFC 9457 problem details, as returned by every ClaimFlow API error. */
interface ProblemDetails {
  readonly title?: string;
  readonly status?: number;
  readonly code?: string;
  readonly errors?: Readonly<Record<string, readonly string[]>>;
}

/** Flattens an API error into human-readable messages: every violated business rule, or the problem title. */
export function problemMessages(error: unknown): string[] {
  if (!(error instanceof HttpErrorResponse)) {
    return ['Erreur inattendue.'];
  }
  if (error.status === 0) {
    return ["L'API est injoignable. Vérifiez votre connexion."];
  }
  const problem = (error.error ?? {}) as ProblemDetails;
  const fieldMessages = Object.values(problem.errors ?? {}).flat();
  if (fieldMessages.length > 0) {
    return fieldMessages;
  }
  return [problem.title ?? `Erreur ${error.status}.`];
}

export function isConflict(error: unknown): boolean {
  return error instanceof HttpErrorResponse && error.status === 409;
}
