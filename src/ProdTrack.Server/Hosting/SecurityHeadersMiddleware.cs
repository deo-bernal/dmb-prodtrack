namespace ProdTrack.Server.Hosting;

/// <summary>Baseline security headers (docs/02 section 13).</summary>
internal sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    public Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;
        headers.XContentTypeOptions = "nosniff";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        headers["Permissions-Policy"] = context.Request.Path.StartsWithSegments("/floor")
            ? "camera=(self), microphone=(), geolocation=()"
            : "camera=(), microphone=(), geolocation=()";
        return next(context);
    }
}
