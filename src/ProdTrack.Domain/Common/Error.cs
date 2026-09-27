namespace ProdTrack.Domain.Common;

/// <summary>A typed, expected failure with a stable code (e.g. <c>WorkOrder.ArtworkNotApproved</c>).</summary>
public record Error(string Code, string Message, ErrorType Type)
{
    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);

    public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);

    public static Error BusinessRule(string code, string message) => new(code, message, ErrorType.BusinessRule);

    public static Error Forbidden(string code, string message) => new(code, message, ErrorType.Forbidden);

    public static Error PreconditionFailed(string code, string message) => new(code, message, ErrorType.PreconditionFailed);

    public static ValidationError Validation(string field, string message) =>
        new(new Dictionary<string, string[]> { [field] = [message] });
}
