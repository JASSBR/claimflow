using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ClaimFlow.Claims.Contracts;
using ClaimFlow.Claims.Domain;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace ClaimFlow.IntegrationTests;

[Collection(ApiTestGroup.Name)]
public sealed class SecurityTests(ClaimFlowApiFactory factory)
{
    [Fact]
    public async Task AnonymousCaller_Gets401_OnApiAndHub()
    {
        using var anonymous = factory.CreateClient();

        (await anonymous.GetAsync("/api/claims", TestContext.Current.CancellationToken)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await anonymous.PostAsync("/hubs/claims/negotiate?negotiateVersion=1", null, TestContext.Current.CancellationToken)).StatusCode
            .ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TokenSignedWithAnotherKey_IsRejected()
    {
        var forged = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = "claimflow-demo",
            Audience = "claimflow-api",
            Claims = new Dictionary<string, object>(StringComparer.Ordinal) { ["sub"] = "mallory", ["role"] = ClaimsRoles.Manager },
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(new string('x', 64))), SecurityAlgorithms.HmacSha256),
        });
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", forged);

        (await client.GetAsync("/api/claims", TestContext.Current.CancellationToken)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DemoProvider_ListsPersonas_AndRejectsUnknownOnes()
    {
        using var anonymous = factory.CreateClient();

        var personas = await anonymous.GetFromJsonAsync<JsonElement>("/api/auth/personas", TestContext.Current.CancellationToken);
        personas.EnumerateArray().Select(p => p.GetProperty("id").GetString()).ShouldBe(["lea", "karim", "nadia", "sophie"]);

        var unknown = await anonymous.PostAsJsonAsync("/api/auth/token", new { personaId = "root" }, TestContext.Current.CancellationToken);
        unknown.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Auditor_ReadsEverything_ButCannotChangeAnything()
    {
        using var lea = await factory.ClientForAsync("lea");
        using var sophie = await factory.ClientForAsync("sophie");
        var claim = await lea.DeclareAsync();

        var read = await sophie.GetFromJsonAsync<ClaimDetailsResponse>($"/api/claims/{claim.Id}", ApiClient.Json, TestContext.Current.CancellationToken);
        read!.AllowedActions.ShouldBeEmpty();

        (await sophie.PostAsJsonAsync("/api/claims", ApiClient.ValidDeclaration(), ApiClient.Json, TestContext.Current.CancellationToken))
            .StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await sophie.ActAsync(claim, ClaimAction.StartReview)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Handler_CannotApproveAboveDelegation_ButManagerCan()
    {
        using var lea = await factory.ClientForAsync("lea");
        using var karim = await factory.ClientForAsync("karim");
        var claim = await lea.DeclareAsync(ApiClient.ValidDeclaration(amount: 40_000m));
        claim = await (await lea.ActAsync(claim, ClaimAction.StartReview)).ReadAsync<ClaimDetailsResponse>();

        var denied = await lea.ActAsync(claim, ClaimAction.Approve, amount: 40_000m);

        denied.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await denied.ReadAsync<JsonElement>()).GetProperty("code").GetString().ShouldBe("claim.approval_limit_exceeded");
        (await karim.ActAsync(claim, ClaimAction.Approve, amount: 40_000m)).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task FourEyes_ApproverCannotReleasePayment_SecondManagerCan()
    {
        using var lea = await factory.ClientForAsync("lea");
        using var karim = await factory.ClientForAsync("karim");
        using var nadia = await factory.ClientForAsync("nadia");
        var claim = await lea.DeclareAsync();
        claim = await (await lea.ActAsync(claim, ClaimAction.StartReview)).ReadAsync<ClaimDetailsResponse>();
        var approved = await (await karim.ActAsync(claim, ClaimAction.Approve, amount: 1_000m)).ReadAsync<ClaimDetailsResponse>();

        approved.AllowedActions.ShouldNotContain(ClaimAction.Settle);
        var selfSettle = await karim.ActAsync(approved, ClaimAction.Settle);
        selfSettle.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await selfSettle.ReadAsync<JsonElement>()).GetProperty("code").GetString().ShouldBe("claim.four_eyes_violation");
        (await lea.ActAsync(approved, ClaimAction.Settle)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        var seenByNadia = await nadia.GetFromJsonAsync<ClaimDetailsResponse>($"/api/claims/{claim.Id}", ApiClient.Json, TestContext.Current.CancellationToken);
        seenByNadia!.AllowedActions.ShouldBe([ClaimAction.Settle]);
        (await nadia.ActAsync(seenByNadia, ClaimAction.Settle)).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("lea", true, 10_000)]
    [InlineData("karim", true, 1_000_000)]
    [InlineData("sophie", false, 10_000)]
    public async Task Capabilities_ReflectRoleAndDelegation(string persona, bool canDeclare, decimal approvalLimit)
    {
        using var client = await factory.ClientForAsync(persona);

        var capabilities = await client.GetFromJsonAsync<ClaimCapabilitiesResponse>("/api/claims/capabilities", ApiClient.Json, TestContext.Current.CancellationToken);

        capabilities!.CanDeclare.ShouldBe(canDeclare);
        capabilities.ApprovalLimit.ShouldBe(approvalLimit);
    }

    [Fact]
    public async Task Cors_AllowsTheConfiguredSpaOrigin_Only()
    {
        using var client = factory.CreateClient();

        var allowed = await PreflightAsync(client, "https://claimflow.example");
        var foreign = await PreflightAsync(client, "https://evil.example");

        allowed.Headers.GetValues("Access-Control-Allow-Origin").ShouldBe(["https://claimflow.example"]);
        allowed.Headers.GetValues("Access-Control-Allow-Credentials").ShouldBe(["true"]);
        foreign.Headers.Contains("Access-Control-Allow-Origin").ShouldBeFalse();
    }

    private static async Task<HttpResponseMessage> PreflightAsync(HttpClient client, string origin)
    {
        using var request = new HttpRequestMessage(HttpMethod.Options, "/api/claims");
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", "POST");
        request.Headers.Add("Access-Control-Request-Headers", "authorization,content-type");
        return await client.SendAsync(request, TestContext.Current.CancellationToken);
    }
}
