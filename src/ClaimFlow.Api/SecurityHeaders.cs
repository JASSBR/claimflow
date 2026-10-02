namespace ClaimFlow.Api;

internal static class SecurityHeaders
{
    /// <summary>JSON-only API: nothing may be framed, sniffed or used as a document, so the policy can be maximally strict.</summary>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app) =>
        app.Use(async (context, next) =>
        {
            var headers = context.Response.Headers;
            headers.XContentTypeOptions = "nosniff";
            headers.XFrameOptions = "DENY";
            headers["Referrer-Policy"] = "no-referrer";
            // Scalar's API reference page (dev only) needs scripts and styles, so the lockdown skips it.
            if (!context.Request.Path.StartsWithSegments("/scalar", StringComparison.OrdinalIgnoreCase))
            {
                headers.ContentSecurityPolicy = "default-src 'none'; frame-ancestors 'none'";
            }

            await next(context);
        });
}
