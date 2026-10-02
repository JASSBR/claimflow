using System.Security.Claims;
using ClaimFlow.BuildingBlocks.Http;
using ClaimFlow.BuildingBlocks.Security;
using ClaimFlow.Claims.Contracts;
using ClaimFlow.Documents.Analysis;
using ClaimFlow.Documents.Domain;
using ClaimFlow.Documents.Persistence;
using ClaimFlow.Documents.Security;
using ClaimFlow.Documents.Storage;
using ClaimFlow.SharedKernel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ClaimFlow.Documents.Features;

internal static class AnalyzeClaim
{
    private static readonly Error AiDisabled =
        Error.Unavailable("analysis.ai_disabled", "The AI assistant is not configured on this environment.");

    private static readonly Error NoDocuments =
        Error.Validation("analysis.no_documents", "Attach at least one document before asking for a review.");

    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/", HandleAsync)
            .RequireAuthorization(DocumentPermissions.WritePolicy)
            .RequireRateLimiting(DocumentPermissions.AnalysisRateLimit)
            .WithName("AnalyzeClaim")
            .WithSummary("Ask the AI assistant to cross-check the claim against its documents (advisory, cited)")
            .Produces<ClaimAnalysisResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status503ServiceUnavailable);

        group.MapGet("/", GetLatestAsync)
            .WithName("GetLatestClaimAnalysis")
            .WithSummary("Most recent AI review of the claim")
            .Produces<ClaimAnalysisResponse>()
            .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> HandleAsync(
        Guid claimId,
        ClaimsPrincipal user,
        IClaimDirectory claims,
        DocumentsDbContext dbContext,
        IDocumentStore store,
        IClaimAnalyst analyst,
        IOptions<AiOptions> options,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        if (!options.Value.Enabled)
        {
            return Result.Failure(AiDisabled).ToProblem();
        }

        var claim = await claims.FindAsync(claimId, cancellationToken);
        if (claim is null)
        {
            return Result.Failure(DocumentErrors.ClaimNotFound).ToProblem();
        }

        var documents = await LoadDocumentsAsync(claimId, dbContext, store, options.Value.MaxTotalBytes, cancellationToken);
        if (documents.Count == 0)
        {
            return Result.Failure(NoDocuments).ToProblem();
        }

        var outcome = await analyst.AnalyzeAsync(claim, documents, cancellationToken);
        var actor = user.ToActor();
        var analysis = new ClaimAnalysis
        {
            ClaimId = claimId,
            CreatedAt = timeProvider.GetUtcNow(),
            RequestedById = actor.Id,
            RequestedByName = actor.Name,
            Model = outcome.Model,
            Refused = outcome.Refused,
            Content = outcome.Content,
            Citations = [.. outcome.Citations],
            DocumentCount = documents.Count,
            InputTokens = outcome.InputTokens,
            OutputTokens = outcome.OutputTokens,
        };
        dbContext.Analyses.Add(analysis);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Created($"/api/claims/{claimId}/analysis", analysis.ToResponse());
    }

    private static async Task<IResult> GetLatestAsync(Guid claimId, DocumentsDbContext dbContext, CancellationToken cancellationToken)
    {
        var latest = await dbContext.Analyses.AsNoTracking()
            .Where(analysis => analysis.ClaimId == claimId)
            .OrderByDescending(analysis => analysis.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        return latest is null ? TypedResults.NoContent() : TypedResults.Ok(latest.ToResponse());
    }

    /// <summary>Most recent documents first, until the request budget is spent; then back to upload order for reading.</summary>
    private static async Task<List<AnalysisDocument>> LoadDocumentsAsync(
        Guid claimId,
        DocumentsDbContext dbContext,
        IDocumentStore store,
        long maxTotalBytes,
        CancellationToken cancellationToken)
    {
        var candidates = await dbContext.Documents.AsNoTracking()
            .Where(document => document.ClaimId == claimId)
            .OrderByDescending(document => document.UploadedAt)
            .ToListAsync(cancellationToken);

        var selected = new List<ClaimDocument>();
        long total = 0;
        foreach (var document in candidates)
        {
            if (total + document.SizeBytes > maxTotalBytes)
            {
                continue;
            }

            total += document.SizeBytes;
            selected.Add(document);
        }

        var documents = new List<AnalysisDocument>(selected.Count);
        foreach (var document in selected.OrderBy(document => document.UploadedAt))
        {
            documents.Add(new AnalysisDocument(
                document.Id.Value, document.FileName, document.ContentType, await store.ReadAllAsync(document.StorageKey, cancellationToken)));
        }

        return documents;
    }
}
