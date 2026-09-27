using ProdTrack.Domain.Common;

namespace ProdTrack.Application.Abstractions;

/// <summary>Handles a domain event after the unit of work committed.</summary>
public interface IDomainEventHandler<in TEvent>
    where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken);
}
