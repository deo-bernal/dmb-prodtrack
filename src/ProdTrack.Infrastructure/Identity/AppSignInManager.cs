using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ProdTrack.Infrastructure.Identity;

/// <summary>Refuses sign-in for deactivated users (PT-012) on top of the standard Identity checks.</summary>
public sealed class AppSignInManager(
    UserManager<AppUser> userManager,
    IHttpContextAccessor contextAccessor,
    IUserClaimsPrincipalFactory<AppUser> claimsFactory,
    IOptions<IdentityOptions> optionsAccessor,
    ILogger<SignInManager<AppUser>> logger,
    IAuthenticationSchemeProvider schemes,
    IUserConfirmation<AppUser> confirmation)
    : SignInManager<AppUser>(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
{
    public override async Task<bool> CanSignInAsync(AppUser user)
    {
        ArgumentNullException.ThrowIfNull(user);
        if (!user.IsActive)
        {
            Logger.LogWarning("Sign-in refused for deactivated user {UserId}", user.Id);
            return false;
        }

        return await base.CanSignInAsync(user);
    }
}
