using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ClaimFlow.Documents;

namespace ClaimFlow.IntegrationTests;

[Collection(ApiTestGroup.Name)]
public sealed class DocumentsTests(ClaimFlowApiFactory factory)
{
    private static readonly byte[] SmallPdf = Encoding.ASCII.GetBytes("%PDF-1.7\n1 0 obj << >> endobj\ntrailer << >>\n%%EOF");

    [Fact]
    public async Task Upload_StoresTheFile_AndServesItBackUnchanged()
    {
        using var lea = await factory.ClientForAsync("lea");
        var claim = await lea.DeclareAsync();

        var response = await UploadAsync(lea, claim.Id, SmallPdf, "../Devis garage.pdf", "application/octet-stream");

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var document = await response.ReadAsync<DocumentResponse>();
        document.FileName.ShouldBe("Devis garage.pdf");
        document.ContentType.ShouldBe("application/pdf");
        document.UploadedBy.ShouldBe("Léa Martin");

        var download = await lea.GetAsync(response.Headers.Location, TestContext.Current.CancellationToken);
        download.Content.Headers.ContentType!.MediaType.ShouldBe("application/pdf");
        (await download.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken)).ShouldBe(SmallPdf);

        var list = await lea.GetFromJsonAsync<List<DocumentResponse>>($"/api/claims/{claim.Id}/documents", ApiClient.Json, TestContext.Current.CancellationToken);
        list!.ShouldHaveSingleItem().Id.ShouldBe(document.Id);
    }

    [Fact]
    public async Task Upload_RejectsAnExecutableNamedPdf()
    {
        using var lea = await factory.ClientForAsync("lea");
        var claim = await lea.DeclareAsync();

        var response = await UploadAsync(lea, claim.Id, Encoding.ASCII.GetBytes("MZ\u0090\0 not a pdf"), "facture.pdf", "application/pdf");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await response.ReadAsync<JsonElement>()).GetProperty("errors").TryGetProperty("document.unsupported_format", out _).ShouldBeTrue();
    }

    [Fact]
    public async Task Upload_RejectsFilesAboveTheLimit()
    {
        using var lea = await factory.ClientForAsync("lea");
        var claim = await lea.DeclareAsync();
        var tooLarge = new byte[ClaimFlow.Documents.Domain.ClaimDocument.MaxSizeBytes + 1];
        SmallPdf.CopyTo(tooLarge, 0);

        (await UploadAsync(lea, claim.Id, tooLarge, "big.pdf", "application/pdf")).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Auditor_CanReadDocuments_ButNotUploadOrAnalyse()
    {
        using var lea = await factory.ClientForAsync("lea");
        using var sophie = await factory.ClientForAsync("sophie");
        var claim = await lea.DeclareAsync();
        (await UploadAsync(lea, claim.Id, SmallPdf, "constat.pdf", "application/pdf")).EnsureSuccessStatusCode();

        (await sophie.GetAsync($"/api/claims/{claim.Id}/documents", TestContext.Current.CancellationToken)).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await UploadAsync(sophie, claim.Id, SmallPdf, "x.pdf", "application/pdf")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await sophie.PostAsync($"/api/claims/{claim.Id}/analysis", null, TestContext.Current.CancellationToken)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DocumentId_FromAnotherClaim_IsNotFound()
    {
        using var lea = await factory.ClientForAsync("lea");
        var owner = await lea.DeclareAsync();
        var other = await lea.DeclareAsync();
        var document = await (await UploadAsync(lea, owner.Id, SmallPdf, "a.pdf", "application/pdf")).ReadAsync<DocumentResponse>();

        var response = await lea.GetAsync($"/api/claims/{other.Id}/documents/{document.Id}/content", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Analysis_IsStoredWithCitations_AndServedAsLatest()
    {
        using var lea = await factory.ClientForAsync("lea");
        var claim = await lea.DeclareAsync();
        (await lea.GetAsync($"/api/claims/{claim.Id}/analysis", TestContext.Current.CancellationToken)).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await lea.PostAsync($"/api/claims/{claim.Id}/analysis", null, TestContext.Current.CancellationToken)).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var document = await (await UploadAsync(lea, claim.Id, SmallPdf, "devis.pdf", "application/pdf")).ReadAsync<DocumentResponse>();

        var created = await lea.PostAsync($"/api/claims/{claim.Id}/analysis", null, TestContext.Current.CancellationToken);

        created.StatusCode.ShouldBe(HttpStatusCode.Created);
        var analysis = await created.ReadAsync<ClaimAnalysisResponse>();
        analysis.Content.ShouldContain(claim.Number);
        analysis.Citations.ShouldHaveSingleItem().DocumentId.ShouldBe(document.Id);
        analysis.RequestedBy.ShouldBe("Léa Martin");
        var latest = await lea.GetFromJsonAsync<ClaimAnalysisResponse>($"/api/claims/{claim.Id}/analysis", ApiClient.Json, TestContext.Current.CancellationToken);
        latest!.Id.ShouldBe(analysis.Id);
        latest.Citations.ShouldHaveSingleItem().CitedText.ShouldBe("Total TTC : 3 480,00 €");
    }

    [Fact]
    public async Task Analysis_IsRateLimitedPerUser()
    {
        using var karim = await factory.ClientForAsync("karim");
        var claim = await karim.DeclareAsync();
        (await UploadAsync(karim, claim.Id, SmallPdf, "devis.pdf", "application/pdf")).EnsureSuccessStatusCode();

        var statuses = new List<HttpStatusCode>();
        for (var i = 0; i < 7; i++)
        {
            statuses.Add((await karim.PostAsync($"/api/claims/{claim.Id}/analysis", null, TestContext.Current.CancellationToken)).StatusCode);
        }

        statuses.Count(status => status == HttpStatusCode.Created).ShouldBe(6);
        statuses[^1].ShouldBe(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task Analysis_Returns503_WhenNoApiKeyIsConfigured()
    {
        await using var withoutAi = factory.WithWebHostBuilder(builder => builder.UseSetting("Ai:ApiKey", string.Empty));
        using var client = withoutAi.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await factory.TokenForAsync("nadia"));

        var settings = await client.GetFromJsonAsync<DocumentSettingsResponse>("/api/documents/settings", ApiClient.Json, TestContext.Current.CancellationToken);
        var response = await client.PostAsync($"/api/claims/{Guid.NewGuid()}/analysis", null, TestContext.Current.CancellationToken);

        settings!.AiEnabled.ShouldBeFalse();
        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    private static Task<HttpResponseMessage> UploadAsync(HttpClient client, Guid claimId, byte[] content, string fileName, string contentType)
    {
        var form = new MultipartFormDataContent();
        var file = new ByteArrayContent(content);
        file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        form.Add(file, "file", fileName);
        return client.PostAsync($"/api/claims/{claimId}/documents", form, TestContext.Current.CancellationToken);
    }
}
