export interface Persona {
  readonly id: string;
  readonly name: string;
  readonly title: string;
  readonly summary: string;
  readonly roles: readonly string[];
}

export interface Session {
  readonly accessToken: string;
  readonly expiresAt: string;
  readonly user: Persona;
}

export const ROLES = {
  handler: 'claims.handler',
  manager: 'claims.manager',
  auditor: 'claims.auditor',
} as const;
