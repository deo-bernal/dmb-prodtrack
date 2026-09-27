using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProdTrack.Infrastructure.Identity;
using ProdTrack.Server.Security;

namespace ProdTrack.Server.Endpoints;

internal static class AccountEndpoints
{
    public static WebApplication MapAccountEndpoints(this WebApplication app, AuthMode authMode)
    {
        var account = app.MapGroup("/account");

        account.MapPost("/logout", async (SignInManager<AppUser> signInManager, [FromForm] string? returnUrl) =>
        {
            await signInManager.SignOutAsync();
            return TypedResults.LocalRedirect("~/account/login");
        });

        if (authMode == AuthMode.Dev)
        {
            // Development-only user picker (Auth:Mode=Dev is refused in other environments).
            account.MapPost("/dev-login", async (SignInManager<AppUser> signInManager, UserManager<AppUser> users, [FromForm] string email) =>
            {
                var user = await users.FindByEmailAsync(email);
                if (user is null || !DevUsers.IsDevUser(email))
                {
                    return Results.LocalRedirect("~/account/login");
                }

                await signInManager.SignInAsync(user, isPersistent: false);
                return Results.LocalRedirect("~/");
            }).AllowAnonymous();
        }

        return app;
    }
}
