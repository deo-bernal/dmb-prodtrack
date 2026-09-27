using ProdTrack.Domain.Common;

namespace ProdTrack.Application.Users;

/// <summary>
/// User administration over ASP.NET Core Identity (PT-012), implemented in Infrastructure with UserManager. Changes are
/// saved through the audited DbContext.
/// </summary>
public interface IUserAdministration
{
    Task<IReadOnlyList<UserSummaryModel>> ListAsync(CancellationToken cancellationToken);

    Task<UserDetailModel?> GetAsync(string id, CancellationToken cancellationToken);

    Task<Result<string>> CreateAsync(NewUser user, CancellationToken cancellationToken);

    /// <summary>Updates profile and roles. <paramref name="expectedVersion"/> is the Identity concurrency stamp (If-Match).</summary>
    Task<Result> UpdateAsync(string id, UserProfileUpdate update, string? expectedVersion, CancellationToken cancellationToken);

    /// <summary>Deactivates (sign-in refused, sessions end at the next security stamp check) or reactivates a user.</summary>
    Task<Result> SetActiveAsync(string id, bool isActive, CancellationToken cancellationToken);

    /// <summary>Sets a temporary password the user must change at the next sign-in; also clears a lockout.</summary>
    Task<Result> ResetPasswordAsync(string id, string temporaryPassword, CancellationToken cancellationToken);
}
