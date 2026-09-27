namespace ProdTrack.ApiClient;

/// <summary>Plain-language API error converted from ProblemDetails.</summary>
public sealed record ApiError(int StatusCode, string Title, string? Detail);
