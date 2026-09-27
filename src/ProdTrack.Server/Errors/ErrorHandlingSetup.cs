using System.Diagnostics;
using ProdTrack.Server.Logging;

namespace ProdTrack.Server.Errors;

internal static class ErrorHandlingSetup
{
    public static IServiceCollection AddProdTrackErrorHandling(this IServiceCollection services)
    {
        services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
        {
            context.ProblemDetails.Extensions["traceId"] = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
            if (context.HttpContext.Items[CorrelationIdMiddleware.ItemKey] is string correlationId)
            {
                context.ProblemDetails.Extensions["correlationId"] = correlationId;
            }
        });
        services.AddExceptionHandler<GlobalExceptionHandler>();
        return services;
    }
}
