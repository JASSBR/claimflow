import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import {
  FormField,
  form,
  maxLength,
  min,
  minLength,
  pattern,
  required,
  submit,
  validate,
} from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { problemMessages } from '../../shared/problem-details';
import { TYPE_LABELS } from '../claim-labels';
import { CLAIM_TYPES, ClaimType } from '../claim.models';
import { ClaimsApi } from '../claims-api';

interface DeclareModel {
  policyNumber: string;
  type: ClaimType;
  incidentDate: string;
  description: string;
  claimedAmount: number;
}

const today = (): string => new Date().toISOString().slice(0, 10);

@Component({
  selector: 'app-claim-declare',
  imports: [FormField, RouterLink],
  templateUrl: './claim-declare.html',
  styleUrl: './claim-declare.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ClaimDeclare {
  private readonly api = inject(ClaimsApi);
  private readonly router = inject(Router);

  protected readonly types = CLAIM_TYPES;
  protected readonly typeLabels = TYPE_LABELS;
  protected readonly serverErrors = signal<string[]>([]);

  protected readonly model = signal<DeclareModel>({
    policyNumber: '',
    type: 'Auto',
    incidentDate: today(),
    description: '',
    claimedAmount: 0,
  });

  // Client rules mirror the domain for instant feedback; the API stays the authority (its errors are shown too).
  protected readonly form = form(this.model, (path) => {
    required(path.policyNumber, { message: 'Le numéro de contrat est obligatoire.' });
    pattern(path.policyNumber, /^POL-\d{6}$/i, { message: 'Format attendu : POL-000000.' });
    required(path.incidentDate, { message: 'La date de survenance est obligatoire.' });
    validate(path.incidentDate, ({ value }) =>
      value() > today()
        ? { kind: 'future', message: 'La date ne peut pas être dans le futur.' }
        : null,
    );
    required(path.description, { message: 'Les circonstances sont obligatoires.' });
    minLength(path.description, 10, { message: 'Décrivez le sinistre en 10 caractères minimum.' });
    maxLength(path.description, 2000, { message: '2000 caractères maximum.' });
    min(path.claimedAmount, 0.01, { message: 'Le montant doit être positif.' });
  });

  protected async onSubmit(event: Event): Promise<void> {
    event.preventDefault();
    this.serverErrors.set([]);
    await submit(this.form, async () => {
      try {
        const claim = await firstValueFrom(this.api.declare(this.model()));
        await this.router.navigate(['/claims', claim.id]);
      } catch (error) {
        this.serverErrors.set(problemMessages(error));
      }
      return undefined;
    });
  }
}
