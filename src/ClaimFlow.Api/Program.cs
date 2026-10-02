using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using ClaimFlow.Api;
using ClaimFlow.Claims;
using Microsoft.AspNetCore.HttpOverrides;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<UnhandledExceptionHandler>();
builder.Services.AddOpenApi();
// Enums travel as names only: an integer like "type": 99 is rejected at binding instead of reaching the database.
builder.Services.AddSignalR()
    .AddJsonProtocol(options => options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false)));
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false)));
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // Behind a platform ingress (Azure Container Apps) the proxy address is not known in advance. Only enable this
    // where the app is reachable exclusively through that proxy, otherwise clients could spoof their IP.
    if (builder.Configuration.GetValue<bool>("ForwardedHeaders:TrustPlatformProxy"))
    {
        options.KnownIPNetworks.Clear();
        options.KnownProxies.Clear();
    }
});
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    // Per client IP: a public demo must survive a script hammering it without starving other visitors.
    options.AddPolicy(RateLimiting.ApiPolicy, context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = builder.Configuration.GetValue("RateLimiting:PermitPerMinute", 120),
            Window = TimeSpan.FromMinutes(1),
        }));
});

builder.AddClaimsModule();

var app = builder.Build();

// First: the rate limiter and logs must see the client's IP, not the ingress proxy's.
app.UseForwardedHeaders();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseSecurityHeaders();
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapDefaultEndpoints();
// Everything a client can call — REST and the SignalR hub — sits behind the per-IP limiter.
var limited = app.MapGroup(string.Empty).RequireRateLimiting(RateLimiting.ApiPolicy);
limited.MapGroup("/api").MapClaimsModule(limited);

if (app.Configuration.GetValue<bool>("Database:InitializeOnStartup"))
{
    await app.Services.InitializeClaimsDatabaseAsync(seed: app.Configuration.GetValue<bool>("Database:SeedDemoData"));
}

await app.RunAsync();
