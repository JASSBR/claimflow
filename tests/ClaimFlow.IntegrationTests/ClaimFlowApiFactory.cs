using ClaimFlow.BuildingBlocks.Outbox;
using ClaimFlow.Claims.Domain;
using ClaimFlow.Claims.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace ClaimFlow.IntegrationTests;

/// <summary>
/// Boots the real API against a throwaway PostgreSQL container: same migrations, same SQL (xmin, SKIP LOCKED, jsonb)
/// as production. An in-memory provider would silently skip exactly the behaviour these tests exist to prove.
/// </summary>
public sealed class ClaimFlowApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18-alpine").Build();

    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync();
        // Forces host start-up (migrations run) before the first test.
        _ = Server;
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    /// <summary>Delivers every pending outbox row now instead of waiting for the background poller.</summary>
    public async Task DrainOutboxAsync()
    {
        var processor = Services.GetRequiredService<OutboxProcessor<ClaimsDbContext>>();
        while (await processor.ProcessBatchAsync(CancellationToken.None) > 0)
        {
            // Keep going until a batch comes back empty.
        }
    }

    internal async Task<T> QueryDbAsync<T>(Func<ClaimsDbContext, Task<T>> query)
    {
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();
        return await query(dbContext);
    }

    /// <summary>Rows still due for delivery (parked rows that exhausted their attempts are excluded).</summary>
    public Task<int> PendingOutboxCountAsync() =>
        QueryDbAsync(db => db.Set<OutboxMessage>().CountAsync(message => message.ProcessedAt == null && message.Attempts < OutboxMessage.MaxAttempts));

    internal Task<List<OutboxMessage>> OutboxMessagesForAsync(Guid claimId) =>
        QueryDbAsync(async db => (await db.Set<OutboxMessage>().AsNoTracking().ToListAsync())
            .Where(message => message.Payload.Contains(claimId.ToString(), StringComparison.OrdinalIgnoreCase))
            .ToList());

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:claimsdb", _postgres.GetConnectionString());
        builder.UseSetting("Database:InitializeOnStartup", "true");
        builder.UseSetting("Database:SeedDemoData", "false");
        // The poller is parked: tests drain the outbox explicitly, so assertions never race a background loop.
        builder.UseSetting("Outbox:PollingInterval", "01:00:00");
        builder.UseSetting("RateLimiting:PermitPerMinute", "100000");
        builder.ConfigureTestServices(services => services.AddScoped<IDomainEventHandler<ClaimDeclared>, PoisonHandler>());
    }
}

[CollectionDefinition(Name)]
public sealed class ApiTestGroup : ICollectionFixture<ClaimFlowApiFactory>
{
    public const string Name = "api";
}
