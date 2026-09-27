using ProdTrack.Domain.Products;

namespace ProdTrack.Application.Routings;

public sealed record RoutingModel(
    int Id,
    ProductType ProductType,
    int Version,
    string Name,
    bool IsCurrent,
    DateTimeOffset CreatedAtUtc,
    IReadOnlyList<RoutingStepModel> Steps);

public sealed record RoutingStepModel(
    int Sequence,
    int StationId,
    string StationCode,
    string StationName,
    decimal SetupMinutes,
    decimal StdMinutesPerUnit,
    bool AllowOverlap);
