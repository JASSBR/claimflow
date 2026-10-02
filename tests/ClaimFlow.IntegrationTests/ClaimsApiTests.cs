using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ClaimFlow.Claims.Contracts;
using ClaimFlow.Claims.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace ClaimFlow.IntegrationTests;

[Collection(ApiTestGroup.Name)]
public sealed class ClaimsApiTests(ClaimFlowApiFactory factory) : IAsyncLifetime
{
    private HttpClient _client = null!;

    public async ValueTask InitializeAsync() => _client = await factory.ClientForAsync("lea");

    public ValueTask DisposeAsync()
    {
        _client.Dispose();
        return ValueTask.CompletedTask;
    }

    [Fact]
    public async Task Declare_Returns201_WithLocationOfReadableClaim()
    {
        var response = await _client.PostAsJsonAsync("/api/claims", ApiClient.ValidDeclaration(), ApiClient.Json, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var created = await response.ReadAsync<ClaimDetailsResponse>();
        created.Status.ShouldBe(ClaimStatus.Declared);
        created.AllowedActions.ShouldBe([ClaimAction.StartReview]);
        created.DeclaredBy.ShouldBe("Léa Martin");
        created.Version.ShouldBeGreaterThan(0u);

        var fetched = await _client.GetFromJsonAsync<ClaimDetailsResponse>(response.Headers.Location, ApiClient.Json, TestContext.Current.CancellationToken);
        fetched!.Number.ShouldBe(created.Number);
    }

    [Fact]
    public async Task Declare_AllocatesSequentialHumanReadableNumbers()
    {
        var first = await _client.DeclareAsync();
        var second = await _client.DeclareAsync();

        first.Number.ShouldMatch(@"^SIN-\d{4}-\d{6}$");
        int.Parse(second.Number[^6..], CultureInfo.InvariantCulture).ShouldBeGreaterThan(int.Parse(first.Number[^6..], CultureInfo.InvariantCulture));
    }

    [Fact]
    public async Task MalformedBody_Returns400_EvenWhenBindingThrows()
    {
        // Development makes minimal APIs throw BadHttpRequestException instead of answering 400 directly.
        await using var throwingApp = factory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            services.Configure<Microsoft.AspNetCore.Routing.RouteHandlerOptions>(options => options.ThrowOnBadRequest = true)));
        using var client = throwingApp.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", await factory.TokenForAsync("lea"));
        using var body = new StringContent("{ not json", System.Text.Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/claims", body, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await response.ReadAsync<JsonElement>()).GetProperty("title").GetString().ShouldBe("The request is malformed.");
    }

