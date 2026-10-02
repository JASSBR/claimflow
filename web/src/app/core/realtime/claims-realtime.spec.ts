import { TestBed } from '@angular/core/testing';
import { HubConnectionState } from '@microsoft/signalr';
import { ToastService } from '../toast';
import { fakeAuth } from '../../testing';
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
  stop = async (): Promise<void> => {
    this.state = HubConnectionState.Disconnected;
  };
  on = ((name: string, handler: (...args: unknown[]) => void) =>
    this.handlers.set(name, handler)) as unknown as ClaimsHubConnection['on'];
  onreconnecting = (callback: () => void): void => void (this.reconnecting = callback);
  onreconnected = (callback: () => void): void => void (this.reconnected = callback);
  onclose = (callback: () => void): void => void (this.closed = callback);

  emit = (change: object) => this.handlers.get('claimChanged')!(change);
  emitReconnecting = () => this.reconnecting();
  emitReconnected = () => this.reconnected();
  emitClose = () => this.closed();
}

describe('ClaimsRealtime', () => {
  let connection: FakeConnection;
  let realtime: ClaimsRealtime;
  let toasts: ToastService;

  const change = (actorName: string, n = 1) => ({
    claimId: `c${n}`,
    number: `SIN-${n}`,
    status: 'Approved',
    actorName,
    occurredAt: '2026-10-02T10:00:00Z',
  });

  beforeEach(() => {
    connection = new FakeConnection();
    TestBed.configureTestingModule({
      providers: [
        fakeAuth().provider,
        { provide: CLAIMS_HUB_CONNECTION, useValue: () => connection },
      ],
    });
    realtime = TestBed.inject(ClaimsRealtime);
    toasts = TestBed.inject(ToastService);
  });

  it('exposes notifications as a signal and an activity feed, newest first', () => {
    connection.emit(change('Karim Benali', 1));
    connection.emit(change('Karim Benali', 2));

    expect(realtime.lastChange()?.claimId).toBe('c2');
    expect(realtime.activity().map((item) => item.claimId)).toEqual(['c2', 'c1']);
  });

  it('toasts colleagues’ decisions but not the user’s own', () => {
    connection.emit(change('Léa Martin'));
    expect(toasts.toasts()).toHaveLength(0);

    connection.emit(change('Karim Benali'));
    expect(toasts.toasts()[0]?.title).toBe('Karim Benali a accepté SIN-1');
  });

  it('tracks the connection lifecycle and connects only once', async () => {
    const start = vi.spyOn(connection, 'start');
    await realtime.connect();
    await realtime.connect();
    expect(start).toHaveBeenCalledTimes(1);
    expect(realtime.connected()).toBe(true);

    connection.emitReconnecting();
    expect(realtime.connected()).toBe(false);
    connection.emitReconnected();
    expect(realtime.connected()).toBe(true);
    connection.emitClose();
    expect(realtime.connected()).toBe(false);
  });

  it('degrades to offline mode instead of failing when the hub is unreachable', async () => {
    connection.startError = new Error('refused');
    await expect(realtime.connect()).resolves.toBeUndefined();
    expect(realtime.connected()).toBe(false);
  });

  it('clears the feed on disconnect (e.g. when switching persona)', async () => {
    connection.emit(change('Karim Benali'));
    await realtime.disconnect();
    expect(realtime.activity()).toEqual([]);
  });
});
