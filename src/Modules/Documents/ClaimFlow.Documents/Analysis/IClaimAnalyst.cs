using ClaimFlow.Claims.Contracts;

namespace ClaimFlow.Documents.Analysis;

/// <summary>Reviews a claim file against its evidence. Advisory only: it never changes the claim (ADR 0009).</summary>
internal interface IClaimAnalyst
{
    Task<AnalysisOutcome> AnalyzeAsync(ClaimSnapshot claim, IReadOnlyList<AnalysisDocument> documents, CancellationToken cancellationToken);
}
