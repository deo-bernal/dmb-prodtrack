using Microsoft.AspNetCore.SignalR;
using ProdTrack.Application.Abstractions;
using ProdTrack.Contracts.Realtime;

namespace ProdTrack.Server.Realtime;

internal sealed class SignalRNotifier(IHubContext<ProductionHub, IProductionClient> hub) : INotifier
{
    public Task WorkOrderChangedAsync(WorkOrderChangedNotification notification, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);
        var message = new WorkOrderChangedDto(notification.WorkOrderId, notification.Number, notification.Status);
        return Task.WhenAll(
            hub.Clients.Group(ProductionHubContract.DashboardGroup).WorkOrderChanged(message),
            hub.Clients.Group(ProductionHubContract.WorkOrderGroup(notification.WorkOrderId)).WorkOrderChanged(message));
    }
}
