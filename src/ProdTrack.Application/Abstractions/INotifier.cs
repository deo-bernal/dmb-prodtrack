namespace ProdTrack.Application.Abstractions;

/// <summary>Real-time push to clients (SignalR hub in the Server). Called after a successful commit.</summary>
public interface INotifier
{
    Task WorkOrderChangedAsync(WorkOrderChangedNotification notification, CancellationToken cancellationToken);
}

public sealed record WorkOrderChangedNotification(int WorkOrderId, string Number, string Status);
