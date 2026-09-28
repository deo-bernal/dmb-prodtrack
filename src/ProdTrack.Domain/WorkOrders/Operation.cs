using ProdTrack.Domain.Common;
using ProdTrack.Domain.Routings;

namespace ProdTrack.Domain.WorkOrders;

/// <summary>One routed step of a work order at a station. State changes go through the <see cref="WorkOrder"/> aggregate.</summary>
public sealed class Operation : Entity
{
    public const int NoteMaxLength = 500;

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

    /// <summary>User who started the operation (PT-031).</summary>
    public string? StartedBy { get; private set; }

    public byte[] RowVersion { get; private set; } = [];

    public IReadOnlyList<OperationEvent> Events => _events;

    public IReadOnlyList<ScrapRecord> ScrapRecords => _scrapRecords;

    /// <summary>Operations that can be worked on at the station (station queue, PT-030).</summary>
    public bool IsActionable => Status is OperationStatus.Ready or OperationStatus.InProgress or OperationStatus.Paused;

    internal void Start(string userId, DateTimeOffset nowUtc, int? overlapInputQuantity, Guid? requestId, string? deviceId)
    {
        if (overlapInputQuantity is { } input)
        {
            InputQuantity = input;
        }

        Status = OperationStatus.InProgress;
        StartedAtUtc ??= nowUtc;
        StartedBy ??= userId;
        _events.Add(new OperationEvent(Id, OperationEventType.Start, userId, nowUtc, null, deviceId, requestId));
    }

    internal Result Pause(int reasonCodeId, string userId, DateTimeOffset nowUtc, Guid? requestId)
    {
        if (Status != OperationStatus.InProgress)
        {
            return OperationErrors.InvalidTransition(Status, "pause");
        }

        Status = OperationStatus.Paused;
        _events.Add(new OperationEvent(Id, OperationEventType.Pause, userId, nowUtc, reasonCodeId, null, requestId));
        return Result.Success();
    }

    internal Result Resume(string userId, DateTimeOffset nowUtc, Guid? requestId)
    {
        if (Status != OperationStatus.Paused)
        {
            return OperationErrors.InvalidTransition(Status, "resume");
        }

        Status = OperationStatus.InProgress;
        _events.Add(new OperationEvent(Id, OperationEventType.Resume, userId, nowUtc, null, null, requestId));
        return Result.Success();
    }

    internal Result LogScrap(int quantity, int reasonCodeId, string? note, string userId, DateTimeOffset nowUtc)
    {
        if (Status is not (OperationStatus.InProgress or OperationStatus.Paused))
        {
            return OperationErrors.InvalidTransition(Status, "log scrap for");
        }

        if (quantity <= 0)
        {
            return OperationErrors.ScrapQuantity;
        }

        if (GoodQuantity + ScrapQuantity + quantity > InputQuantity)
        {
            return Error.Validation("quantity", $"Scrap would exceed the input quantity of {InputQuantity}.");
        }

        var trimmed = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        if (trimmed?.Length > NoteMaxLength)
        {
            return Error.Validation("note", $"Note must be at most {NoteMaxLength} characters.");
        }

        ScrapQuantity += quantity;
        _scrapRecords.Add(new ScrapRecord(Id, quantity, reasonCodeId, userId, nowUtc, trimmed, photoFileKey: null));
        return Result.Success();
    }

    internal Result Complete(int goodQuantity, string userId, DateTimeOffset nowUtc, Guid? requestId)
    {
        if (Status != OperationStatus.InProgress)
        {
            return OperationErrors.InvalidTransition(Status, "complete");
        }

        if (goodQuantity < 0)
        {
            return OperationErrors.InvalidQuantity("goodQuantity");
        }

        if (goodQuantity + ScrapQuantity > InputQuantity)
        {
            return OperationErrors.QuantityExceedsInput(InputQuantity);
        }

        GoodQuantity = goodQuantity;
        Status = OperationStatus.Completed;
        CompletedAtUtc = nowUtc;
        _events.Add(new OperationEvent(Id, OperationEventType.Complete, userId, nowUtc, null, null, requestId));
        return Result.Success();
    }

    internal void MakeReady(int inputQuantity)
    {
        InputQuantity = inputQuantity;
        if (Status == OperationStatus.Pending)
        {
            Status = OperationStatus.Ready;
        }
    }

    /// <summary>The work order was cancelled: the operation leaves all queues.</summary>
    internal void Cancel() => Status = OperationStatus.Skipped;
}
