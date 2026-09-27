using ProdTrack.Domain.Common;
using ProdTrack.Domain.Products;
using ProdTrack.Domain.Routings;

namespace ProdTrack.Domain.Tests;

[Trait("Category", "Unit")]
[Trait("Story", "PT-014")]
public class RoutingTests
{
    [Fact]
    public void Create_orders_steps_by_sequence_and_is_current()
    {
        var steps = new[]
        {
            new RoutingStepDefinition(20, TestData.Station("LAM-01", 2), 0, 1, false),
            new RoutingStepDefinition(10, TestData.Station("PRINT-01", 1), 10, 0.5m, false),
        };

        var routing = Routing.Create(ProductType.Label, 1, "Labels", steps, TestData.Now).Value;

        routing.IsCurrent.Should().BeTrue();
        routing.Steps.Select(s => s.Sequence).Should().Equal(10, 20);
        routing.Steps[0].StationId.Should().Be(1);
    }

    [Fact]
    public void Create_requires_at_least_one_step()
    {
        var result = Routing.Create(ProductType.Label, 1, "Labels", [], TestData.Now);

        result.Error.Should().BeOfType<ValidationError>().Which.Errors.Should().ContainKey("steps");
    }

    [Fact]
    public void Create_rejects_duplicate_sequences()
    {
        var station = TestData.Station("PRINT-01", 1);
        var steps = new[]
        {
            new RoutingStepDefinition(10, station, 0, 1, false),
            new RoutingStepDefinition(10, TestData.Station("LAM-01", 2), 0, 1, false),
        };

        Routing.Create(ProductType.Label, 1, "Labels", steps, TestData.Now).IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_rejects_inactive_stations()
    {
        var station = TestData.Station("PRINT-01", 1);
        station.Deactivate();

        var result = Routing.Create(ProductType.Label, 1, "Labels", [new RoutingStepDefinition(10, station, 0, 1, false)], TestData.Now);

        result.Error!.Code.Should().Be("Station.Inactive");
    }

    [Fact]
    public void Supersede_clears_current_flag()
    {
        var routing = TestData.Routing(ProductType.Label, "PRINT-01");

        routing.Supersede();

        routing.IsCurrent.Should().BeFalse();
    }
}
