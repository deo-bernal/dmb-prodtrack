using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using ProdTrack.Contracts;
using ProdTrack.Contracts.Common;

namespace ProdTrack.Server.Endpoints;

internal static class ApiEndpoints
{
    /// <summary>Maps /api/v1. Every endpoint requires authentication (fallback policy) plus its own policy.</summary>
    public static WebApplication MapProdTrackApi(this WebApplication app)
    {
        var api = app.MapGroup(ApiRoutes.Base)
            .AddEndpointFilter(ValidateAntiforgeryForCookieRequestsAsync);

        api.MapGet("/antiforgery", (HttpContext context, IAntiforgery antiforgery) =>
        {
            var tokens = antiforgery.GetAndStoreTokens(context);
            return TypedResults.Ok(new AntiforgeryTokenResponse(tokens.RequestToken!, tokens.HeaderName!));
        }).WithSummary("Antiforgery token for cookie-authenticated unsafe requests");

        api.MapMeEndpoints();
        api.MapStationEndpoints();
        api.MapReasonCodeEndpoints();
        api.MapReferenceEndpoints();
        api.MapProductEndpoints();
        api.MapRoutingEndpoints();
        api.MapWorkOrderEndpoints();
        api.MapDashboardEndpoints();

        if (app.Environment.IsEnvironment("Testing"))
        {
            // Test-only endpoint to verify the unhandled-exception ProblemDetails (PT-005).
            api.MapGet("/_test/throw", IResult () => throw new InvalidOperationException("Test exception"));
        }

        return app;
    }

    /// <summary>
    /// Browser clients authenticated by the Identity cookie must send X-XSRF-TOKEN on unsafe requests (docs/02 section 7.4).
    /// </summary>
    private static async ValueTask<object?> ValidateAntiforgeryForCookieRequestsAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var http = context.HttpContext;
        var method = http.Request.Method;
        var unsafeMethod = !(HttpMethods.IsGet(method) || HttpMethods.IsHead(method) || HttpMethods.IsOptions(method) || HttpMethods.IsTrace(method));
        if (unsafeMethod && string.Equals(http.User.Identity?.AuthenticationType, IdentityConstants.ApplicationScheme, StringComparison.Ordinal))
        {
            var antiforgery = http.RequestServices.GetRequiredService<IAntiforgery>();
            if (!await antiforgery.IsRequestValidAsync(http))
            {
                return TypedResults.Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Missing or invalid antiforgery token (X-XSRF-TOKEN).",
                    extensions: new Dictionary<string, object?> { ["code"] = "Antiforgery.Invalid" });
            }
        }

        return await next(context);
    }
}
