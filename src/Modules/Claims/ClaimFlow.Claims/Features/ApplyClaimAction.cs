using System.Security.Claims;
using ClaimFlow.BuildingBlocks.Http;
using ClaimFlow.BuildingBlocks.Security;
using ClaimFlow.Claims.Contracts;
using ClaimFlow.Claims.Domain;
using ClaimFlow.Claims.Persistence;
using ClaimFlow.Claims.Security;
using ClaimFlow.SharedKernel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Claim = ClaimFlow.Claims.Domain.Claim;

namespace ClaimFlow.Claims.Features;

internal static class ApplyClaimAction
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("/{id:guid}/actions", HandleAsync)
            .RequireAuthorization(ClaimPermissions.WritePolicy)
            .WithName("ApplyClaimAction")
            .WithSummary("Move a claim through its workflow (review, request information, approve, reject, settle)")
            .Produces<ClaimDetailsResponse>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

    private static async Task<IResult> HandleAsync(
        Guid id,
        ClaimActionRequest request,
        ClaimsPrincipal user,
        ClaimPermissions permissions,
        ClaimsDbContext dbContext,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        if (!ClaimPermissions.CanPerform(user, request.Action))
        {
            return Result.Failure(Error.Forbidden("claim.action_forbidden", $"Your role does not allow '{request.Action}'.")).ToProblem();
        }

        var claim = await dbContext.Claims.SingleOrDefaultAsync(c => c.Id == new ClaimId(id), cancellationToken);
        if (claim is null)
        {
            return Result.Failure(ClaimErrors.NotFound).ToProblem();
        }

        // Compare against the version the client saw, not the one just loaded: that is what makes the check meaningful.
        dbContext.Entry(claim).Property(c => c.Version).OriginalValue = request.ExpectedVersion;

        var now = timeProvider.GetUtcNow();
        var actor = user.ToActor();
        var result = request.Action switch
        {
            ClaimAction.StartReview => claim.StartReview(actor, now),
            ClaimAction.RequestInformation => claim.RequestInformation(actor, request.Reason, now),
            ClaimAction.ResumeReview => claim.ResumeReview(actor, now),
            ClaimAction.Approve => claim.Approve(request.ApprovedAmount ?? 0, permissions.AuthorityOf(user), now),
            ClaimAction.Reject => claim.Reject(actor, request.Reason, now),
            ClaimAction.Settle => claim.Settle(actor, now),
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

        return TypedResults.Ok(ClaimDetailsResponse.From(claim, ClaimPermissions.AllowedActions(claim, user)));
    }
}
