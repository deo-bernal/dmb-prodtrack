namespace ProdTrack.Application.Abstractions;

/// <summary>Plant-local calendar based on the injected TimeProvider and Plant:TimeZone (default Asia/Manila).</summary>
public interface IPlantClock
{
    DateTimeOffset UtcNow { get; }

    DateOnly Today { get; }

    DateTimeOffset ToPlantTime(DateTimeOffset utc);
}
