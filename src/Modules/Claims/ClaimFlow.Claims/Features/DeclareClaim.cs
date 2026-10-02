using ClaimFlow.BuildingBlocks.Http;
using ClaimFlow.Claims.Contracts;
using ClaimFlow.Claims.Domain;
using ClaimFlow.Claims.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ClaimFlow.Claims.Features;

internal static class DeclareClaim
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("/", HandleAsync)
            .WithName("DeclareClaim")
            .WithSummary("Declare a new claim (first notice of loss)")
            .Produces<ClaimDetailsResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

    private static async Task<IResult> HandleAsync(
        DeclareClaimRequest request,
        ClaimsDbContext dbContext,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var data = new DeclareClaimData(request.PolicyNumber, request.Type, request.IncidentDate, request.Description, request.ClaimedAmount);
        var result = Claim.Declare(data, await dbContext.NextClaimNumberAsync(now, cancellationToken), now);
        if (result.IsFailure)
        {
            return result.ToProblem();
        }

        dbContext.Claims.Add(result.Value);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Created($"/api/claims/{result.Value.Id}", ClaimDetailsResponse.From(result.Value));
    }
}
