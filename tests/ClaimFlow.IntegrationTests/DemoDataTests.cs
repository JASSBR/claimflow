using System.Net.Http.Json;
using ClaimFlow.Claims.Contracts;
using ClaimFlow.Documents;

namespace ClaimFlow.IntegrationTests;

[Collection(ApiTestGroup.Name)]
public sealed class DemoDataTests(ClaimFlowApiFactory factory)
{
    [Fact]
    public async Task Seed_CreatesClaimsInEveryState_WithGeneratedPdfEvidence()
    {
        using var sophie = await factory.ClientForAsync("sophie");

        var collision = (await sophie.GetFromJsonAsync<PagedResponse<ClaimSummaryResponse>>(
            "/api/claims?search=POL-104233&pageSize=50", ApiClient.Json, TestContext.Current.CancellationToken))!.Items
            .Single(claim => claim.ClaimedAmount == 2_350m);
        var documents = await sophie.GetFromJsonAsync<List<DocumentResponse>>(
            $"/api/claims/{collision.Id}/documents", ApiClient.Json, TestContext.Current.CancellationToken);

        documents.ShouldNotBeNull();
        documents.Select(d => d.FileName).ShouldBe(["Constat amiable.pdf", "Devis Carrosserie des Lilas.pdf", "Attestation d'assurance.pdf"], ignoreOrder: true);
        var pdf = await sophie.GetByteArrayAsync($"/api/claims/{collision.Id}/documents/{documents[0].Id}/content", TestContext.Current.CancellationToken);
        System.Text.Encoding.ASCII.GetString(pdf, 0, 5).ShouldBe("%PDF-");
        pdf.Length.ShouldBeGreaterThan(1_000);
    }
}
