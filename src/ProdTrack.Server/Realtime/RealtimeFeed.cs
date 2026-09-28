namespace ProdTrack.Server.Realtime;

/// <summary>A production change as seen by in-process (Blazor Server) subscribers.</summary>
public sealed record RealtimeChange(int WorkOrderId, string Number, string? StationCode, string Status);

/// <summary>
/// In-process fan-out of the same events the SignalR hub pushes, so Blazor Server circuits (dashboard, work order
/// detail) update live without polling or a hub round-trip. Singleton; handlers must be cheap and non-throwing.
/// </summary>
public interface IRealtimeFeed
{
    event Action<RealtimeChange>? Changed;

    void Publish(RealtimeChange change);
}

internal sealed class RealtimeFeed(ILogger<RealtimeFeed> logger) : IRealtimeFeed
{
    public event Action<RealtimeChange>? Changed;

    public void Publish(RealtimeChange change)
    {
        var handlers = Changed;
        if (handlers is null)
        {
            return;
        }

        foreach (var handler in handlers.GetInvocationList().Cast<Action<RealtimeChange>>())
        {
            try
            {
                handler(change);
            }
            catch (Exception ex) when (ex is InvalidOperationException or ObjectDisposedException)
            {
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug(ex, "Realtime subscriber failed for {WorkOrderNumber}", change.Number);
                }
            }
        }
    }
}
