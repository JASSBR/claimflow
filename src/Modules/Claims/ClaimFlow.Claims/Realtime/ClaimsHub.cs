using ClaimFlow.BuildingBlocks.Outbox;
using ClaimFlow.Claims.Contracts;
using ClaimFlow.Claims.Domain;
using Microsoft.AspNetCore.SignalR;

namespace ClaimFlow.Claims.Realtime;

/// <summary>Server → client only: clients subscribe to "claimChanged" and refresh what they display.</summary>
internal sealed class ClaimsHub : Hub
{
    public const string Path = "/hubs/claims";
    public const string ClaimChangedMethod = "claimChanged";
}

/// <summary>
/// Driven by the outbox, so clients are only told about changes that are actually committed.
/// Idempotent by nature: a duplicate delivery just triggers one more refresh.
/// </summary>
internal sealed class ClaimChangedNotifier(IHubContext<ClaimsHub> hub)
    : IDomainEventHandler<ClaimDeclared>, IDomainEventHandler<ClaimStatusChanged>
{
    public Task HandleAsync(ClaimDeclared domainEvent, CancellationToken cancellationToken) =>
        PublishAsync(new ClaimChangedNotification(domainEvent.ClaimId.Value, domainEvent.Number, ClaimStatus.Declared), cancellationToken);

    public Task HandleAsync(ClaimStatusChanged domainEvent, CancellationToken cancellationToken) =>
        PublishAsync(new ClaimChangedNotification(domainEvent.ClaimId.Value, domainEvent.Number, domainEvent.To), cancellationToken);

    private Task PublishAsync(ClaimChangedNotification notification, CancellationToken cancellationToken) =>
        hub.Clients.All.SendAsync(ClaimsHub.ClaimChangedMethod, notification, cancellationToken);
}
