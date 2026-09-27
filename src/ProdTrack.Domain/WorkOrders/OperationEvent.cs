namespace ProdTrack.Domain.WorkOrders;

/// <summary>Shop-floor event on an operation (start, pause, resume, complete, skip). High volume: bigint key.</summary>
public sealed class OperationEvent
{
    private OperationEvent()
    {
    }

    public OperationEvent(int operationId, OperationEventType type, string userId, DateTimeOffset occurredAtUtc, int? reasonCodeId, string? deviceId, Guid? requestId)
    {
        OperationId = operationId;
        Type = type;
        UserId = userId;
        OccurredAtUtc = occurredAtUtc;
        ReasonCodeId = reasonCodeId;
        DeviceId = deviceId;
        RequestId = requestId;
    }

    public long Id { get; private set; }

    public int OperationId { get; private set; }

    public OperationEventType Type { get; private set; }

    public int? ReasonCodeId { get; private set; }

    public string UserId { get; private set; } = string.Empty;

    public DateTimeOffset OccurredAtUtc { get; private set; }

    public string? DeviceId { get; private set; }

    /// <summary>Client-generated idempotency key (docs/02 section 11).</summary>
    public Guid? RequestId { get; private set; }
}
