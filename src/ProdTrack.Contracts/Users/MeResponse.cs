namespace ProdTrack.Contracts.Users;

/// <summary>The signed-in user (GET /api/v1/me).</summary>
public sealed record MeResponse(string UserId, string UserName, string DisplayName, IReadOnlyList<string> Roles, string? HomeStationCode);
