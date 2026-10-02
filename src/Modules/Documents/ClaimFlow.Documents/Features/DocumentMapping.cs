using ClaimFlow.Documents.Analysis;
using ClaimFlow.Documents.Domain;

namespace ClaimFlow.Documents.Features;

internal static class DocumentMapping
{
    public static DocumentResponse ToResponse(this ClaimDocument document) => new(
        document.Id.Value,
        document.FileName,
        document.ContentType,
        document.SizeBytes,
        document.Sha256,
        document.UploadedByName,
        document.UploadedAt);

    public static ClaimAnalysisResponse ToResponse(this ClaimAnalysis analysis) => new(
        analysis.Id,
        analysis.CreatedAt,
        analysis.RequestedByName,
        analysis.Model,
        analysis.Refused,
        analysis.Content,
        analysis.Citations,
        analysis.DocumentCount,
        analysis.InputTokens,
        analysis.OutputTokens);
}
