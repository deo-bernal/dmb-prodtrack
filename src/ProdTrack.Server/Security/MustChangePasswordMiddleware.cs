using ProdTrack.Infrastructure.Identity;
using ProdTrack.Server.Errors;

namespace ProdTrack.Server.Security;

/// <summary>Forces admin-created / bootstrap accounts to change the temporary password before using the app (PT-009).</summary>
internal sealed class MustChangePasswordMiddleware(RequestDelegate next)
{
    public const string ChangePasswordPath = "/account/change-password";

    public Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path;
        if (!context.User.HasClaim(AppClaimTypes.MustChangePassword, "true")
            || path.StartsWithSegments("/account")
            || path.StartsWithSegments("/_framework")
            || path.StartsWithSegments("/_content")
            || path.StartsWithSegments("/health")
            || Path.HasExtension(path.Value))
        {
            return next(context);
        }

        if (ApiPaths.IsApi(path))
        {
            return Results.Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "You must change your temporary password first.",
                extensions: new Dictionary<string, object?> { ["code"] = "Account.PasswordChangeRequired" })
                .ExecuteAsync(context);
        }

        context.Response.Redirect(ChangePasswordPath);
        return Task.CompletedTask;
    }
}
