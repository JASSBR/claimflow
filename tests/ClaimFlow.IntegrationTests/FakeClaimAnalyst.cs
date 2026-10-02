using ClaimFlow.Claims.Contracts;
using ClaimFlow.Documents;
using ClaimFlow.Documents.Analysis;

namespace ClaimFlow.IntegrationTests;

/// <summary>Deterministic stand-in for Claude: echoes what it received so tests can assert on the inputs.</summary>
internal sealed class FakeClaimAnalyst : IClaimAnalyst
{
    public Task<AnalysisOutcome> AnalyzeAsync(ClaimSnapshot claim, IReadOnlyList<AnalysisDocument> documents, CancellationToken cancellationToken)
    {
        var first = documents[0];
        var citation = new AnalysisCitation(1, first.DocumentId, first.Title, "Total TTC : 3 480,00 €", 1, 1);
        var content = $"## Synthèse\n- Dossier {claim.Number}, {documents.Count} pièce(s)[[1]]";
        return Task.FromResult(new AnalysisOutcome(false, content, [citation], "fake-model", 1_200, 300));
    }
}
