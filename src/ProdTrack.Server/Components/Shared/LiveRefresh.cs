using ProdTrack.Server.Realtime;

namespace ProdTrack.Server.Components.Shared;

/// <summary>
/// Debounced subscription to <see cref="IRealtimeFeed"/> for Blazor components: bursts of changes collapse into one
/// reload after <paramref name="delay"/>.
/// </summary>
internal sealed class LiveRefresh : IDisposable
{
    private readonly IRealtimeFeed feed;
    private readonly Func<RealtimeChange, bool> filter;
    private readonly Timer timer;
    private readonly TimeSpan delay;

    public LiveRefresh(IRealtimeFeed feed, Func<RealtimeChange, bool> filter, Func<Task> reload, TimeSpan delay)
    {
        this.feed = feed;
        this.filter = filter;
        this.delay = delay;
        timer = new Timer(_ => _ = reload(), null, Timeout.Infinite, Timeout.Infinite);
        feed.Changed += OnChanged;
    }

    public DateTimeOffset? LastChangeUtc { get; private set; }

    private void OnChanged(RealtimeChange change)
    {
        if (filter(change))
        {
            LastChangeUtc = DateTimeOffset.UtcNow;
            timer.Change(delay, Timeout.InfiniteTimeSpan);
        }
    }

    public void Dispose()
    {
        feed.Changed -= OnChanged;
        timer.Dispose();
    }
}
