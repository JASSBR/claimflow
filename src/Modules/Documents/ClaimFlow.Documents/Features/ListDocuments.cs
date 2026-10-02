using ClaimFlow.Documents.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace ClaimFlow.Documents.Features;

internal static class ListDocuments
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("/", HandleAsync)
            .WithName("ListClaimDocuments")
            .WithSummary("Documents attached to a claim, oldest first");

    private static async Task<Ok<List<DocumentResponse>>> HandleAsync(Guid claimId, DocumentsDbContext dbContext, CancellationToken cancellationToken)
    {
        var documents = await dbContext.Documents.AsNoTracking()
            .Where(document => document.ClaimId == claimId)
            .OrderBy(document => document.UploadedAt)
            .ToListAsync(cancellationToken);
        return TypedResults.Ok(documents.ConvertAll(document => document.ToResponse()));
    }
}
