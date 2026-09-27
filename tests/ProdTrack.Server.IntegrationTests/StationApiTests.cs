using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using ProdTrack.Application.Security;
using ProdTrack.Contracts.Stations;
using ProdTrack.Domain.Audit;

namespace ProdTrack.Server.IntegrationTests;

[Collection(ServerCollection.Name)]
[Trait("Category", "Integration")]
[Trait("Story", "PT-013")]
public sealed class StationApiTests(ProdTrackFactory factory)
{
    [Fact]
    public async Task Create_update_and_deactivate_station_with_audit()
    {
        using var client = factory.CreateClientAs(Roles.Admin);

        using var created = await client.PostAsJsonAsync("/api/v1/stations", new { code = "cut-02", name = "Second cutter", type = "Cutting", workCenter = "WC-C" });
        created.StatusCode.Should().Be(HttpStatusCode.Created);
        var station = await created.Content.ReadFromJsonAsync<StationDto>();
        station!.Code.Should().Be("CUT-02");
        created.Headers.Location!.ToString().Should().EndWith($"/api/v1/stations/{station.Id}");

        using var duplicate = await client.PostAsJsonAsync("/api/v1/stations", new { code = "CUT-02", name = "Again", type = "Cutting" });
        duplicate.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        using var updated = await client.PutAsJsonAsync($"/api/v1/stations/{station.Id}", new { name = "Cutter 2", type = "Cutting", workCenter = "WC-C", isActive = true });
        updated.StatusCode.Should().Be(HttpStatusCode.OK);
        (await updated.Content.ReadFromJsonAsync<StationDto>())!.Name.Should().Be("Cutter 2");

        using var deactivated = await client.DeleteAsync(new Uri($"/api/v1/stations/{station.Id}", UriKind.Relative));
        deactivated.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await deactivated.Content.ReadFromJsonAsync<DeactivateStationResponse>();
        result!.Station.IsActive.Should().BeFalse();
        result.OpenOperationCount.Should().Be(0);

        var active = await client.GetFromJsonAsync<List<StationDto>>("/api/v1/stations");
        active!.Should().NotContain(s => s.Code == "CUT-02");

        var audit = await factory.WithDbAsync(db => db.AuditEntries
            .Where(a => a.EntityName == "Station" && a.EntityKey == station.Id.ToString(System.Globalization.CultureInfo.InvariantCulture))
            .OrderBy(a => a.Id)
            .ToListAsync());
        audit.Select(a => a.Action).Should().StartWith([AuditAction.Insert, AuditAction.Update]);
        audit.Should().OnlyContain(a => a.UserId == "it-admin");
    }

    [Fact]
    public async Task Invalid_enum_value_is_a_validation_error()
    {
        using var client = factory.CreateClientAs(Roles.Admin);

        using var response = await client.PostAsJsonAsync("/api/v1/stations", new { code = "X-01", name = "X", type = "Teleporter" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