    [Fact]
    public async Task Declare_Rejects_NumericEnumValue()
    {
        using var body = new StringContent(
            """{"policyNumber":"POL-555001","type":99,"incidentDate":"2026-09-30","description":"Pipe burst in the kitchen.","claimedAmount":100}""",
            System.Text.Encoding.UTF8,
            "application/json");

        var response = await _client.PostAsync("/api/claims", body, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task List_HandlesAbsurdPageNumber_WithoutServerError()
    {
        var response = await _client.GetAsync($"/api/claims?page={int.MaxValue}&pageSize=100", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await response.ReadAsync<PagedResponse<ClaimSummaryResponse>>()).Items.ShouldBeEmpty();
    }

    [Fact]
    public async Task Declare_Returns400ProblemDetails_ListingEveryViolatedRule()
    {
        var invalid = new DeclareClaimRequest("nope", ClaimType.Auto, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), "short", -1m);

        var response = await _client.PostAsJsonAsync("/api/claims", invalid, ApiClient.Json, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType!.MediaType.ShouldBe("application/problem+json");
        var problem = await response.ReadAsync<ValidationProblemDetails>();
        problem.Errors.Keys.ShouldBe(
            ["claim.policy_number_invalid", "claim.incident_date_in_future", "claim.description_length", "claim.claimed_amount_out_of_range"],
            ignoreOrder: true);
    }

    [Fact]
    public async Task Workflow_ReviewApproveSettle_EndsSettledWithHistory()
    {
        var claim = await _client.DeclareAsync();

        claim = await (await _client.ActAsync(claim, ClaimAction.StartReview)).ReadAsync<ClaimDetailsResponse>();
        claim = await (await _client.ActAsync(claim, ClaimAction.Approve, amount: 1_200m)).ReadAsync<ClaimDetailsResponse>();
        using var nadia = await factory.ClientForAsync("nadia");
        var response = await nadia.ActAsync(claim, ClaimAction.Settle);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var settled = await response.ReadAsync<ClaimDetailsResponse>();
        settled.Status.ShouldBe(ClaimStatus.Settled);
        settled.ApprovedAmount.ShouldBe(1_200m);
        settled.History.Select(h => h.Action).ShouldBe([ClaimAction.StartReview, ClaimAction.Approve, ClaimAction.Settle]);
        settled.History.Select(h => h.ActorName).ShouldBe(["Léa Martin", "Léa Martin", "Nadia Haddad"]);
        settled.AllowedActions.ShouldBeEmpty();
    }

    [Fact]
    public async Task ForbiddenTransition_Returns409_WithMachineReadableCode()
    {
        var claim = await _client.DeclareAsync();

        // A manager is allowed to settle in general; the workflow state is what forbids it here.
        using var karim = await factory.ClientForAsync("karim");
        var response = await karim.ActAsync(claim, ClaimAction.Settle);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        var problem = await response.ReadAsync<JsonElement>();
        problem.GetProperty("code").GetString().ShouldBe("claim.transition_not_allowed");
    }

    [Fact]
    public async Task StaleVersion_Returns409_AndDoesNotOverwriteConcurrentDecision()
    {
        var seenByBoth = await _client.DeclareAsync();
        var first = await _client.ActAsync(seenByBoth, ClaimAction.StartReview);
        first.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Second user still holds the original version; their (otherwise valid) request must be refused.
        var reviewed = await first.ReadAsync<ClaimDetailsResponse>();
        var stale = await _client.PostAsJsonAsync(
            $"/api/claims/{seenByBoth.Id}/actions",
            new ClaimActionRequest(ClaimAction.Reject, seenByBoth.Version, "Out of scope", null),
            ApiClient.Json,
            TestContext.Current.CancellationToken);

        stale.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await stale.ReadAsync<JsonElement>()).GetProperty("code").GetString().ShouldBe("claim.concurrent_update");
        var current = await _client.GetFromJsonAsync<ClaimDetailsResponse>($"/api/claims/{seenByBoth.Id}", ApiClient.Json, TestContext.Current.CancellationToken);
        current!.Status.ShouldBe(ClaimStatus.UnderReview);
        current.Version.ShouldBe(reviewed.Version);
    }

    [Fact]
    public async Task UnknownClaim_Returns404ProblemDetails()
    {
        var response = await _client.GetAsync($"/api/claims/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await response.ReadAsync<JsonElement>()).GetProperty("code").GetString().ShouldBe("claim.not_found");
    }

    [Fact]
    public async Task List_FiltersByStatusAndSearch()
    {
        var policy = $"POL-{Random.Shared.Next(100_000, 999_999)}";
        var declared = await _client.DeclareAsync(ApiClient.ValidDeclaration(policy));
        var reviewed = await _client.DeclareAsync(ApiClient.ValidDeclaration(policy));
        (await _client.ActAsync(reviewed, ClaimAction.StartReview)).EnsureSuccessStatusCode();

        var page = await _client.GetFromJsonAsync<PagedResponse<ClaimSummaryResponse>>(
            $"/api/claims?search={policy.ToLowerInvariant()}&status=UnderReview", ApiClient.Json, TestContext.Current.CancellationToken);

        page!.TotalCount.ShouldBe(1);
        page.Items.ShouldHaveSingleItem().Id.ShouldBe(reviewed.Id);
        page.Items.ShouldNotContain(item => item.Id == declared.Id);
    }

    [Fact]
    public async Task Stats_ReturnEveryStatusKey()
    {
        await _client.DeclareAsync();

        var stats = await _client.GetFromJsonAsync<ClaimStatsResponse>("/api/claims/stats", ApiClient.Json, TestContext.Current.CancellationToken);

        stats!.CountByStatus.Keys.ShouldBe(Enum.GetValues<ClaimStatus>(), ignoreOrder: true);
        stats.CountByStatus[ClaimStatus.Declared].ShouldBeGreaterThan(0);
        stats.TotalClaimedAmount.ShouldBeGreaterThan(0m);
    }

    [Fact]
    public async Task Responses_CarrySecurityHeaders()
    {
        var response = await _client.GetAsync("/api/claims", TestContext.Current.CancellationToken);

        response.Headers.GetValues("X-Content-Type-Options").ShouldBe(["nosniff"]);
        response.Headers.GetValues("X-Frame-Options").ShouldBe(["DENY"]);
        response.Headers.GetValues("Content-Security-Policy").Single().ShouldContain("default-src 'none'");
    }

    [Fact]
    public async Task HealthEndpoint_ReportsHealthy_IncludingDatabase()
    {
        var response = await _client.GetAsync("/health", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)).ShouldBe("Healthy");
    }
}
