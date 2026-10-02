using System.Security.Claims;
using ClaimFlow.BuildingBlocks.Http;
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

internal static class GetClaim
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("/{id:guid}", HandleAsync)
            .WithName("GetClaim")
            .WithSummary("Get a claim with its history and the actions currently allowed")
            .Produces<ClaimDetailsResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> HandleAsync(Guid id, ClaimsPrincipal user, ClaimsDbContext dbContext, CancellationToken cancellationToken)
    {
        var claim = await dbContext.Claims.AsNoTracking()
            .SingleOrDefaultAsync(c => c.Id == new ClaimId(id), cancellationToken);

        return claim is null
            ? Result.Failure(ClaimErrors.NotFound).ToProblem()
            : TypedResults.Ok(ClaimDetailsResponse.From(claim, ClaimPermissions.AllowedActions(claim, user)));
    }
}
