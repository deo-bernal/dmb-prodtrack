using Microsoft.EntityFrameworkCore;
using ProdTrack.Domain.Common;

namespace ProdTrack.Application.Messaging;

/// <summary>
/// Translates an optimistic concurrency failure during SaveChanges (another request saved the same row first) into a
/// 409 <see cref="ConcurrencyErrors.Conflict"/> result, for the REST API and Blazor alike.
/// </summary>
internal sealed class ConcurrencyCommandDecorator<TCommand, TResult>(ICommandHandler<TCommand, TResult> inner)
    : ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    public async Task<Result<TResult>> HandleAsync(TCommand command, CancellationToken cancellationToken)
    {
        try
        {
            return await inner.HandleAsync(command, cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<TResult>.Failure(ConcurrencyErrors.Conflict);
        }
    }
}
