using System.Collections.Concurrent;
using ProdTrack.Application.Abstractions;
using ProdTrack.Application.Security;

namespace ProdTrack.TestSupport;

/// <summary>Mutable current user for tests.</summary>
public sealed class TestCurrentUser : ICurrentUser
{
    public string? UserId { get; set; } = "test-user";

    public string? UserName { get; set; } = "test.user";

    public List<string> RoleList { get; } = [Application.Security.Roles.Admin];

    public bool IsAuthenticated => UserId is not null;

    public IReadOnlyList<string> Roles => RoleList;

    public bool IsInRole(string role) => RoleList.Contains(role, StringComparer.Ordinal);

    public void SetRoles(params string[] roles)
    {
        RoleList.Clear();
        RoleList.AddRange(roles);
    }
}

/// <summary>Evaluates the real <see cref="Policies.Definitions"/> against the test user's roles.</summary>
public sealed class TestAuthorizationChecker(ICurrentUser user) : IAuthorizationChecker
{
    public Task<bool> IsAuthorizedAsync(string policy, CancellationToken cancellationToken) =>
        Task.FromResult(user.IsAuthenticated && Policies.Definitions[policy].Any(user.IsInRole));
}

public sealed class TestCorrelationIdAccessor : ICorrelationIdAccessor
{
    public string? CorrelationId { get; set; } = "test-correlation";
}

public sealed class InMemoryFileStorage : IFileStorage
{
    private readonly ConcurrentDictionary<string, byte[]> _files = new(StringComparer.Ordinal);

    public IReadOnlyCollection<string> Keys => [.. _files.Keys];

    public async Task SaveAsync(string key, Stream content, string contentType, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(content);
        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);
        _files[key] = buffer.ToArray();
    }

    public Task<Stream?> OpenReadAsync(string key, CancellationToken cancellationToken) =>
        Task.FromResult<Stream?>(_files.TryGetValue(key, out var bytes) ? new MemoryStream(bytes, writable: false) : null);

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken) => Task.FromResult(_files.ContainsKey(key));

    public Task<bool> DeleteAsync(string key, CancellationToken cancellationToken) => Task.FromResult(_files.TryRemove(key, out _));
}

public sealed class RecordingNotifier : INotifier
{
    public ConcurrentQueue<WorkOrderChangedNotification> Notifications { get; } = new();

    public Task WorkOrderChangedAsync(WorkOrderChangedNotification notification, CancellationToken cancellationToken)
    {
        Notifications.Enqueue(notification);
        return Task.CompletedTask;
    }

    public ConcurrentQueue<OperationChangedNotification> OperationNotifications { get; } = new();

    public Task OperationChangedAsync(OperationChangedNotification notification, CancellationToken cancellationToken)
    {
        OperationNotifications.Enqueue(notification);
        return Task.CompletedTask;
    }
}
