using System.Security.Claims;
using System.Security.Cryptography;
using ClaimFlow.BuildingBlocks.Http;
using ClaimFlow.BuildingBlocks.Security;
using ClaimFlow.Claims.Contracts;
using ClaimFlow.Documents.Domain;
using ClaimFlow.Documents.Persistence;
using ClaimFlow.Documents.Security;
using ClaimFlow.Documents.Storage;
using ClaimFlow.SharedKernel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace ClaimFlow.Documents.Features;

internal static class UploadDocument
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("/", HandleAsync)
            .RequireAuthorization(DocumentPermissions.WritePolicy)
            // Bearer-token API: browsers never attach the token automatically, so there is no CSRF vector to defend.
            .DisableAntiforgery()
            .WithMetadata(new RequestSizeLimitAttribute(ClaimDocument.MaxSizeBytes + (64 * 1024)))
            .WithName("UploadClaimDocument")
            .WithSummary("Attach a PDF, PNG or JPEG to a claim (format detected from content)")
            .Produces<DocumentResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

    private static async Task<IResult> HandleAsync(
        Guid claimId,
        IFormFile file,
        ClaimsPrincipal user,
        IClaimDirectory claims,
        DocumentsDbContext dbContext,
        IDocumentStore store,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        if (await claims.FindAsync(claimId, cancellationToken) is null)
        {
            return Result.Failure(DocumentErrors.ClaimNotFound).ToProblem();
        }

        // Reject oversized uploads from the declared length, before reading a single byte into memory.
        if (file.Length > ClaimDocument.MaxSizeBytes)
        {
            return Result.Failure(DocumentErrors.TooLarge).ToProblem();
        }

        var bytes = new byte[file.Length];
        await using (var stream = file.OpenReadStream())
        {
            await stream.ReadExactlyAsync(bytes, cancellationToken);
        }

        var existing = await dbContext.Documents.CountAsync(document => document.ClaimId == claimId, cancellationToken);
        var result = ClaimDocument.Upload(
            claimId,
            file.FileName,
            DocumentFormat.Detect(bytes.AsSpan(0, Math.Min(bytes.Length, DocumentFormat.SignatureLength))),
            bytes.LongLength,
            Convert.ToHexStringLower(SHA256.HashData(bytes)),
            existing,
            user.ToActor(),
            timeProvider.GetUtcNow());
        if (result.IsFailure)
        {
            return result.ToProblem();
        }

        // Bytes first, row second: a crash in between leaves an unreferenced blob (harmless, collectable),
        // never a row pointing to missing content.
        var document = result.Value;
        await store.SaveAsync(document.StorageKey, bytes, document.ContentType, cancellationToken);
        dbContext.Documents.Add(document);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Created($"/api/claims/{claimId}/documents/{document.Id}/content", document.ToResponse());
    }
}
