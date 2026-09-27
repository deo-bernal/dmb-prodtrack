using ProdTrack.Application.Security;
using ProdTrack.Application.Stations.CreateStation;
using ProdTrack.Application.Stations.DeactivateStation;
using ProdTrack.Application.Stations.GetStations;
using ProdTrack.Domain.Common;
using ProdTrack.Domain.Stations;
using ProdTrack.TestSupport;

namespace ProdTrack.Application.Tests;

[Trait("Category", "Unit")]
[Trait("Story", "PT-013")]
public sealed class StationUseCaseTests : IAsyncLifetime
{
    private TestApplication _app = null!;

    public async Task InitializeAsync() => _app = await TestApplication.CreateAsync();

    public async Task DisposeAsync() => await _app.DisposeAsync();

    [Fact]
    [Trait("Story", "PT-016")]
    public async Task Reference_data_seeds_the_brd_stations()
    {
        var stations = (await _app.QueryAsync(new GetStationsQuery())).Value;

        stations.Select(s => s.Code).Should().Contain(["PREPRESS", "PRINT-01", "LAM-01", "ENGRAVE-01", "DIECUT-01", "QC-01", "PACK-01"]);
    }

    [Fact]
    public async Task Create_station_succeeds_for_admin()
    {
        var result = await _app.SendAsync(new CreateStationCommand("print-02", "Second printer", StationType.Printing, "WC-B"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Code.Should().Be("PRINT-02");
    }

    [Fact]
    public async Task Duplicate_code_is_a_validation_error()
    {
        var result = await _app.SendAsync(new CreateStationCommand("PRINT-01", "Duplicate", StationType.Printing, null));

        result.Error.Should().BeOfType<ValidationError>().Which.Errors.Should().ContainKey("code");
    }

    [Fact]
    public async Task Validator_runs_before_the_handler()
    {
        var result = await _app.SendAsync(new CreateStationCommand(string.Empty, string.Empty, StationType.Printing, null));

        result.Error.Should().BeOfType<ValidationError>().Which.Errors.Should().ContainKeys("code", "name");
    }

    [Theory]
    [InlineData(Roles.Planner)]
    [InlineData(Roles.Operator)]
    [InlineData(Roles.Viewer)]
    [Trait("Story", "PT-010")]
    public async Task Non_admins_cannot_create_stations(string role)
    {
        _app.User.SetRoles(role);

        var result = await _app.SendAsync(new CreateStationCommand("PRINT-09", "Printer", StationType.Printing, null));

        result.Error!.Type.Should().Be(ErrorType.Forbidden);
        result.Error.Code.Should().Be("Authorization.Forbidden");
    }

    [Fact]
    public async Task Deactivated_station_is_hidden_unless_requested()
    {
        var station = (await _app.QueryAsync(new GetStationsQuery())).Value.First(s => s.Code == "ENGRAVE-01");

        var deactivated = await _app.SendAsync(new DeactivateStationCommand(station.Id));

        deactivated.IsSuccess.Should().BeTrue();
        deactivated.Value.OpenOperationCount.Should().Be(0);
        (await _app.QueryAsync(new GetStationsQuery())).Value.Should().NotContain(s => s.Code == "ENGRAVE-01");
        (await _app.QueryAsync(new GetStationsQuery(IncludeInactive: true))).Value.Should().Contain(s => s.Code == "ENGRAVE-01" && !s.IsActive);
    }

    [Fact]
    public async Task Deactivating_unknown_station_is_not_found()
    {
        var result = await _app.SendAsync(new DeactivateStationCommand(9999));

        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }
}
