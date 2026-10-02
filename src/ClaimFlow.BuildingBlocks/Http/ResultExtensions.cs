using ClaimFlow.SharedKernel;
using Microsoft.AspNetCore.Http;

namespace ClaimFlow.BuildingBlocks.Http;

/// <summary>Maps business failures to RFC 9457 problem details. One mapping for the whole API, so every error looks the same.</summary>
public static class ResultExtensions
{
    public static IResult ToProblem(this Result result)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("A successful result cannot be converted to a problem.");
        }

        var first = result.Errors[0];
        if (first.Type == ErrorType.Validation)
        {
            var errors = result.Errors
                .GroupBy(error => error.Code, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.Select(error => error.Description).ToArray(), StringComparer.Ordinal);
            return TypedResults.ValidationProblem(errors, title: "One or more business rules were violated.");
        }

        return TypedResults.Problem(
            statusCode: first.Type switch
            {
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                ErrorType.Unavailable => StatusCodes.Status503ServiceUnavailable,
                _ => StatusCodes.Status409Conflict,
            },
            title: first.Description,
            extensions: new Dictionary<string, object?>(StringComparer.Ordinal) { ["code"] = first.Code });
    }
}
