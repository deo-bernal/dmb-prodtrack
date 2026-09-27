using ProdTrack.Application.Abstractions;
using ProdTrack.Domain.WorkOrders;

namespace ProdTrack.Application.WorkOrders.Notifications;

/// <summary>Pushes released work orders to dashboards and station queues after commit.</summary>
internal sealed class WorkOrderReleasedNotificationHandler(INotifier notifier) : IDomainEventHandler<WorkOrderReleased>
{
    public Task HandleAsync(WorkOrderReleased domainEvent, CancellationToken cancellationToken) =>
        notifier.WorkOrderChangedAsync(
            new WorkOrderChangedNotification(domainEvent.WorkOrderId, domainEvent.Number, nameof(WorkOrderStatus.Released)),
            cancellationToken);
}
