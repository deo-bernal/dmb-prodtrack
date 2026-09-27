using System.Security.Claims;

namespace ProdTrack.Server.Security;

/// <summary>Holds the circuit user for Blazor Server scopes (HttpContext is not available in circuits).</summary>
public sealed class ClaimsPrincipalAccessor
{
    public ClaimsPrincipal? Principal { get; set; }
}
