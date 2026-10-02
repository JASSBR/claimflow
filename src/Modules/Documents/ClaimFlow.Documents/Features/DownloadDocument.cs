using ClaimFlow.BuildingBlocks.Http;
using ClaimFlow.Documents.Domain;
using ClaimFlow.Documents.Persistence;
using ClaimFlow.Documents.Storage;
using ClaimFlow.SharedKernel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;

namespace ClaimFlow.Documents.Features;

internal static class DownloadDocument
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("/{documentId:guid}/content", HandleAsync)
            .WithName("DownloadClaimDocument")
            .WithSummary("Stream a document's content (inline: PDFs and images only, format verified at upload)")
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> HandleAsync(
        Guid claimId,
        Guid documentId,
        HttpContext httpContext,
        DocumentsDbContext dbContext,
        IDocumentStore store,
        CancellationToken cancellationToken)
    {
        // Scoped by claim id too: a document id alone never grants access to another claim's file.
        var document = await dbContext.Documents.AsNoTracking()
            .SingleOrDefaultAsync(d => d.Id == new DocumentId(documentId) && d.ClaimId == claimId, cancellationToken);
        if (document is null)
        {
            return Result.Failure(DocumentErrors.NotFound).ToProblem();
        }

        var disposition = new ContentDispositionHeaderValue("inline");
        disposition.SetHttpFileName(document.FileName);
        httpContext.Response.Headers.ContentDisposition = disposition.ToString();
        httpContext.Response.Headers.CacheControl = "private, max-age=3600";

        return TypedResults.Stream(await store.OpenReadAsync(document.StorageKey, cancellationToken), document.ContentType);
    }
}
