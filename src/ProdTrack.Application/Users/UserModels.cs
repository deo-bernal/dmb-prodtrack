namespace ProdTrack.Application.Users;

public sealed record UserSummaryModel(
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

public sealed record UserDetailModel(
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

public sealed record NewUser(
    string Email,
    string DisplayName,
    string TemporaryPassword,
    IReadOnlyList<string> Roles,
    string? EmployeeNumber,
    string? BadgeNumber,
    string? HomeStationCode);

public sealed record UserProfileUpdate(
    string DisplayName,
    IReadOnlyList<string> Roles,
    string? EmployeeNumber,
    string? BadgeNumber,
    string? HomeStationCode);

public sealed record UserCreatedModel(string Id);
