namespace ProdTrack.Application.Abstractions;

/// <summary>The signed-in user. Application code never reads identity details from request bodies.</summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    string? UserId { get; }

    string? UserName { get; }

    IReadOnlyList<string> Roles { get; }

    bool IsInRole(string role);
}
