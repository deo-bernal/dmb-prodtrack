namespace ProdTrack.Contracts.Routings;

public sealed record RoutingStepDto(int Sequence, int StationId, string StationCode, string StationName, decimal SetupMinutes, decimal StdMinutesPerUnit, bool AllowOverlap);

public sealed record RoutingDto(int Id, string ProductType, int Version, string Name, bool IsCurrent, DateTimeOffset CreatedAtUtc, IReadOnlyList<RoutingStepDto> Steps);

public sealed record CreateRoutingStepRequest(int Sequence, string StationCode, decimal SetupMinutes, decimal StdMinutesPerUnit, bool AllowOverlap);

/// <summary>Creates the next routing version for a product type.</summary>
public sealed record CreateRoutingRequest(string ProductType, string Name, IReadOnlyList<CreateRoutingStepRequest> Steps);
