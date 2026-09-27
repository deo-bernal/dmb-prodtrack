using ProdTrack.Domain.Common;

namespace ProdTrack.Domain.WorkOrders;

/// <summary>Scrap logged against an operation; requires a reason code (BR-09). Behaviour arrives with PT-034.</summary>
public sealed class ScrapRecord : Entity
{
    private ScrapRecord()
    {
    }

    public ScrapRecord(int operationId, int quantity, int reasonCodeId, string userId, DateTimeOffset occurredAtUtc, string? note, string? photoFileKey)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        OperationId = operationId;
        Quantity = quantity;
        ReasonCodeId = reasonCodeId;
        UserId = userId;
        OccurredAtUtc = occurredAtUtc;
        Note = note;
        PhotoFileKey = photoFileKey;
    }

    public int OperationId { get; private set; }

    public int Quantity { get; private set; }

    public int ReasonCodeId { get; private set; }

    public string? Note { get; private set; }

    public string? PhotoFileKey { get; private set; }

    public string UserId { get; private set; } = string.Empty;

    public DateTimeOffset OccurredAtUtc { get; private set; }
}
