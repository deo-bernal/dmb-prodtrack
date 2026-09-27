using System.Security.Claims;
using ProdTrack.Application.Abstractions;

namespace ProdTrack.Server.Security;

/// <summary><see cref="ICurrentUser"/> for both REST requests (HttpContext) and Blazor circuits.</summary>
internal sealed class CurrentUser(IHttpContextAccessor httpContextAccessor, ClaimsPrincipalAccessor circuitUser) : ICurrentUser
{
    public ClaimsPrincipal? Principal => circuitUser.Principal ?? httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public string? UserId => IsAuthenticated ? Principal!.FindFirstValue(ClaimTypes.NameIdentifier) : null;

    public string? UserName => IsAuthenticated ? Principal!.Identity!.Name : null;

    public IReadOnlyList<string> Roles =>
        Principal?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? [];

    public bool IsInRole(string role) => Principal?.IsInRole(role) == true;
}
