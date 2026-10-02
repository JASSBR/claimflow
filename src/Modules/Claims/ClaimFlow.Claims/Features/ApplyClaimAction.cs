using ClaimFlow.BuildingBlocks.Http;
using ClaimFlow.Claims.Contracts;
using ClaimFlow.Claims.Domain;
using ClaimFlow.Claims.Persistence;
using ClaimFlow.SharedKernel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace ClaimFlow.Claims.Features;

internal static class ApplyClaimAction
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("/{id:guid}/actions", HandleAsync)
            .WithName("ApplyClaimAction")
            .WithSummary("Move a claim through its workflow (review, request information, approve, reject, settle)")
            .Produces<ClaimDetailsResponse>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

    private static async Task<IResult> HandleAsync(
        Guid id,
        ClaimActionRequest request,
        ClaimsDbContext dbContext,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var claim = await dbContext.Claims.SingleOrDefaultAsync(c => c.Id == new ClaimId(id), cancellationToken);
        if (claim is null)
        {
            return Result.Failure(ClaimErrors.NotFound).ToProblem();
        }

        // Compare against the version the client saw, not the one just loaded: that is what makes the check meaningful.
        dbContext.Entry(claim).Property(c => c.Version).OriginalValue = request.ExpectedVersion;

        var now = timeProvider.GetUtcNow();
        var result = request.Action switch
        {
            ClaimAction.StartReview => claim.StartReview(now),
            ClaimAction.RequestInformation => claim.RequestInformation(request.Reason, now),
            ClaimAction.ResumeReview => claim.ResumeReview(now),
            ClaimAction.Approve => claim.Approve(request.ApprovedAmount ?? 0, now),
            ClaimAction.Reject => claim.Reject(request.Reason, now),
            ClaimAction.Settle => claim.Settle(now),
            _ => Result.Failure(Error.Validation("claim.unknown_action", $"Unknown action '{request.Action}'.")),
        };
        if (result.IsFailure)
        {
            return result.ToProblem();
        }

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure(ClaimErrors.ConcurrentUpdate).ToProblem();
        }

        return TypedResults.Ok(ClaimDetailsResponse.From(claim));
    }
}
