// One command (`dotnet run --project src/ClaimFlow.AppHost` or `aspire run`) starts the whole system:
// PostgreSQL in a container, the API (migrated + seeded) and the Angular dev server, wired by service discovery,
// with logs, traces and metrics in the Aspire dashboard.
var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);
var database = postgres.AddDatabase("claimflow");

// Azurite locally; a real storage account when deployed.
var blobs = builder.AddAzureStorage("storage")
    .RunAsEmulator(emulator => emulator.WithDataVolume().WithLifetime(ContainerLifetime.Persistent))
    .AddBlobs("blobs");

// Generated once, persisted in user-secrets: the demo identity provider's signing key never lives in the repo.
var demoSigningKey = builder.AddParameter(
    "demo-signing-key",
    new GenerateParameterDefault { MinLength = 48, Special = false },
    secret: true,
    persist: true);

var api = builder.AddProject<Projects.ClaimFlow_Api>("api")
    .WithReference(database)
    .WaitFor(database)
    .WithReference(blobs)
    .WaitFor(blobs)
    .WithEnvironment("Auth__Mode", "Demo")
    .WithEnvironment("Auth__DemoSigningKey", demoSigningKey)
    .WithHttpHealthCheck("/health");

// The AI assistant is optional: without a key the API runs and the UI explains how to enable it.
if (builder.Configuration["ANTHROPIC_API_KEY"] is { Length: > 0 } anthropicApiKey)
{
    api.WithEnvironment("Ai__ApiKey", anthropicApiKey);
}

builder.AddJavaScriptApp("web", "../../web", "start")
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .WithReference(api)
    .WaitFor(api);

await builder.Build().RunAsync();
