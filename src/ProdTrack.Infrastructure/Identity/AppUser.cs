using Microsoft.AspNetCore.Identity;

namespace ProdTrack.Infrastructure.Identity;

/// <summary>Identity user extended with plant profile fields (docs/02 section 7.1). Stored in schema sec.</summary>
public sealed class AppUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;

    public string? EmployeeNumber { get; set; }

    public string? BadgeNumber { get; set; }

    public string? HomeStationCode { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>Set for admin-created and bootstrap accounts; cleared after the user changes the password.</summary>
    public bool MustChangePassword { get; set; }
}
