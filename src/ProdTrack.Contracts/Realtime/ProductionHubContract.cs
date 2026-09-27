namespace ProdTrack.Contracts.Realtime;

/// <summary>SignalR hub route, groups and method names (docs/02 section 8).</summary>
public static class ProductionHubContract
{
    public const string Route = "/hubs/production";
    public const string DashboardGroup = "dashboard";

    public static string StationGroup(string stationCode) => $"station:{stationCode}";

    public static string WorkOrderGroup(int workOrderId) => $"workorder:{workOrderId}";
}

/// <summary>Server-to-client messages (strongly typed hub).</summary>
public interface IProductionClient
{
    Task WorkOrderChanged(WorkOrderChangedDto message);
}

public sealed record WorkOrderChangedDto(int WorkOrderId, string Number, string Status);
