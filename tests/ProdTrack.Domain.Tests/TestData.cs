using System.Reflection;
using ProdTrack.Domain.Common;
using ProdTrack.Domain.Products;
using ProdTrack.Domain.Routings;
using ProdTrack.Domain.Stations;

namespace ProdTrack.Domain.Tests;

internal static class TestData
{
    public static readonly DateTimeOffset Now = new(2026, 3, 10, 0, 0, 0, TimeSpan.Zero);

    public static readonly DateOnly Today = new(2026, 3, 10);

    public static ProductSpec PipeMarkerSpec => new() { Material = "Vinyl", ColorSchemeCode = "FLAMMABLE", PipeOdRange = "1-2 in" };

    public static ProductSpec SafetySignSpec => new() { Material = "Aluminium", SignalWord = "DANGER", WidthMm = 297, HeightMm = 210 };

    public static Product PipeMarker(bool requiresArtwork = false) =>
        WithId(Product.Create("PM-1", "Pipe marker", ProductType.PipeMarker, requiresArtwork, PipeMarkerSpec).Value, 1);

    public static Product SafetySign() =>
        WithId(Product.Create("SS-1", "Safety sign", ProductType.SafetySign, requiresArtworkApproval: true, SafetySignSpec).Value, 2);

    public static Station Station(string code, int id)
    {
        var station = WithId(Stations.Station.Create(code, code, StationType.Printing, null).Value, id);
        return station;
    }

    public static Routing Routing(ProductType type, params string[] stationCodes)
    {
        var steps = stationCodes.Select((code, i) => new RoutingStepDefinition((i + 1) * 10, Station(code, i + 1), 5, 0.5m, false)).ToList();
        return WithId(Routings.Routing.Create(type, 1, $"{type} v1", steps, Now).Value, 7);
    }

    /// <summary>Simulates the database-generated identity value.</summary>
    public static T WithId<T>(T entity, int id)
        where T : Entity
    {
        typeof(Entity).GetProperty(nameof(Entity.Id))!.SetValue(entity, id);
        return entity;
    }
}
