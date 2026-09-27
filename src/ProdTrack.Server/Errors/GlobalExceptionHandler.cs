using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ProdTrack.Server.Errors;

/// <summary>
/// Unhandled exceptions on API paths become RFC 9457 ProblemDetails (500, or 409 for concurrency conflicts); stack
/// traces only in Development (PT-005). Other paths fall through to the /Error page.
/// </summary>
internal sealed partial class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    IHostEnvironment environment,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // The middleware has already switched Request.Path to the /Error page; decide on the original path.
        var originalPath = httpContext.Features.Get<IExceptionHandlerPathFeature>()?.Path ?? httpContext.Request.Path.Value;
        if (!ApiPaths.IsApi(new PathString(originalPath)))
        {
            return false;
        }

        var (status, title) = exception switch
        {
            DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "The record was changed by someone else. Reload and try again."),
            BadHttpRequestException bad => (bad.StatusCode, "Bad request."),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred."),
        };

        if (status >= 500)
        {
            LogUnhandled(logger, exception, originalPath ?? string.Empty);
        }

        httpContext.Response.StatusCode = status;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = environment.IsDevelopment() ? exception.ToString() : null,
                Type = $"https://httpstatuses.io/{status}",
            },
        });
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception for {Path}")]
    private static partial void LogUnhandled(ILogger logger, Exception exception, string path);
}
