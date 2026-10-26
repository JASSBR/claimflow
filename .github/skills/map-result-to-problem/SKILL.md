---
name: map-result-to-problem
description: "Convert a domain or application failure Result into an RFC 9457 HTTP ProblemDetails response."
---

> Evidence: value gate verified the facts below against this repository.

Convert a domain or application failure Result into an RFC 9457 HTTP ProblemDetails response.

Applies in: `claim-flow-building-blocks`.

## What is specific to this repository

- ToProblem throws an InvalidOperationException if invoked on a successful Result. [src/ClaimFlow.BuildingBlocks/Http/ResultExtensions.cs:L14-L14]
- When the first error type is Validation, ToProblem groups errors by code and returns a ValidationProblem titled 'One or more business rules were violated.'. [src/ClaimFlow.BuildingBlocks/Http/ResultExtensions.cs:L23-L23]
- For non-validation errors, ToProblem maps NotFound to 404, Forbidden to 403, Unavailable to 503, and any other type to 409 Conflict. [src/ClaimFlow.BuildingBlocks/Http/ResultExtensions.cs:L27-L33]

## Rules this change touches

- [Invariant] A failed Result instance must contain at least one error in its Errors collection when converted via ToProblem. (approved, conviction 1.00, src/ClaimFlow.BuildingBlocks/Http/ResultExtensions.cs:L17-L17)
- [Invariant] A Result converted via ToProblem must not be null and must not represent a successful outcome (result.IsSuccess must be false). Attempting to convert a successful result throws an InvalidOperationException. (approved, conviction 1.00, src/ClaimFlow.BuildingBlocks/Http/ResultExtensions.cs:L10-L15)
- [InformalDecision] `ResultExtensions.ToProblem` provides an extension method on `Result` that standardizes business failure mapping into RFC 9457 Problem Details responses across all HTTP endpoints in the API. (approved, conviction 1.00, src/ClaimFlow.BuildingBlocks/Http/ResultExtensions.cs:L6-L8)

## Start here

`src/ClaimFlow.BuildingBlocks/Http/ResultExtensions.cs` [src/ClaimFlow.BuildingBlocks/Http/ResultExtensions.cs:L6-L37] is the worked example for this change, as it stands in the repository:

```csharp
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
```

## Patterns

Required:
- `Result`
- `ToProblem`
- `TypedResults`

## Workflow

1. Verify the Result is a failure before invoking error mapping; never call ToProblem on a successful result. [src/ClaimFlow.BuildingBlocks/Http/ResultExtensions.cs:L9-L9]
2. Call result.ToProblem() to produce an RFC 9457 ProblemDetails IResult. [src/ClaimFlow.BuildingBlocks/Http/ResultExtensions.cs:L9-L9]
3. Allow ToProblem to map ErrorType.Validation to ValidationProblem or other error types to appropriate status codes. [src/ClaimFlow.BuildingBlocks/Http/ResultExtensions.cs:L9-L9]

## Checklist

- [ ] Verify that ToProblem is only invoked when result.IsSuccess is false. [src/ClaimFlow.BuildingBlocks/Http/ResultExtensions.cs:L9-L9]
- [ ] Ensure validation errors map to TypedResults.ValidationProblem grouped by code. [src/ClaimFlow.BuildingBlocks/Http/ResultExtensions.cs:L23-L23]
