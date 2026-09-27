using ProdTrack.Application.Security;

namespace ProdTrack.Infrastructure.Identity;

/// <summary>Passwordless picker users seeded only when Auth:Mode=Dev (Development environment).</summary>
public static class DevUsers
{
    public const string EmailDomain = "dev.prodtrack.local";

    public static IReadOnlyList<string> Emails { get; } = [.. Roles.All.Select(role => $"{role.ToLowerInvariant()}@{EmailDomain}")];

    public static bool IsDevUser(string? email) =>
        email is not null && email.EndsWith("@" + EmailDomain, StringComparison.OrdinalIgnoreCase);
}
