import { TestBed } from '@angular/core/testing';
import { HubConnectionState } from '@microsoft/signalr';
import { CLAIMS_HUB_CONNECTION, ClaimsHubConnection, ClaimsRealtime } from './claims-realtime';

class FakeConnection implements ClaimsHubConnection {
  state = HubConnectionState.Disconnected;
  startError: Error | null = null;
  readonly handlers = new Map<string, (...args: unknown[]) => void>();
  private reconnecting = (): void => undefined;
  private reconnected = (): void => undefined;
  private closed = (): void => undefined;

  start = async (): Promise<void> => {
    if (this.startError) throw this.startError;
    this.state = HubConnectionState.Connected;
  };
  on = ((name: string, handler: (...args: unknown[]) => void) =>
    this.handlers.set(name, handler)) as unknown as ClaimsHubConnection['on'];
  onreconnecting = (callback: () => void): void => void (this.reconnecting = callback);
  onreconnected = (callback: () => void): void => void (this.reconnected = callback);
  onclose = (callback: () => void): void => void (this.closed = callback);

  emitReconnecting = () => this.reconnecting();
  emitReconnected = () => this.reconnected();
  emitClose = () => this.closed();
}

describe('ClaimsRealtime', () => {
  let connection: FakeConnection;
  let realtime: ClaimsRealtime;

  beforeEach(() => {
    connection = new FakeConnection();
    TestBed.configureTestingModule({
      providers: [{ provide: CLAIMS_HUB_CONNECTION, useValue: () => connection }],
    });
    realtime = TestBed.inject(ClaimsRealtime);
  });

  it('exposes server notifications as a signal', () => {
    connection.handlers.get('claimChanged')!({
      claimId: 'c1',
      number: 'SIN-1',
      status: 'Approved',
    });

    expect(realtime.lastChange()).toEqual({ claimId: 'c1', number: 'SIN-1', status: 'Approved' });
  });

  it('tracks the connection lifecycle', async () => {
    await realtime.connect();
    expect(realtime.connected()).toBe(true);

    connection.emitReconnecting();
    expect(realtime.connected()).toBe(false);
    connection.emitReconnected();
    expect(realtime.connected()).toBe(true);
    connection.emitClose();
    expect(realtime.connected()).toBe(false);
  });

  it('connects only once', async () => {
    const start = vi.spyOn(connection, 'start');

    await realtime.connect();
    await realtime.connect();

    expect(start).toHaveBeenCalledTimes(1);
  });

  it('degrades to offline mode instead of failing when the hub is unreachable', async () => {
    connection.startError = new Error('refused');

    await expect(realtime.connect()).resolves.toBeUndefined();
    expect(realtime.connected()).toBe(false);
  });
});
