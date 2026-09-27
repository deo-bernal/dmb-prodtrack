using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using ProdTrack.Infrastructure.Time;

namespace ProdTrack.Infrastructure.Tests;

[Trait("Category", "Unit")]
public class PlantClockTests
{
    [Fact]
    public void Today_uses_the_plant_time_zone()
    {
        var time = new FakeTimeProvider(new DateTimeOffset(2026, 3, 10, 17, 30, 0, TimeSpan.Zero));
        var clock = new PlantClock(time, Options.Create(new PlantOptions { TimeZone = "Asia/Manila" }));

        clock.Today.Should().Be(new DateOnly(2026, 3, 11));
        clock.ToPlantTime(time.GetUtcNow()).Offset.Should().Be(TimeSpan.FromHours(8));
    }
}
