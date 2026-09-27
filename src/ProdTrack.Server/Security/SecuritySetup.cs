using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using ProdTrack.Application.Abstractions;
using ProdTrack.Application.Security;
using ProdTrack.Infrastructure;
using ProdTrack.Server.Errors;

namespace ProdTrack.Server.Security;

internal static class SecuritySetup
{
    public const string TestOrCookieScheme = "TestOrCookie";

    /// <summary>ASP.NET Core Identity cookie auth, role policies with a deny-by-default fallback, antiforgery header for APIs.</summary>
    public static AuthMode AddProdTrackSecurity(this WebApplicationBuilder builder)
    {
        var mode = builder.Configuration.GetValue("Auth:Mode", AuthMode.Identity);
        var env = builder.Environment;
        if (mode != AuthMode.Identity && !env.IsDevelopment() && !env.IsEnvironment("Testing"))
        {
            throw new InvalidOperationException($"Auth:Mode={mode} is only allowed in the Development or Testing environment (current: {env.EnvironmentName}).");
        }

        var services = builder.Services;
        services.AddCascadingAuthenticationState();
        services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

        var authentication = services.AddAuthentication(options =>
        {
            options.DefaultScheme = mode == AuthMode.Test ? TestOrCookieScheme : IdentityConstants.ApplicationScheme;
            options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
        });
        authentication.AddIdentityCookies();
        if (mode == AuthMode.Test)
        {
            authentication.AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, null);
            authentication.AddPolicyScheme(TestOrCookieScheme, TestOrCookieScheme, o => o.ForwardDefaultSelector = context =>
                context.Request.Headers.ContainsKey(TestAuthHandler.UserHeader) ? TestAuthHandler.SchemeName : IdentityConstants.ApplicationScheme);
        }

        services.AddProdTrackIdentityStores();
        services.Configure<SecurityStampValidatorOptions>(o => o.ValidationInterval = TimeSpan.FromMinutes(30));
        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "__Host-ProdTrack";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.ExpireTimeSpan = TimeSpan.FromHours(12);
            options.SlidingExpiration = true;
            options.LoginPath = "/account/login";
            options.LogoutPath = "/account/logout";
            options.AccessDeniedPath = "/account/access-denied";
            options.Events.OnRedirectToLogin = context => ApiAwareRedirect(context, StatusCodes.Status401Unauthorized);
            options.Events.OnRedirectToAccessDenied = context => ApiAwareRedirect(context, StatusCodes.Status403Forbidden);
        });

        var authorization = services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
        foreach (var (policy, roles) in Policies.Definitions)
        {
            authorization.AddPolicy(policy, p => p.RequireAuthenticatedUser().RequireRole(roles));
        }

        services.AddAntiforgery(o => o.HeaderName = "X-XSRF-TOKEN");
        services.AddScoped<ClaimsPrincipalAccessor>();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<IAuthorizationChecker, AuthorizationChecker>();
        return mode;
    }

    /// <summary>API calls get 401/403 ProblemDetails instead of a redirect to the sign-in page.</summary>
    private static Task ApiAwareRedirect(RedirectContext<Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationOptions> context, int status)
    {
        if (!ApiPaths.IsApi(context.Request.Path))
        {
            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        }

        context.Response.StatusCode = status;
        return Results.Problem(
            statusCode: status,
            title: status == StatusCodes.Status401Unauthorized ? "Please sign in." : "You do not have permission for this action.")
            .ExecuteAsync(context.HttpContext);
    }
}
