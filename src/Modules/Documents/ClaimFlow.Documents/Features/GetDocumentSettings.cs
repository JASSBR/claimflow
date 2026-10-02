using ClaimFlow.Documents.Analysis;
using ClaimFlow.Documents.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace ClaimFlow.Documents.Features;

internal static class GetDocumentSettings
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("/settings", Handle)
            .WithName("GetDocumentSettings")
            .WithSummary("Upload limits and whether the AI assistant is available");

    private static Ok<DocumentSettingsResponse> Handle(IOptions<AiOptions> options) =>
        TypedResults.Ok(new DocumentSettingsResponse(
            options.Value.Enabled,
            options.Value.Enabled ? options.Value.Model : null,
            ClaimDocument.MaxSizeBytes,
            ClaimDocument.MaxDocumentsPerClaim,
            [DocumentFormat.Pdf.ContentType, DocumentFormat.Png.ContentType, DocumentFormat.Jpeg.ContentType]));
}
