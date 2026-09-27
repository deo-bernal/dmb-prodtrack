namespace ProdTrack.Domain.Common;

/// <summary>Validation failure with field-level messages (400 ProblemDetails with an <c>errors</c> dictionary).</summary>
public sealed record ValidationError(IReadOnlyDictionary<string, string[]> Errors)
    : Error("Validation", "One or more validation errors occurred.", ErrorType.Validation);
