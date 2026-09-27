using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using ProdTrack.Application.Abstractions;

namespace ProdTrack.Server.Security;

/// <summary>Evaluates Application-level policies with ASP.NET Core authorization (same rules for REST and Blazor).</summary>
internal sealed class AuthorizationChecker(IAuthorizationService authorizationService, ICurrentUser currentUser) : IAuthorizationChecker
{
    public async Task<bool> IsAuthorizedAsync(string policy, CancellationToken cancellationToken)
    {
        var principal = (currentUser as CurrentUser)?.Principal ?? new ClaimsPrincipal(new ClaimsIdentity());
        var result = await authorizationService.AuthorizeAsync(principal, policy);
        return result.Succeeded;
    }
}
