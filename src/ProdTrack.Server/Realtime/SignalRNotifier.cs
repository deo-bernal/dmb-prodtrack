using Microsoft.AspNetCore.SignalR;
using ProdTrack.Application.Abstractions;
using ProdTrack.Contracts.Realtime;

namespace ProdTrack.Server.Realtime;

/// <summary>Pushes committed changes to SignalR groups (dashboard, station, work order) and the in-process feed.</summary>
internal sealed class SignalRNotifier(IHubContext<ProductionHub, IProductionClient> hub, IRealtimeFeed feed) : INotifier
{
    public Task WorkOrderChangedAsync(WorkOrderChangedNotification notification, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);
        var message = new WorkOrderChangedDto(notification.WorkOrderId, notification.Number, notification.Status);
        feed.Publish(new RealtimeChange(notification.WorkOrderId, notification.Number, null, notification.Status));
        return Task.WhenAll(
            hub.Clients.Group(ProductionHubContract.DashboardGroup).WorkOrderChanged(message),
            hub.Clients.Group(ProductionHubContract.WorkOrderGroup(notification.WorkOrderId)).WorkOrderChanged(message));
    }

    public Task OperationChangedAsync(OperationChangedNotification notification, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);
        var message = new OperationChangedDto(
            notification.WorkOrderId, notification.Number, notification.OperationId, notification.Sequence, notification.StationCode, notification.Status);
        feed.Publish(new RealtimeChange(notification.WorkOrderId, notification.Number, notification.StationCode, notification.Status));
        return Task.WhenAll(
            hub.Clients.Group(ProductionHubContract.DashboardGroup).OperationChanged(message),
            hub.Clients.Group(ProductionHubContract.StationGroup(notification.StationCode)).OperationChanged(message),
            hub.Clients.Group(ProductionHubContract.WorkOrderGroup(notification.WorkOrderId)).OperationChanged(message));
    }
}
