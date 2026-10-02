import { HttpErrorResponse } from '@angular/common/http';

/** RFC 9457 problem details, as returned by every ClaimFlow API error. */
interface ProblemDetails {
  readonly title?: string;
  readonly status?: number;
  readonly code?: string;
  readonly errors?: Readonly<Record<string, readonly string[]>>;
}

/**
 * The API speaks stable error codes; the UI owns the wording, in the user's language.
 * An unknown code falls back to the server's (English) description, so a new rule is never silent.
 */
const MESSAGES: Readonly<Record<string, string>> = {
  'claim.policy_number_invalid': $localize`:@@error.policyNumber:Le numéro de contrat doit respecter le format POL-000000.`,
  'claim.incident_date_in_future': $localize`:@@error.futureDate:La date de survenance ne peut pas être dans le futur.`,
  'claim.incident_time_barred': $localize`:@@error.timeBarred:Sinistre prescrit : plus de deux ans se sont écoulés (art. L114-1 du Code des assurances).`,
  'claim.description_length': $localize`:@@error.descriptionLength:Les circonstances doivent faire entre 10 et 2000 caractères.`,
  'claim.claimed_amount_out_of_range': $localize`:@@error.claimedAmount:Le montant réclamé doit être positif et inférieur à 1 000 000 €.`,
  'claim.type_invalid': $localize`:@@error.type:Type de sinistre inconnu.`,
  'claim.reason_required': $localize`:@@error.reason:Un motif est obligatoire pour cette décision.`,
  'claim.approved_amount_out_of_range': $localize`:@@error.approvedAmount:Le montant accordé doit être positif et ne peut pas dépasser le montant réclamé.`,
  'claim.approval_limit_exceeded': $localize`:@@error.approvalLimit:Ce montant dépasse votre délégation de pouvoir : un responsable doit accepter ce dossier.`,
  'claim.four_eyes_violation': $localize`:@@error.fourEyes:Principe des quatre yeux : la personne qui a accepté un dossier ne peut pas en verser l'indemnité.`,
  'claim.action_forbidden': $localize`:@@error.actionForbidden:Votre rôle ne permet pas cette action.`,
  'claim.transition_not_allowed': $localize`:@@error.transition:Cette action n'est pas possible dans l'état actuel du dossier.`,
  'claim.concurrent_update': $localize`:@@error.concurrent:Le dossier vient d'être modifié par quelqu'un d'autre. Sa nouvelle version est affichée : vérifiez-la avant de décider.`,
  'claim.not_found': $localize`:@@error.claimNotFound:Ce sinistre n'existe pas.`,
  'document.unsupported_format': $localize`:@@error.documentFormat:Seuls les PDF, PNG et JPEG sont acceptés (format vérifié sur le contenu du fichier, pas sur son nom).`,
  'document.too_large': $localize`:@@error.documentSize:Un document ne peut pas dépasser 10 Mo.`,
  'document.empty': $localize`:@@error.documentEmpty:Le fichier est vide.`,
  'document.too_many': $localize`:@@error.documentCount:Le dossier a atteint le nombre maximal de pièces.`,
  'document.not_found': $localize`:@@error.documentNotFound:Cette pièce n'existe pas sur ce dossier.`,
  'analysis.ai_disabled': $localize`:@@error.aiDisabled:L'assistant IA n'est pas configuré sur cet environnement.`,
  'analysis.no_documents': $localize`:@@error.noDocuments:Ajoutez au moins une pièce avant de lancer la revue.`,
};

/** Flattens an API error into human-readable messages: every violated business rule, or the problem title. */
export function problemMessages(error: unknown): string[] {
  if (!(error instanceof HttpErrorResponse)) {
    return [$localize`:@@error.unexpected:Erreur inattendue.`];
  }
  if (error.status === 0) {
    return [$localize`:@@error.network:L'API est injoignable. Vérifiez votre connexion.`];
  }
  if (error.status === 429) {
    return [
      $localize`:@@error.rateLimited:Trop de demandes en peu de temps. Réessayez dans quelques minutes.`,
    ];
  }
  const problem = (error.error ?? {}) as ProblemDetails;
  const fieldMessages = Object.entries(problem.errors ?? {}).flatMap(
    ([code, descriptions]) => MESSAGES[code] ?? descriptions,
  );
  if (fieldMessages.length > 0) {
    return fieldMessages;
  }
  return [
    (problem.code && MESSAGES[problem.code]) ||
      problem.title ||
      $localize`:@@error.status:Erreur ${error.status}:status:.`,
  ];
}

export function isConflict(error: unknown): boolean {
  return error instanceof HttpErrorResponse && error.status === 409;
}
