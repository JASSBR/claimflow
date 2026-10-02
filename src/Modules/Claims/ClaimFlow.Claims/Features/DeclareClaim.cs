using System.Security.Claims;
using ClaimFlow.BuildingBlocks.Http;
using ClaimFlow.BuildingBlocks.Security;
using ClaimFlow.Claims.Contracts;
using ClaimFlow.Claims.Domain;
using ClaimFlow.Claims.Persistence;
using ClaimFlow.Claims.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Claim = ClaimFlow.Claims.Domain.Claim;

namespace ClaimFlow.Claims.Features;

internal static class DeclareClaim
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("/", HandleAsync)
            .RequireAuthorization(ClaimPermissions.WritePolicy)
            .WithName("DeclareClaim")
            .WithSummary("Declare a new claim (first notice of loss)")
            .Produces<ClaimDetailsResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

    private static async Task<IResult> HandleAsync(
        DeclareClaimRequest request,
        ClaimsPrincipal user,
        ClaimsDbContext dbContext,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var data = new DeclareClaimData(request.PolicyNumber, request.Type, request.IncidentDate, request.Description, request.ClaimedAmount);
        var result = Claim.Declare(data, await dbContext.NextClaimNumberAsync(now, cancellationToken), user.ToActor(), now);
        if (result.IsFailure)
        {
            return result.ToProblem();
        }

        dbContext.Claims.Add(result.Value);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Created(
            $"/api/claims/{result.Value.Id}",
            ClaimDetailsResponse.From(result.Value, ClaimPermissions.AllowedActions(result.Value, user)));
    }
}
