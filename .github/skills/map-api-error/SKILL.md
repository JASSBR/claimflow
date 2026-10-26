---
name: map-api-error
description: "Map a backend RFC 9457 domain error code to a localized user-facing message in the web portal error registry."
---

> Evidence: value gate verified the facts below against this repository.

Map a backend RFC 9457 domain error code to a localized user-facing message in the web portal error registry.

Applies in: `claimflow-web`.

## What is specific to this repository

- Domain error codes are mapped to localized messages in the MESSAGES dictionary. [web/src/app/shared/problem-details.ts:L15-L15]
- Error mappings in MESSAGES use $localize with an @@error.<identifier> localization id and French text. [web/src/app/shared/problem-details.ts:L16-L16]
- Consuming UI components call problemMessages on caught errors to retrieve localized error messages. [web/src/app/pages/login/login.ts:L85-L85]

## Rules this change touches

- [Invariant] While an authentication request for a persona is in flight, the pending signal stores that persona's identifier and is cleared only if the login operation encounters an error. (approved, conviction 1.00, web/src/app/pages/login/login.ts:L78-L88)
- [BusinessRule] When mapping a persona's assigned roles to a localized display label, if the persona includes the 'claims.manager' role, the label is 'Responsable'; otherwise if it includes 'claims.auditor', the label is 'Lecture seule'; otherwise the default label is 'Gestionnaire'. (approved, conviction 1.00, web/src/app/pages/login/login.ts:L72-L76)

## Context

- The enter method triggers the login sequence for a given persona: it marks the persona id as pending, clears any previous error, requests a session token via Auth.loginAs(persona.id), navigates to '/dashboard' on success, or sets the error signal to the first message extracted by problemMessages(error) and resets pending on failure. [web/src/app/pages/login/login.ts:L78-L88]
- Highlight defines the structure for marketing and architecture feature highlights displayed on the login page, containing an icon name (constrained by IconName), a localized title, and localized description text. [web/src/app/pages/login/login.ts:L20-L24]
- The Login component serves as the application's authentication page, enabling demo users to select and log in as predefined personas (e.g. Handler, Manager, Auditor), view technical and domain highlights, see the tech stack, switch language locale, and navigate to the dashboard upon successful authentication. [web/src/app/pages/login/login.ts:L26-L40]

## Files you will probably also touch

| File | Why |
|---|---|
| `web/src/app/app.routes.ts` | referenced by or referencing this skill's files |
| `web/src/app/core/auth/auth.models.ts` | referenced by or referencing this skill's files |
| `web/src/app/core/auth/auth.ts` | referenced by or referencing this skill's files |
| `web/src/app/core/locale.ts` | referenced by or referencing this skill's files |
| `web/src/app/pages/claim-declare/claim-declare.ts` | referenced by or referencing this skill's files |
| `web/src/app/pages/claim-detail/analysis-panel.ts` | referenced by or referencing this skill's files |
| `web/src/app/pages/claim-detail/claim-detail.ts` | referenced by or referencing this skill's files |
| `web/src/app/pages/claim-detail/documents-panel.ts` | referenced by or referencing this skill's files |

## Start here

`web/src/app/shared/problem-details.ts` [web/src/app/shared/problem-details.ts:L13-L52] is the worked example for this change, as it stands in the repository:

```typescript
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
```

## The same change elsewhere in this repository

- `web/src/app/pages/login/login.ts` [web/src/app/pages/login/login.ts:L11-L11]: `import { problemMessages } from '../../shared/problem-details';`
- `web/src/app/pages/login/login.ts` [web/src/app/pages/login/login.ts:L85-L85]: `this.error.set(problemMessages(error)[0] ?? null);`

## Patterns

Required:
- `$localize`
- `MESSAGES`
- `problemMessages`
Forbidden:
- Avoid `alert(` in this change.

## Workflow

1. Identify the domain error code returned by the API in problem details (e.g. claim.*, document.*, analysis.*). [web/src/app/shared/problem-details.ts:L3-L3]
2. Open web/src/app/shared/problem-details.ts and locate the MESSAGES dictionary. [web/src/app/shared/problem-details.ts:L16-L16]
3. Add the code mapping to MESSAGES using $localize with a scoped translation identifier :@@error.<ruleName>:. [web/src/app/shared/problem-details.ts:L15-L15]
4. In the consuming component, import problemMessages and invoke it on the caught HttpErrorResponse. [web/src/app/shared/problem-details.ts:L1-L1]
5. Verify error display in UI components via problemMessages(error). [web/src/app/shared/problem-details.ts:L40-L40]

## Checklist

- [ ] The error code key in MESSAGES matches the RFC 9457 error code or problem code returned by the backend exactly. [web/src/app/shared/problem-details.ts:L15-L15]
- [ ] Caller handles errors using problemMessages(error) to display the translated error text. [web/src/app/shared/problem-details.ts:L40-L40]
- [ ] Does the error code key match the code returned in RFC 9457 ProblemDetails from the backend? [web/src/app/shared/problem-details.ts:L4-L4]
- [ ] Are components using problemMessages(error) to extract translated error messages? [web/src/app/shared/problem-details.ts:L40-L40]
