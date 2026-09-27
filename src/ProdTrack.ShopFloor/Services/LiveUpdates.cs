using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using ProdTrack.Contracts.Realtime;

namespace ProdTrack.ShopFloor.Services;

/// <summary>
/// SignalR connection to the production hub (same-origin cookie). Pages subscribe to a station or work order group
/// and reload when a change arrives. Reconnects automatically; pages also reload after a reconnect.
/// </summary>
public sealed class LiveUpdates(NavigationManager navigation) : IAsyncDisposable
{
    private HubConnection? connection;
    private string? station;
    private int? workOrderId;

    public event Func<Task>? Changed;

    public bool IsConnected => connection?.State == HubConnectionState.Connected;

    public async Task WatchStationAsync(string stationCode)
    {
        await EnsureStartedAsync();
        if (connection is null || !IsConnected)
        {
            return;
        }

        if (station is not null && station != stationCode)
        {
            await connection.InvokeAsync("LeaveStation", station);
        }

        station = stationCode;
        await connection.InvokeAsync("JoinStation", stationCode);
    }

    public async Task WatchWorkOrderAsync(int id)
    {
        await EnsureStartedAsync();
        if (connection is null || !IsConnected)
        {
            return;
        }

        workOrderId = id;
        await connection.InvokeAsync("WatchWorkOrder", id);
    }

    private async Task EnsureStartedAsync()
    {
        if (connection is not null)
        {
            return;
        }

        connection = new HubConnectionBuilder()
            .WithUrl(navigation.ToAbsoluteUri(ProductionHubContract.Route))
            .WithAutomaticReconnect()
            .Build();
        connection.On<OperationChangedDto>(ProductionHubContract.OperationChangedMethod, _ => RaiseAsync());
        connection.On<WorkOrderChangedDto>(ProductionHubContract.WorkOrderChangedMethod, _ => RaiseAsync());
        connection.Reconnected += async _ =>
        {
            if (station is not null)
            {
                await connection.InvokeAsync("JoinStation", station);
            }

            if (workOrderId is not null)
            {
                await connection.InvokeAsync("WatchWorkOrder", workOrderId.Value);
            }

            await RaiseAsync();
        };

        try
        {
            await connection.StartAsync();
        }
        catch (HttpRequestException)
        {
            // Live updates are a convenience; pages still work (manual refresh) without the hub.
        }
    }

    private Task RaiseAsync() => Changed?.Invoke() ?? Task.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        if (connection is not null)
        {
            await connection.DisposeAsync();
        }
    }
}
