namespace ProdTrack.Application.Abstractions;

/// <summary>Real-time push to clients (SignalR hub and the in-process Blazor feed). Called after a successful commit.</summary>
public interface INotifier
{
    Task WorkOrderChangedAsync(WorkOrderChangedNotification notification, CancellationToken cancellationToken);

    Task OperationChangedAsync(OperationChangedNotification notification, CancellationToken cancellationToken);
}

public sealed record WorkOrderChangedNotification(int WorkOrderId, string Number, string Status);

public sealed record OperationChangedNotification(int WorkOrderId, string Number, int OperationId, int Sequence, string StationCode, string Status);
