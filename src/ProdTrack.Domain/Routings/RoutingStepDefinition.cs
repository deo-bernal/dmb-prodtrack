using ProdTrack.Domain.Stations;

namespace ProdTrack.Domain.Routings;

/// <summary>Input for a routing step when creating a routing version.</summary>
public sealed record RoutingStepDefinition(
    int Sequence,
    Station Station,
    decimal SetupMinutes,
    decimal StdMinutesPerUnit,
    bool AllowOverlap);
