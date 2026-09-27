namespace ProdTrack.Application.Abstractions;

/// <summary>Correlation ID of the current request (X-Correlation-Id), used by logging and the audit trail.</summary>
public interface ICorrelationIdAccessor
{
    string? CorrelationId { get; }
}
