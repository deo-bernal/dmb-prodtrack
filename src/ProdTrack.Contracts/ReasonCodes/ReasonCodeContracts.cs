namespace ProdTrack.Contracts.ReasonCodes;

/// <summary>Reason code. Category: Scrap|Pause|Downtime|Hold|Skip.</summary>
public sealed record ReasonCodeDto(int Id, string Code, string Description, string Category, bool IsActive);

public sealed record CreateReasonCodeRequest(string Code, string Description, string Category);

public sealed record UpdateReasonCodeRequest(string Description, bool IsActive);
