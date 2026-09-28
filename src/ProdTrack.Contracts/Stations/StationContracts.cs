namespace ProdTrack.Contracts.Stations;

/// <summary>Production station. Type: Prepress|Printing|Laminating|Engraving|Cutting|Inspection|Packing.</summary>
public sealed record StationDto(int Id, string Code, string Name, string Type, string? WorkCenter, bool IsActive, string? Version = null);

public sealed record CreateStationRequest(string Code, string Name, string Type, string? WorkCenter);

public sealed record UpdateStationRequest(string Name, string Type, string? WorkCenter, bool IsActive, string? Version = null);

/// <summary>Result of deactivating a station; <see cref="OpenOperationCount"/> &gt; 0 means open work continues there.</summary>
public sealed record DeactivateStationResponse(StationDto Station, int OpenOperationCount);
