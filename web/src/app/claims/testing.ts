import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ClaimChangedNotification, ClaimDetails } from './claim.models';
import { ClaimsRealtime } from './claims-realtime';

/** Stands in for the SignalR connection: tests push notifications by setting lastChange. */
export function fakeRealtime() {
  const fake = {
    lastChange: signal<ClaimChangedNotification | null>(null),
    connected: signal(true),
    connect: async () => undefined,
  };
  return { fake, provider: { provide: ClaimsRealtime, useValue: fake } };
}

export function claimDetails(overrides: Partial<ClaimDetails> = {}): ClaimDetails {
  return {
    id: 'c1',
    number: 'SIN-2026-0000ABCD',
    policyNumber: 'POL-104233',
    type: 'Auto',
    status: 'UnderReview',
    claimedAmount: 1500,
    approvedAmount: null,
    incidentDate: '2026-09-28',
    declaredAt: '2026-09-29T08:00:00Z',
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
