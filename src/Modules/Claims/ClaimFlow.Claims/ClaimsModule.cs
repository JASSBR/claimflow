using ClaimFlow.BuildingBlocks.Outbox;
using ClaimFlow.Claims.Domain;
using ClaimFlow.Claims.Features;
using ClaimFlow.Claims.Persistence;
using ClaimFlow.Claims.Realtime;
using ClaimFlow.Claims.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace ClaimFlow.Claims;

/// <summary>The module's only public entry point: the host registers and maps it, and knows nothing else about it.</summary>
public static class ClaimsModule
{
    public const string ConnectionStringName = "claimsdb";

    public static IHostApplicationBuilder AddClaimsModule(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddOutbox<ClaimsDbContext>(builder.Configuration, typeof(ClaimDeclared), typeof(ClaimStatusChanged));
        builder.Services.AddDbContextPool<ClaimsDbContext>((provider, options) => options
            .UseNpgsql(BuildConnectionString(provider.GetRequiredService<IConfiguration>()), ConfigureNpgsql)
            .AddInterceptors(provider.GetRequiredService<DomainEventsToOutboxInterceptor>()));
        // Adds Aspire's retries, health check and OpenTelemetry instrumentation to the context registered above.
        builder.EnrichNpgsqlDbContext<ClaimsDbContext>();

        builder.Services.AddOptions<ClaimsOptions>().Bind(builder.Configuration.GetSection(ClaimsOptions.SectionName));
        builder.Services.AddSingleton<ClaimPermissions>();
        // The module declares its own policies: the host only knows that endpoints require *some* authorization.
        builder.Services.AddAuthorizationBuilder()
            .AddPolicy(ClaimPermissions.ReadPolicy, policy => policy.RequireRole(ClaimPermissions.ReaderRoles))
            .AddPolicy(ClaimPermissions.WritePolicy, policy => policy.RequireRole(ClaimPermissions.WriterRoles));

        builder.Services.AddScoped<IDomainEventHandler<ClaimDeclared>, ClaimChangedNotifier>();
        builder.Services.AddScoped<IDomainEventHandler<ClaimStatusChanged>, ClaimChangedNotifier>();
        return builder;
    }

    public static IEndpointRouteBuilder MapClaimsModule(this IEndpointRouteBuilder api, IEndpointRouteBuilder root)
    {
        ArgumentNullException.ThrowIfNull(api);
        ArgumentNullException.ThrowIfNull(root);

        var claims = api.MapGroup("/claims").WithTags("Claims").RequireAuthorization(ClaimPermissions.ReadPolicy);
        DeclareClaim.Map(claims);
        GetClaimCapabilities.Map(claims);
        ListClaims.Map(claims);
        GetClaimStats.Map(claims);
        GetClaim.Map(claims);
        ApplyClaimAction.Map(claims);

        root.MapHub<ClaimsHub>(ClaimsHub.Path).RequireAuthorization(ClaimPermissions.ReadPolicy);
        return api;
    }

    /// <summary>Applies pending migrations, then optionally seeds demo data. Production runs migrations as a pipeline step instead (ADR 0003).</summary>
    public static async Task InitializeClaimsDatabaseAsync(this IServiceProvider services, bool seed, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);
        if (seed)
        {
            await ClaimsSeeder.SeedAsync(dbContext, scope.ServiceProvider.GetRequiredService<TimeProvider>(), cancellationToken);
        }
    }

    // Kerberos is never used here, and the chiseled runtime image ships without libgssapi:
    // without this, Npgsql tries GSS encryption on every new connection and logs an error.
    private static string BuildConnectionString(IConfiguration configuration) =>
        new NpgsqlConnectionStringBuilder(configuration.GetConnectionString(ConnectionStringName))
        {
            GssEncryptionMode = GssEncryptionMode.Disable,
        }.ConnectionString;

    internal static void ConfigureNpgsql(NpgsqlDbContextOptionsBuilder npgsql) =>
        npgsql.MigrationsHistoryTable("__ef_migrations_history", ClaimsDbContext.Schema);
}
