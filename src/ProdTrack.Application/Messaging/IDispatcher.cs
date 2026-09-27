using ProdTrack.Domain.Common;

namespace ProdTrack.Application.Messaging;

/// <summary>Resolves and invokes the handler (with its decorators) for a command or query.</summary>
public interface IDispatcher
{
    Task<Result<TResult>> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default);

    Task<Result<TResult>> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);
}
