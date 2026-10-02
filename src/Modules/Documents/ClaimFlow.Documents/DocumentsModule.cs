using System.Threading.RateLimiting;
using Anthropic;
using ClaimFlow.BuildingBlocks.Security;
using ClaimFlow.Claims.Contracts;
using ClaimFlow.Documents.Analysis;
using ClaimFlow.Documents.Features;
using ClaimFlow.Documents.Persistence;
using ClaimFlow.Documents.Security;
using ClaimFlow.Documents.Seeding;
using ClaimFlow.Documents.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace ClaimFlow.Documents;

/// <summary>The Documents module's only public entry point.</summary>
public static class DocumentsModule
{
    public const string BlobsConnectionName = "blobs";

    /// <summary>The monolith's shared database (schema "documents" is ours).</summary>
    public const string DatabaseConnectionName = "claimflow";

    public static IHostApplicationBuilder AddDocumentsModule(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Same database as Claims, own schema and own migrations history (ADR 0001).
        builder.Services.AddDbContextPool<DocumentsDbContext>((provider, options) => options
            .UseNpgsql(
                new NpgsqlConnectionStringBuilder(provider.GetRequiredService<IConfiguration>().GetConnectionString(DatabaseConnectionName))
                {
                    GssEncryptionMode = GssEncryptionMode.Disable,
                }.ConnectionString,
                ConfigureNpgsql));
        builder.EnrichNpgsqlDbContext<DocumentsDbContext>();

        builder.AddAzureBlobServiceClient(BlobsConnectionName);
        builder.Services.AddSingleton<IDocumentStore, BlobDocumentStore>();

        builder.Services.AddOptions<AiOptions>()
            .Bind(builder.Configuration.GetSection(AiOptions.SectionName))
            // The conventional variable name works too, so the key can come from any standard secret injection.
            .PostConfigure(options => options.ApiKey ??= builder.Configuration["ANTHROPIC_API_KEY"]);
        builder.Services.AddSingleton(provider => new AnthropicClient { ApiKey = provider.GetRequiredService<IOptions<AiOptions>>().Value.ApiKey });
        builder.Services.AddScoped<IClaimAnalyst, ClaudeClaimAnalyst>();

        builder.Services.AddAuthorizationBuilder()
            .AddPolicy(DocumentPermissions.ReadPolicy, policy => policy.RequireRole(DocumentPermissions.Readers))
            .AddPolicy(DocumentPermissions.WritePolicy, policy => policy.RequireRole(DocumentPermissions.Writers));
        builder.Services.Configure<RateLimiterOptions>(options => options.AddPolicy(
            DocumentPermissions.AnalysisRateLimit,
            context => RateLimitPartition.GetFixedWindowLimiter(
                // Only reached by authenticated callers (the endpoint requires authorization), so "sub" is present.
                context.User.FindFirst(ClaimsPrincipalExtensions.SubjectClaim)?.Value ?? "anonymous",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = 6, Window = TimeSpan.FromHours(1) })));

        return builder;
    }

    public static IEndpointRouteBuilder MapDocumentsModule(this IEndpointRouteBuilder api)
    {
        ArgumentNullException.ThrowIfNull(api);

        var documents = api.MapGroup("/claims/{claimId:guid}/documents")
            .WithTags("Documents")
            .RequireAuthorization(DocumentPermissions.ReadPolicy);
        ListDocuments.Map(documents);
        UploadDocument.Map(documents);
        DownloadDocument.Map(documents);

        var analysis = api.MapGroup("/claims/{claimId:guid}/analysis")
            .WithTags("AI assistant")
            .RequireAuthorization(DocumentPermissions.ReadPolicy);
        AnalyzeClaim.Map(analysis);

        GetDocumentSettings.Map(api.MapGroup("/documents").WithTags("Documents").RequireAuthorization(DocumentPermissions.ReadPolicy));
        return api;
    }

    /// <summary>Runs after the Claims module is initialized: demo documents attach to seeded claims.</summary>
    public static async Task InitializeDocumentsAsync(this IServiceProvider services, bool seed, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DocumentsDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);
        var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
        await store.EnsureCreatedAsync(cancellationToken);
        if (seed)
        {
            await DemoDocumentsSeeder.SeedAsync(
                dbContext, store, scope.ServiceProvider.GetRequiredService<IClaimDirectory>(), scope.ServiceProvider.GetRequiredService<TimeProvider>(), cancellationToken);
        }
    }

    internal static void ConfigureNpgsql(NpgsqlDbContextOptionsBuilder npgsql) =>
        npgsql.MigrationsHistoryTable("__ef_migrations_history", DocumentsDbContext.Schema);
}
