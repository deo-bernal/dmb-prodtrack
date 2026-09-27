using System.Security.Claims;
using Serilog.Context;

namespace ProdTrack.Server.Logging;

/// <summary>Adds UserId to the log context after authentication (never personal data beyond the ID).</summary>
internal sealed class UserLogContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            await next(context);
            return;
        }

        using (LogContext.PushProperty("UserId", userId))
        {
            await next(context);
        }
    }
}
