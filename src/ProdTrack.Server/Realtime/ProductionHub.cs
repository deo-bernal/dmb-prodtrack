using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ProdTrack.Contracts.Realtime;

namespace ProdTrack.Server.Realtime;

/// <summary>
/// Push-only hub (docs/02 section 8): clients subscribe to groups; business writes always go through REST or
/// in-process use cases.
/// </summary>
[Authorize]
public sealed class ProductionHub : Hub<IProductionClient>
{
    public Task JoinDashboard() => Groups.AddToGroupAsync(Context.ConnectionId, ProductionHubContract.DashboardGroup);

    public Task JoinStation(string stationCode) =>
        Groups.AddToGroupAsync(Context.ConnectionId, ProductionHubContract.StationGroup(stationCode.Trim().ToUpperInvariant()));

    public Task LeaveStation(string stationCode) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, ProductionHubContract.StationGroup(stationCode.Trim().ToUpperInvariant()));

    public Task WatchWorkOrder(int workOrderId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, ProductionHubContract.WorkOrderGroup(workOrderId));
}
