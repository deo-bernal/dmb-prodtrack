namespace ProdTrack.Infrastructure.Persistence.Seeding;

/// <summary>First-run admin (Auth:BootstrapAdmin). Password only from server-only config or user-secrets.</summary>
public sealed class BootstrapAdminOptions
{
    public const string SectionName = "Auth:BootstrapAdmin";

    public string? Email { get; set; }

    public string? Password { get; set; }
}
