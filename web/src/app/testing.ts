import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Auth } from './core/auth/auth';
import { Persona } from './core/auth/auth.models';
import { ClaimsRealtime } from './core/realtime/claims-realtime';
import { ClaimChangedNotification, ClaimDetails } from './claims/claim.models';

/** Stands in for the SignalR connection: tests push notifications by setting lastChange. */
export function fakeRealtime() {
  const fake = {
    lastChange: signal<ClaimChangedNotification | null>(null),
    activity: signal<readonly ClaimChangedNotification[]>([]),
    connected: signal(true),
    connect: async () => undefined,
    disconnect: async () => undefined,
  };
  return { fake, provider: { provide: ClaimsRealtime, useValue: fake } };
}

export const LEA: Persona = {
  id: 'lea',
  name: 'Léa Martin',
  title: 'Gestionnaire sinistres',
  summary: '',
  roles: ['claims.handler'],
};
export const KARIM: Persona = {
  id: 'karim',
  name: 'Karim Benali',
  title: 'Responsable indemnisation',
  summary: '',
  roles: ['claims.manager'],
};

export function fakeAuth(user: Persona = LEA) {
  const current = signal<Persona | null>(user);
  const fake = {
    user: current,
    isAuthenticated: () => current() !== null,
    isAuditor: () => current()?.roles.includes('claims.auditor') ?? false,
    accessToken: () => 'token',
    logout: () => current.set(null),
  };
  return { fake, provider: { provide: Auth, useValue: fake } };
}

export function claimDetails(overrides: Partial<ClaimDetails> = {}): ClaimDetails {
  return {
    id: 'c1',
    number: 'SIN-2026-000001',
    policyNumber: 'POL-104233',
    type: 'Auto',
    status: 'UnderReview',
    claimedAmount: 1500,
    approvedAmount: null,
    incidentDate: '2026-09-28',
    declaredAt: '2026-09-29T08:00:00Z',
    declaredBy: 'Léa Martin',
    lastUpdatedAt: '2026-09-29T09:00:00Z',
    description: 'Rear-end collision.',
    version: 7,
    allowedActions: ['RequestInformation', 'Approve', 'Reject'],
    history: [],
    ...overrides,
  };
}

/**
 * Lets promise callbacks run, then flushes signal effects. Needed where whenStable() can't be used:
 * it also waits for in-flight HTTP requests, which a test must flush itself first.
 */
export async function settle(): Promise<void> {
  await new Promise((resolve) => setTimeout(resolve));
  TestBed.tick();
}
