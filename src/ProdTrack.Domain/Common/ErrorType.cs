namespace ProdTrack.Domain.Common;

/// <summary>Categories of expected failures; mapped to HTTP status codes by the Server (docs/02 section 11).</summary>
public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
    Forbidden,
    BusinessRule,

    /// <summary>The client's If-Match version is stale (HTTP 412).</summary>
    PreconditionFailed,
}
