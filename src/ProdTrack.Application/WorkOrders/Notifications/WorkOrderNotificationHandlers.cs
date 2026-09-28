using Microsoft.EntityFrameworkCore;
using ProdTrack.Application.Abstractions;
using ProdTrack.Domain.WorkOrders;

namespace ProdTrack.Application.WorkOrders.Notifications;

/// <summary>Pushes work order status changes (hold, resume, cancel, start, complete) to dashboards after commit.</summary>
internal sealed class WorkOrderUpdatedNotificationHandler(INotifier notifier) : IDomainEventHandler<WorkOrderUpdated>
{
    public Task HandleAsync(WorkOrderUpdated domainEvent, CancellationToken cancellationToken) =>
        notifier.WorkOrderChangedAsync(
            new WorkOrderChangedNotification(domainEvent.WorkOrderId, domainEvent.Number, domainEvent.Status.ToString()),
            cancellationToken);
}

/// <summary>Pushes operation changes to the station queue, the work order watchers and the dashboard (PT-030, PT-041).</summary>
internal sealed class OperationChangedNotificationHandler(INotifier notifier, IAppDbContext db) : IDomainEventHandler<OperationChanged>
{
    public async Task HandleAsync(OperationChanged domainEvent, CancellationToken cancellationToken)
    {
        var stationCode = await db.Stations.AsNoTracking()
            .Where(s => s.Id == domainEvent.StationId)
            .Select(s => s.Code)
            .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;
        await notifier.OperationChangedAsync(
            new OperationChangedNotification(
                domainEvent.WorkOrderId,
                domainEvent.Number,
                domainEvent.OperationId,
                domainEvent.Sequence,
                stationCode,
                domainEvent.Status.ToString()),
            cancellationToken);
    }
}
