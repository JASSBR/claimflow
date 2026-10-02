// One command (`dotnet run --project src/ClaimFlow.AppHost` or `aspire run`) starts the whole system:
// PostgreSQL in a container, the API (migrated + seeded) and the Angular dev server, wired by service discovery,
// with logs, traces and metrics in the Aspire dashboard.
var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);
var claimsDb = postgres.AddDatabase("claimsdb");

var api = builder.AddProject<Projects.ClaimFlow_Api>("api")
    .WithReference(claimsDb)
    .WaitFor(claimsDb)
    .WithHttpHealthCheck("/health");

builder.AddJavaScriptApp("web", "../../web", "start")
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .WithReference(api)
    .WaitFor(api);

await builder.Build().RunAsync();
