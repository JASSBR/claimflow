import { InjectionToken, Injectable, inject, signal } from '@angular/core';
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from '@microsoft/signalr';
import { ClaimChangedNotification } from './claim.models';

/** The subset of HubConnection this app uses: lets tests drive the service without a server. */
export type ClaimsHubConnection = Pick<
  HubConnection,
  'state' | 'start' | 'on' | 'onreconnecting' | 'onreconnected' | 'onclose'
>;

export const CLAIMS_HUB_CONNECTION = new InjectionToken<() => ClaimsHubConnection>(
  'CLAIMS_HUB_CONNECTION',
  {
    providedIn: 'root',
    factory: () => () =>
      new HubConnectionBuilder()
        .withUrl('/hubs/claims')
        .withAutomaticReconnect()
        .configureLogging(LogLevel.Warning)
        .build(),
  },
);

/**
 * One SignalR connection for the whole app. Screens don't subscribe to anything:
 * they read `lastChange` inside their resource request, so a committed change refetches exactly what is on screen.
 */
@Injectable({ providedIn: 'root' })
export class ClaimsRealtime {
  readonly lastChange = signal<ClaimChangedNotification | null>(null);
  readonly connected = signal(false);

  private readonly connection = inject(CLAIMS_HUB_CONNECTION)();

  constructor() {
    this.connection.on('claimChanged', (change: ClaimChangedNotification) =>
      this.lastChange.set(change),
    );
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
}
