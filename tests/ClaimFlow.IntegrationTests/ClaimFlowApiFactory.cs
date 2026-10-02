using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using ClaimFlow.BuildingBlocks.Outbox;
using ClaimFlow.Claims.Domain;
using ClaimFlow.Claims.Persistence;
using ClaimFlow.Documents.Analysis;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.Azurite;
using Testcontainers.PostgreSql;

namespace ClaimFlow.IntegrationTests;

/// <summary>
/// Boots the real API against a throwaway PostgreSQL container: same migrations, same SQL (xmin, SKIP LOCKED, jsonb)
/// as production. An in-memory provider would silently skip exactly the behaviour these tests exist to prove.
/// </summary>
public sealed class ClaimFlowApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    /// <summary>Random per run: proves nothing depends on a committed key.</summary>
    public static readonly string DemoSigningKey = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18-alpine").Build();
    private readonly AzuriteContainer _azurite = new AzuriteBuilder("mcr.microsoft.com/azure-storage/azurite:latest").Build();
    private readonly ConcurrentDictionary<string, string> _tokens = new(StringComparer.Ordinal);

    public async ValueTask InitializeAsync()
    {
        await Task.WhenAll(_postgres.StartAsync(), _azurite.StartAsync());
        // Forces host start-up (migrations run) before the first test.
        _ = Server;
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
        await _azurite.DisposeAsync();
    }

    /// <summary>An HTTP client authenticated as a demo persona (lea, karim, nadia, sophie) through the real token endpoint.</summary>
    public async Task<HttpClient> ClientForAsync(string personaId)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await TokenForAsync(personaId));
        return client;
    }

    public async Task<string> TokenForAsync(string personaId)
    {
        if (_tokens.TryGetValue(personaId, out var cached))
        {
            return cached;
        }

        using var anonymous = CreateClient();
        var response = await anonymous.PostAsJsonAsync("/api/auth/token", new { personaId });
        response.EnsureSuccessStatusCode();
        var token = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("accessToken").GetString()!;
        return _tokens.GetOrAdd(personaId, token);
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
        builder.UseSetting("ConnectionStrings:claimflow", _postgres.GetConnectionString());
        builder.UseSetting("ConnectionStrings:blobs", _azurite.GetConnectionString());
        // Enables the assistant; the real Claude client is replaced below, CI never calls a paid API.
        builder.UseSetting("Ai:ApiKey", "test-key");
        builder.UseSetting("Database:InitializeOnStartup", "true");
        // Seeding runs too: it exercises the cross-module directory and real PDF generation on every CI run.
        builder.UseSetting("Database:SeedDemoData", "true");
        // The poller is parked: tests drain the outbox explicitly, so assertions never race a background loop.
        builder.UseSetting("Outbox:PollingInterval", "01:00:00");
        builder.UseSetting("RateLimiting:PermitPerMinute", "100000");
        builder.UseSetting("Auth:Mode", "Demo");
        builder.UseSetting("Cors:AllowedOrigins:0", "https://claimflow.example");
        builder.UseSetting("Auth:DemoSigningKey", DemoSigningKey);
        builder.ConfigureTestServices(services =>
        {
            services.AddScoped<IDomainEventHandler<ClaimDeclared>, PoisonHandler>();
            services.AddScoped<IClaimAnalyst, FakeClaimAnalyst>();
        });
    }
}

[CollectionDefinition(Name)]
public sealed class ApiTestGroup : ICollectionFixture<ClaimFlowApiFactory>
{
    public const string Name = "api";
}
