namespace ProdTrack.Contracts.Realtime;

/// <summary>SignalR hub route, groups and method names (docs/02 section 8).</summary>
public static class ProductionHubContract
{
    public const string Route = "/hubs/production";
    public const string DashboardGroup = "dashboard";

    public const string WorkOrderChangedMethod = nameof(IProductionClient.WorkOrderChanged);
    public const string OperationChangedMethod = nameof(IProductionClient.OperationChanged);

    public static string StationGroup(string stationCode) => $"station:{stationCode}";

    public static string WorkOrderGroup(int workOrderId) => $"workorder:{workOrderId}";
}

/// <summary>Server-to-client messages (strongly typed hub).</summary>
public interface IProductionClient
{
    Task WorkOrderChanged(WorkOrderChangedDto message);

    Task OperationChanged(OperationChangedDto message);
}

public sealed record WorkOrderChangedDto(int WorkOrderId, string Number, string Status);

public sealed record OperationChangedDto(int WorkOrderId, string Number, int OperationId, int Sequence, string StationCode, string Status);
