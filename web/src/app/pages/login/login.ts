import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { httpResource } from '@angular/common/http';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { Auth } from '../../core/auth/auth';
import { Persona, ROLES } from '../../core/auth/auth.models';
import { Avatar } from '../../shared/avatar';
import { Icon, IconName } from '../../shared/icon';
import { problemMessages } from '../../shared/problem-details';

interface Highlight {
  readonly icon: IconName;
  readonly title: string;
  readonly text: string;
}

@Component({
  selector: 'app-login',
  imports: [Avatar, Icon],
  templateUrl: './login.html',
  styleUrl: './login.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Login {
  private readonly auth = inject(Auth);
  private readonly router = inject(Router);

  protected readonly repositoryUrl = environment.repositoryUrl;
  protected readonly personas = httpResource<Persona[]>(() => '/api/auth/personas');
  protected readonly pending = signal<string | null>(null);
  protected readonly error = signal<string | null>(null);

  protected readonly highlights: readonly Highlight[] = [
    {
      icon: 'shield',
      title: 'Règles métier réelles',
      text: 'Délégation de pouvoir, principe des quatre yeux, prescription biennale, piste d’audit.',
    },
    {
      icon: 'bolt',
      title: 'Temps réel fiable',
      text: 'Chaque décision se propage instantanément, via une outbox transactionnelle et SignalR.',
    },
    {
      icon: 'sparkles',
      title: 'IA explicable',
      text: 'La revue de dossier cite la page exacte de chaque pièce. L’humain décide.',
    },
    {
      icon: 'lock',
      title: 'Qualité vérifiée',
      text: '120+ tests, architecture imposée par des tests, CI avec seuil de couverture.',
    },
  ];

  protected readonly stack = [
    '.NET 10',
    'ASP.NET Core',
    'EF Core',
    'PostgreSQL',
    'Angular 22',
    'Signals',
    'SignalR',
    '.NET Aspire',
    'Claude',
    'Azure',
  ];

  protected roleLabel(persona: Persona): string {
    if (persona.roles.includes(ROLES.manager)) return 'Responsable';
    if (persona.roles.includes(ROLES.auditor)) return 'Lecture seule';
    return 'Gestionnaire';
  }

  protected enter(persona: Persona): void {
    this.pending.set(persona.id);
    this.error.set(null);
    this.auth.loginAs(persona.id).subscribe({
      next: () => void this.router.navigate(['/dashboard']),
      error: (error: unknown) => {
        this.pending.set(null);
        this.error.set(problemMessages(error)[0] ?? null);
      },
    });
  }
}
