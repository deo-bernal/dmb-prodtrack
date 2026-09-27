using Microsoft.Extensions.Options;
using ProdTrack.Application.Abstractions;

namespace ProdTrack.Infrastructure.Time;

/// <summary>Plant calendar on top of the injected <see cref="TimeProvider"/> (BR-07: server UTC is authoritative).</summary>
public sealed class PlantClock(TimeProvider timeProvider, IOptions<PlantOptions> options) : IPlantClock
{
    private readonly TimeZoneInfo _timeZone = TimeZoneInfo.FindSystemTimeZoneById(options.Value.TimeZone);

    public DateTimeOffset UtcNow => timeProvider.GetUtcNow();

    public DateOnly Today => DateOnly.FromDateTime(ToPlantTime(UtcNow).DateTime);

    public DateTimeOffset ToPlantTime(DateTimeOffset utc) => TimeZoneInfo.ConvertTime(utc, _timeZone);
}
