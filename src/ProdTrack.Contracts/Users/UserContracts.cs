namespace ProdTrack.Contracts.Users;

public sealed record UserSummaryDto(
    string Id,
    string Email,
    string DisplayName,
    string? EmployeeNumber,
    string? BadgeNumber,
    string? HomeStationCode,
    bool IsActive,
    bool IsLockedOut,
    bool MustChangePassword,
    IReadOnlyList<string> Roles);

/// <summary>User with the Identity concurrency stamp as <see cref="Version"/> (also the ETag).</summary>
public sealed record UserDetailDto(
    string Id,
    string Email,
    string DisplayName,
    string? EmployeeNumber,
    string? BadgeNumber,
    string? HomeStationCode,
    bool IsActive,
    bool IsLockedOut,
    bool MustChangePassword,
    IReadOnlyList<string> Roles,
    string Version);

public sealed record CreateUserRequest(
    string Email,
    string DisplayName,
    string TemporaryPassword,
    IReadOnlyList<string> Roles,
    string? EmployeeNumber,
    string? BadgeNumber,
    string? HomeStationCode);

public sealed record UpdateUserRequest(string DisplayName, IReadOnlyList<string> Roles, string? EmployeeNumber, string? BadgeNumber, string? HomeStationCode);

public sealed record SetUserActiveRequest(bool IsActive);

public sealed record ResetPasswordRequest(string TemporaryPassword);

public sealed record UserCreatedResponse(string Id);
