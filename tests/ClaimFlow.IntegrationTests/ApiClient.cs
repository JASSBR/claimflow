using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ClaimFlow.Claims.Contracts;
using ClaimFlow.Claims.Domain;

namespace ClaimFlow.IntegrationTests;

internal static class ApiClient
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public static DeclareClaimRequest ValidDeclaration(string policyNumber = "POL-555001", decimal amount = 1_500m) => new(
        policyNumber,
        ClaimType.Home,
        DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3)),
        "Water damage in the kitchen after a pipe burst.",
        amount);

    public static async Task<ClaimDetailsResponse> DeclareAsync(this HttpClient client, DeclareClaimRequest? request = null)
    {
        var response = await client.PostAsJsonAsync("/api/claims", request ?? ValidDeclaration(), Json);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ClaimDetailsResponse>(Json))!;
    }

    public static Task<HttpResponseMessage> ActAsync(this HttpClient client, ClaimDetailsResponse claim, ClaimAction action, string? reason = null, decimal? amount = null) =>
        client.PostAsJsonAsync($"/api/claims/{claim.Id}/actions", new ClaimActionRequest(action, claim.Version, reason, amount), Json);

    public static async Task<T> ReadAsync<T>(this HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<T>(Json))!;
}
