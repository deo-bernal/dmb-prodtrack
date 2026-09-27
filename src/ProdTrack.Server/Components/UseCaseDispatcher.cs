using ProdTrack.Application.Messaging;
using ProdTrack.Domain.Common;
using ProdTrack.Server.Security;

namespace ProdTrack.Server.Components;

/// <summary>
/// Blazor Server entry point to the Application layer (docs/09 section 4): runs each use case in its own DI scope
/// (fresh DbContext) with the circuit user, so authorization and audit work exactly like the REST path.
/// </summary>
public sealed class UseCaseDispatcher(IServiceScopeFactory scopeFactory, ClaimsPrincipalAccessor circuitUser, IHttpContextAccessor httpContextAccessor)
{
    public async Task<Result<TResult>> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
    {
        await using var scope = CreateScope();
        return await scope.ServiceProvider.GetRequiredService<IDispatcher>().SendAsync(command, cancellationToken);
    }

    public async Task<Result<TResult>> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        await using var scope = CreateScope();
        return await scope.ServiceProvider.GetRequiredService<IDispatcher>().QueryAsync(query, cancellationToken);
    }

    private AsyncServiceScope CreateScope()
    {
        var scope = scopeFactory.CreateAsyncScope();
        scope.ServiceProvider.GetRequiredService<ClaimsPrincipalAccessor>().Principal =
            circuitUser.Principal ?? httpContextAccessor.HttpContext?.User;
        return scope;
    }
}
