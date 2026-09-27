namespace ProdTrack.Domain.Audit;

/// <summary>Immutable audit record written automatically on every save (PT-047). Insert/select only.</summary>
public sealed class AuditEntry
{
    private AuditEntry()
    {
    }

    public AuditEntry(string entityName, string entityKey, AuditAction action, string changesJson, string? userId, string? correlationId, DateTimeOffset occurredAtUtc)
    {
        EntityName = entityName;
        EntityKey = entityKey;
        Action = action;
        ChangesJson = changesJson;
        UserId = userId;
        CorrelationId = correlationId;
        OccurredAtUtc = occurredAtUtc;
    }

    public long Id { get; private set; }

    public string EntityName { get; private set; } = string.Empty;

    public string EntityKey { get; private set; } = string.Empty;

    public AuditAction Action { get; private set; }

    /// <summary>JSON object: property name to { old, new }.</summary>
    public string ChangesJson { get; private set; } = "{}";

    public string? UserId { get; private set; }

    public string? CorrelationId { get; private set; }

    public DateTimeOffset OccurredAtUtc { get; private set; }
}
