using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;
using System.Text.Json;
using ClaimFlow.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClaimFlow.BuildingBlocks.Outbox;

internal static class OutboxTelemetry
{
    public static readonly ActivitySource ActivitySource = new("ClaimFlow.Outbox");
    private static readonly Meter Meter = new("ClaimFlow.Outbox");
    public static readonly Counter<long> Processed = Meter.CreateCounter<long>("claimflow.outbox.processed");
    public static readonly Counter<long> Failed = Meter.CreateCounter<long>("claimflow.outbox.failed");
}

public sealed class OutboxOptions
{
    public const string SectionName = "Outbox";

    public TimeSpan PollingInterval { get; init; } = TimeSpan.FromSeconds(2);

    public int BatchSize { get; init; } = 50;
}

/// <summary>
/// Publishes pending outbox rows to in-process handlers. Rows are claimed with FOR UPDATE SKIP LOCKED,
/// so several API replicas can run this loop concurrently without delivering the same row twice at the same time.
/// </summary>
public sealed partial class OutboxProcessor<TDbContext>(
    IServiceScopeFactory scopeFactory,
    OutboxEventRegistry registry,
    TimeProvider timeProvider,
    IOptions<OutboxOptions> options,
    ILogger<OutboxProcessor<TDbContext>> logger) : BackgroundService
    where TDbContext : DbContext
{
    /// <summary>Processes one batch. Public so integration tests can drain the outbox deterministically.</summary>
    public async Task<int> ProcessBatchAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var strategy = dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(
            async ct =>
            {
                dbContext.ChangeTracker.Clear();
                await using var transaction = await dbContext.Database.BeginTransactionAsync(ct);

                var messages = await dbContext.Set<OutboxMessage>()
                    .FromSqlRaw(BuildClaimSql(dbContext), options.Value.BatchSize, OutboxMessage.MaxAttempts)
                    .ToListAsync(ct)
                    ;

                foreach (var message in messages)
                {
                    await DispatchAsync(scope.ServiceProvider, message, ct);
                }

                await dbContext.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                return messages.Count;
            },
            cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(options.Value.PollingInterval, timeProvider);
        do
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            // Only a shutdown may end the loop. Any other exception — a handler's HttpClient timeout is an
            // OperationCanceledException too — must not escape: BackgroundService would stop the whole host.
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                LogBatchFailed(logger, ex);
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    // Identifiers come from the EF model (never from input); only values are parameters.
    private static string BuildClaimSql(DbContext dbContext)
    {
        var entity = dbContext.Model.FindEntityType(typeof(OutboxMessage))
            ?? throw new InvalidOperationException($"{typeof(TDbContext).Name} does not map the outbox. Call ApplyOutbox().");
        var table = $"\"{entity.GetSchema()}\".\"{entity.GetTableName()}\"";

        return $"""
            SELECT * FROM {table}
            WHERE "ProcessedAt" IS NULL AND "Attempts" < {"{1}"}
            ORDER BY "OccurredAt"
            LIMIT {"{0}"}
            FOR UPDATE SKIP LOCKED
            """;
    }

    private async Task DispatchAsync(IServiceProvider services, OutboxMessage message, CancellationToken cancellationToken)
    {
        using var activity = OutboxTelemetry.ActivitySource.StartActivity($"outbox.dispatch {message.Type}");
        try
        {
            if (!registry.TryResolve(message.Type, out var eventType))
            {
                throw new InvalidOperationException($"Unknown outbox event type '{message.Type}'.");
            }

            var domainEvent = (IDomainEvent)(JsonSerializer.Deserialize(message.Payload, eventType, JsonSerializerOptions.Web)
                ?? throw new InvalidOperationException($"Outbox message {message.Id} has an empty payload."));

            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
            foreach (var handler in services.GetServices(handlerType))
            {
                // DoNotWrapExceptions: a handler failure is recorded as itself, not as TargetInvocationException.
                var task = (Task)handlerType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync))!
                    .Invoke(handler, BindingFlags.DoNotWrapExceptions, binder: null, [domainEvent, cancellationToken], culture: null)!;
                await task;
            }

            message.ProcessedAt = timeProvider.GetUtcNow();
            message.LastError = null;
            OutboxTelemetry.Processed.Add(1, new KeyValuePair<string, object?>("event.type", eventType.Name));
        }
        // A cancellation that is not ours (e.g. a handler's HTTP timeout) is a failed attempt like any other;
        // letting it escape would roll back the batch without counting the attempt: a poison message retried forever.
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            message.Attempts++;
            message.LastError = ex.Message.Length > 2000 ? ex.Message[..2000] : ex.Message;
            OutboxTelemetry.Failed.Add(1);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            LogDispatchFailed(logger, ex, message.Id, message.Attempts);
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Outbox batch failed")]
    private static partial void LogBatchFailed(ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Outbox message {MessageId} failed (attempt {Attempts})")]
    private static partial void LogDispatchFailed(ILogger logger, Exception exception, Guid messageId, int attempts);
}
