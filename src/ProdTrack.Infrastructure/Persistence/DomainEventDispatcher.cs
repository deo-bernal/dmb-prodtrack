using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProdTrack.Application.Abstractions;
using ProdTrack.Domain.Common;

namespace ProdTrack.Infrastructure.Persistence;

/// <summary>
/// Dispatches domain events to <see cref="IDomainEventHandler{TEvent}"/> after a successful commit. Failures are logged,
/// never thrown: clients re-sync on reconnect (docs/02 section 8).
/// </summary>
public sealed partial class DomainEventDispatcher(IServiceProvider serviceProvider, ILogger<DomainEventDispatcher> logger)
{
    public async Task DispatchAsync(IReadOnlyList<IDomainEvent> events, CancellationToken cancellationToken)
    {
        foreach (var domainEvent in events)
        {
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
            var method = handlerType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync))!;
            foreach (var handler in serviceProvider.GetServices(handlerType))
            {
                try
                {
                    await (Task)method.Invoke(handler, [domainEvent, cancellationToken])!;
                }
#pragma warning disable CA1031 // Notification failures must not fail the committed business operation.
                catch (Exception ex)
#pragma warning restore CA1031
                {
                    LogHandlerFailed(logger, ex, domainEvent.GetType().Name, handler!.GetType().Name);
                }
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Domain event {EventType} handler {HandlerType} failed")]
    private static partial void LogHandlerFailed(ILogger logger, Exception exception, string eventType, string handlerType);
}
