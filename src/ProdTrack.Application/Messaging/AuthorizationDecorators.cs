using System.Reflection;
using ProdTrack.Application.Abstractions;
using ProdTrack.Domain.Common;

namespace ProdTrack.Application.Messaging;

internal static class AuthorizationGuard
{
    public static async Task<Error?> CheckAsync(Type requestType, IAuthorizationChecker checker, CancellationToken cancellationToken)
    {
        var attribute = requestType.GetCustomAttribute<RequiresPolicyAttribute>();
        if (attribute is null || await checker.IsAuthorizedAsync(attribute.Policy, cancellationToken))
        {
            return null;
        }

        return Error.Forbidden("Authorization.Forbidden", $"You do not have permission for this action (policy {attribute.Policy}).");
    }
}

/// <summary>Enforces <see cref="RequiresPolicyAttribute"/> for commands (same rules for REST and Blazor, docs/02 section 4.2).</summary>
internal sealed class AuthorizationCommandDecorator<TCommand, TResult>(
    ICommandHandler<TCommand, TResult> inner,
    IAuthorizationChecker checker) : ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    public async Task<Result<TResult>> HandleAsync(TCommand command, CancellationToken cancellationToken)
    {
        var error = await AuthorizationGuard.CheckAsync(typeof(TCommand), checker, cancellationToken);
        return error is null ? await inner.HandleAsync(command, cancellationToken) : Result<TResult>.Failure(error);
    }
}

/// <summary>Enforces <see cref="RequiresPolicyAttribute"/> for queries.</summary>
internal sealed class AuthorizationQueryDecorator<TQuery, TResult>(
    IQueryHandler<TQuery, TResult> inner,
    IAuthorizationChecker checker) : IQueryHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    public async Task<Result<TResult>> HandleAsync(TQuery query, CancellationToken cancellationToken)
    {
        var error = await AuthorizationGuard.CheckAsync(typeof(TQuery), checker, cancellationToken);
        return error is null ? await inner.HandleAsync(query, cancellationToken) : Result<TResult>.Failure(error);
    }
}
