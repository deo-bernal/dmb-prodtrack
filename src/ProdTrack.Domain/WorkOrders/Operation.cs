using ProdTrack.Domain.Common;
using ProdTrack.Domain.Routings;

namespace ProdTrack.Domain.WorkOrders;

/// <summary>One routed step of a work order at a station. Execution behaviour arrives with PT-031..PT-034.</summary>
public sealed class Operation : Entity
{
    private readonly List<OperationEvent> _events = [];
    private readonly List<ScrapRecord> _scrapRecords = [];

    private Operation()
    {
    }

    internal Operation(RoutingStep step, OperationStatus status, int inputQuantity)
    {
        Sequence = step.Sequence;
        StationId = step.StationId;
        SetupMinutes = step.SetupMinutes;
        StdMinutesPerUnit = step.StdMinutesPerUnit;
        AllowOverlap = step.AllowOverlap;
        Status = status;
        InputQuantity = inputQuantity;
    }

    public int WorkOrderId { get; private set; }

    public int Sequence { get; private set; }

    public int StationId { get; private set; }

    public OperationStatus Status { get; private set; }

    public int InputQuantity { get; private set; }

    public int GoodQuantity { get; private set; }

    public int ScrapQuantity { get; private set; }

    public decimal SetupMinutes { get; private set; }

    public decimal StdMinutesPerUnit { get; private set; }

    public bool AllowOverlap { get; private set; }

    public bool IsRework { get; private set; }

    public DateTimeOffset? StartedAtUtc { get; private set; }

    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public byte[] RowVersion { get; private set; } = [];

    public IReadOnlyList<OperationEvent> Events => _events;

    public IReadOnlyList<ScrapRecord> ScrapRecords => _scrapRecords;
}
