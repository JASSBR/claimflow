import { Injectable, InjectionToken, inject, signal } from '@angular/core';
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from '@microsoft/signalr';
import { ClaimChangedNotification } from '../../claims/claim.models';
import { STATUS_VERBS } from '../../claims/claim-labels';
import { API_BASE_URL } from '../api-base-url';
import { Auth } from '../auth/auth';
import { ToastService } from '../toast';

/** The subset of HubConnection this app uses: lets tests drive the service without a server. */
export type ClaimsHubConnection = Pick<
  HubConnection,
  'state' | 'start' | 'stop' | 'on' | 'onreconnecting' | 'onreconnected' | 'onclose'
>;

export const CLAIMS_HUB_CONNECTION = new InjectionToken<() => ClaimsHubConnection>(
  'CLAIMS_HUB_CONNECTION',
  {
    providedIn: 'root',
    factory: () => {
      const auth = inject(Auth);
      const baseUrl = inject(API_BASE_URL);
      return () =>
        new HubConnectionBuilder()
          .withUrl(`${baseUrl}/hubs/claims`, { accessTokenFactory: () => auth.accessToken() ?? '' })
          .withAutomaticReconnect()
          .configureLogging(LogLevel.Warning)
          .build();
    },
  },
);

const ACTIVITY_SIZE = 30;

/**
 * One SignalR connection for the whole app. Screens don't subscribe to anything:
 * they read `lastChange` inside their resource request, so a committed change refetches exactly what is on screen.
 * Changes made by colleagues also feed the activity list and a toast.
 */
@Injectable({ providedIn: 'root' })
export class ClaimsRealtime {
  private readonly auth = inject(Auth);
  private readonly toasts = inject(ToastService);

  readonly lastChange = signal<ClaimChangedNotification | null>(null);
  readonly activity = signal<readonly ClaimChangedNotification[]>([]);
  readonly connected = signal(false);

  private readonly connection = inject(CLAIMS_HUB_CONNECTION)();

  constructor() {
    this.connection.on('claimChanged', (change: ClaimChangedNotification) => this.receive(change));
    this.connection.onreconnecting(() => this.connected.set(false));
    this.connection.onreconnected(() => this.connected.set(true));
    this.connection.onclose(() => this.connected.set(false));
  }

  async connect(): Promise<void> {
    if (this.connection.state !== HubConnectionState.Disconnected) {
      return;
    }
    try {
      await this.connection.start();
      this.connected.set(true);
    } catch {
      // Live updates are a comfort, not a requirement: the app keeps working on plain HTTP.
      this.connected.set(false);
    }
  }

  async disconnect(): Promise<void> {
    await this.connection.stop();
    this.activity.set([]);
  }

  private receive(change: ClaimChangedNotification): void {
    this.lastChange.set(change);
    this.activity.update((items) => [change, ...items].slice(0, ACTIVITY_SIZE));
    if (change.actorName !== this.auth.user()?.name) {
      this.toasts.show({
        tone: 'info',
        title: `${change.actorName} ${STATUS_VERBS[change.status]} ${change.number}`,
      });
    }
  }
}
