using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using ProdTrack.Domain.Common;

namespace ProdTrack.Application.Messaging;

/// <summary>Small in-house dispatcher (no MediatR, ADR-0001).</summary>
internal sealed class Dispatcher(IServiceProvider serviceProvider) : IDispatcher
{
    private static readonly ConcurrentDictionary<(Type Request, Type Result), object> Invokers = new();

    public Task<Result<TResult>> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        var invoker = (IInvoker<TResult>)Invokers.GetOrAdd(
            (command.GetType(), typeof(TResult)),
            key => Activator.CreateInstance(typeof(CommandInvoker<,>).MakeGenericType(key.Request, key.Result))!);
        return invoker.InvokeAsync(command, serviceProvider, cancellationToken);
    }

    public Task<Result<TResult>> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        var invoker = (IInvoker<TResult>)Invokers.GetOrAdd(
            (query.GetType(), typeof(TResult)),
            key => Activator.CreateInstance(typeof(QueryInvoker<,>).MakeGenericType(key.Request, key.Result))!);
        return invoker.InvokeAsync(query, serviceProvider, cancellationToken);
    }

    private interface IInvoker<TResult>
    {
        Task<Result<TResult>> InvokeAsync(object request, IServiceProvider services, CancellationToken cancellationToken);
    }

    private sealed class CommandInvoker<TCommand, TResult> : IInvoker<TResult>
        where TCommand : ICommand<TResult>
    {
        public Task<Result<TResult>> InvokeAsync(object request, IServiceProvider services, CancellationToken cancellationToken) =>
            services.GetRequiredService<ICommandHandler<TCommand, TResult>>().HandleAsync((TCommand)request, cancellationToken);
    }

    private sealed class QueryInvoker<TQuery, TResult> : IInvoker<TResult>
        where TQuery : IQuery<TResult>
    {
        public Task<Result<TResult>> InvokeAsync(object request, IServiceProvider services, CancellationToken cancellationToken) =>
            services.GetRequiredService<IQueryHandler<TQuery, TResult>>().HandleAsync((TQuery)request, cancellationToken);
    }
}
