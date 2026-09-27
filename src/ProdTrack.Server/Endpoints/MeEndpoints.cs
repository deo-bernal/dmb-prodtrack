using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using ProdTrack.Application.Security;
using ProdTrack.Contracts.Users;
using ProdTrack.Infrastructure.Identity;

namespace ProdTrack.Server.Endpoints;

internal static class MeEndpoints
{
    public static RouteGroupBuilder MapMeEndpoints(this RouteGroupBuilder api)
    {
        api.MapGet("/me", async (ClaimsPrincipal principal, UserManager<AppUser> users) =>
        {
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            var user = await users.FindByIdAsync(userId);
            return TypedResults.Ok(new MeResponse(
                userId,
                principal.Identity?.Name ?? userId,
                user?.DisplayName ?? principal.FindFirstValue(AppClaimTypes.DisplayName) ?? principal.Identity?.Name ?? userId,
                roles,
                user?.HomeStationCode));
        })
        .RequireAuthorization(Policies.ReadAll)
        .WithTags("Users")
        .WithSummary("Current user, roles and home station");
        return api;
    }
}
