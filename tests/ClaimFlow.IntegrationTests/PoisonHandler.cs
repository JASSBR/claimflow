using System.Collections.Concurrent;
using ClaimFlow.BuildingBlocks.Outbox;
using ClaimFlow.Claims.Domain;

namespace ClaimFlow.IntegrationTests;

/// <summary>Test-only handler that fails for the claims a test marks, the two ways real handlers fail.</summary>
internal sealed class PoisonHandler : IDomainEventHandler<ClaimDeclared>
{
    public enum Failure
    {
        /// <summary>Throws before returning a Task — reflection would wrap it in TargetInvocationException.</summary>
        Synchronous,

        /// <summary>Faults with TaskCanceledException, like an HttpClient timeout, while the host is NOT shutting down.</summary>
        Timeout,
    }

    private static readonly ConcurrentDictionary<Guid, Failure> Poisoned = new();

    public static void Poison(Guid claimId, Failure failure) => Poisoned[claimId] = failure;

    public Task HandleAsync(ClaimDeclared domainEvent, CancellationToken cancellationToken)
    {
        if (!Poisoned.TryGetValue(domainEvent.ClaimId.Value, out var failure))
        {
            return Task.CompletedTask;
        }

        return failure == Failure.Synchronous
            ? throw new InvalidOperationException("sync handler failure")
            : TimeoutAsync();
    }

    private static async Task TimeoutAsync()
    {
        await Task.Yield();
        throw new TaskCanceledException("handler timeout");
    }
}
