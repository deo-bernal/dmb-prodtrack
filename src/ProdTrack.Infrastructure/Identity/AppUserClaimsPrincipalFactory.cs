using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ProdTrack.Infrastructure.Identity;

/// <summary>Adds display name, home station and the must-change-password flag to the cookie principal.</summary>
public sealed class AppUserClaimsPrincipalFactory(
    UserManager<AppUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<IdentityOptions> options)
    : UserClaimsPrincipalFactory<AppUser, IdentityRole>(userManager, roleManager, options)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(AppUser user)
    {
        ArgumentNullException.ThrowIfNull(user);
        var identity = await base.GenerateClaimsAsync(user);
        identity.AddClaim(new Claim(AppClaimTypes.DisplayName, string.IsNullOrWhiteSpace(user.DisplayName) ? user.UserName ?? string.Empty : user.DisplayName));
        if (!string.IsNullOrWhiteSpace(user.HomeStationCode))
        {
            identity.AddClaim(new Claim(AppClaimTypes.HomeStation, user.HomeStationCode));
        }

        if (user.MustChangePassword)
        {
            identity.AddClaim(new Claim(AppClaimTypes.MustChangePassword, "true"));
        }

        return identity;
    }
}
