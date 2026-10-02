using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ClaimFlow.BuildingBlocks.Outbox;

public static class OutboxServiceCollectionExtensions
{
    /// <summary>Registers the outbox for one module's DbContext. The DbContext must call ApplyOutbox() and add the interceptor.</summary>
    public static IServiceCollection AddOutbox<TDbContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        params Type[] eventTypes)
        where TDbContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddSingleton(new OutboxEventTypes(eventTypes));
        services.TryAddSingleton<OutboxEventRegistry>();
        services.TryAddSingleton<DomainEventsToOutboxInterceptor>();
        services.TryAddSingleton(TimeProvider.System);
        services.AddOptions<OutboxOptions>().Bind(configuration.GetSection(OutboxOptions.SectionName));

        services.AddSingleton<OutboxProcessor<TDbContext>>();
        services.AddHostedService(provider => provider.GetRequiredService<OutboxProcessor<TDbContext>>());
        return services;
    }
}
