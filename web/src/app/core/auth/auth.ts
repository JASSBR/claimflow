import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { Persona, ROLES, Session } from './auth.models';

const STORAGE_KEY = 'claimflow.session';

/**
 * Holds the demo session. sessionStorage (not localStorage): closing the tab logs out, and the token never outlives
 * the browsing session. With a real OIDC provider this service would delegate to an OIDC client instead.
 */
@Injectable({ providedIn: 'root' })
export class Auth {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly session = signal<Session | null>(readSession());

  readonly user = computed(() => this.session()?.user ?? null);
  readonly isAuthenticated = computed(() => this.session() !== null);
  readonly isAuditor = computed(() => this.user()?.roles.includes(ROLES.auditor) ?? false);

  accessToken(): string | null {
    return this.session()?.accessToken ?? null;
  }

  personas(): Observable<Persona[]> {
    return this.http.get<Persona[]>('/api/auth/personas');
  }

  loginAs(personaId: string): Observable<Session> {
    return this.http.post<Session>('/api/auth/token', { personaId }).pipe(
      tap((session) => {
        sessionStorage.setItem(STORAGE_KEY, JSON.stringify(session));
        this.session.set(session);
      }),
    );
  }

  logout(): void {
    sessionStorage.removeItem(STORAGE_KEY);
    this.session.set(null);
    void this.router.navigate(['/login']);
  }
}

function readSession(): Session | null {
  try {
    const raw = sessionStorage.getItem(STORAGE_KEY);
    const session = raw ? (JSON.parse(raw) as Session) : null;
    return session && new Date(session.expiresAt) > new Date() ? session : null;
  } catch {
    return null;
  }
}
