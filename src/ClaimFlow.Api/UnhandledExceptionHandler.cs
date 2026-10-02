using Microsoft.AspNetCore.Diagnostics;

namespace ClaimFlow.Api;

/// <summary>
/// Last line of defence: logs the exception with its trace id and returns a generic problem.
/// Never echoes exception details to the client, whatever the environment.
/// </summary>
internal sealed partial class UnhandledExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<UnhandledExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // Malformed input (unparsable JSON, "type": 99…) is the client's fault. In Development, minimal APIs throw it
        // instead of answering 400 (RouteHandlerOptions.ThrowOnBadRequest), so it must not be reported as a 500.
        var (status, title) = exception is BadHttpRequestException badRequest
            ? (badRequest.StatusCode, "The request is malformed.")
            : (StatusCodes.Status500InternalServerError, "An unexpected error occurred.");

        if (status >= StatusCodes.Status500InternalServerError)
        {
            LogUnhandled(logger, exception, httpContext.Request.Method, httpContext.Request.Path);
        }
        else
        {
            LogBadRequest(logger, httpContext.Request.Method, httpContext.Request.Path, exception.Message);
        }

        httpContext.Response.StatusCode = status;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails =
            {
                Status = status,
                Title = title,
            },
        });
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception on {Method} {Path}")]
    private static partial void LogUnhandled(ILogger logger, Exception exception, string method, PathString path);

    [LoggerMessage(Level = LogLevel.Information, Message = "Rejected malformed request {Method} {Path}: {Reason}")]
    private static partial void LogBadRequest(ILogger logger, string method, PathString path, string reason);
}
