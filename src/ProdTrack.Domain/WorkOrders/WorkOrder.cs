using ProdTrack.Domain.Common;
using ProdTrack.Domain.Products;
using ProdTrack.Domain.Routings;

namespace ProdTrack.Domain.WorkOrders;

/// <summary>
/// Work order aggregate: spec snapshot (BR-02), lifecycle (BRD 5.5), operations created from the routing on release,
/// artwork proofs gating the release.
/// </summary>
public sealed class WorkOrder : AggregateRoot
{
    public const int NumberMaxLength = 20;
    public const int LegendMaxLength = 500;
    public const int CustomerMaxLength = 200;

    private readonly List<Operation> _operations = [];
    private readonly List<ArtworkProof> _artworkProofs = [];

    private WorkOrder()
    {
    }

    public string Number { get; private set; } = string.Empty;

    public int? SalesOrderLineId { get; private set; }

    public int ProductId { get; private set; }

    public ProductType ProductType { get; private set; }

    public bool RequiresArtworkApproval { get; private set; }

    public int? RoutingId { get; private set; }

    public WorkOrderStatus Status { get; private set; }

    public int Quantity { get; private set; }

    public int CompletedQuantity { get; private set; }

    /// <summary>1 = high, 5 = low.</summary>
    public int Priority { get; private set; }

    public DateOnly DueDate { get; private set; }

    public string? CustomerName { get; private set; }

    public string? Legend { get; private set; }

    public ProductSpec Spec { get; private set; } = new();

    public string? HoldReason { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? ReleasedAtUtc { get; private set; }

    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public IReadOnlyList<Operation> Operations => _operations;

    public IReadOnlyList<ArtworkProof> ArtworkProofs => _artworkProofs;

    public bool IsOpen => Status is not (WorkOrderStatus.Completed or WorkOrderStatus.Cancelled);

    /// <summary>Late when past due and not completed/cancelled (PT-021).</summary>
    public bool IsLate(DateOnly plantToday) => IsOpen && DueDate < plantToday;

    public static Result<WorkOrder> Create(
        string number,
        Product product,
        int quantity,
        DateOnly dueDate,
        int priority,
        string? customerName,
        string? legend,
        DateTimeOffset nowUtc)
    {
        ArgumentNullException.ThrowIfNull(product);
        ArgumentException.ThrowIfNullOrWhiteSpace(number);
        if (!product.IsActive)
        {
            return ProductErrors.Inactive(product.Sku);
        }

        var workOrder = new WorkOrder
        {
            Number = number,
            ProductId = product.Id,
            ProductType = product.ProductType,
            RequiresArtworkApproval = product.RequiresArtworkApproval,
            Spec = product.DefaultSpec,
            Status = WorkOrderStatus.Draft,
            CreatedAtUtc = nowUtc,
        };

        var result = workOrder.ApplyPlanning(quantity, dueDate, priority, customerName, legend);
        if (result.IsFailure)
        {
            return result.Error!;
        }

        workOrder.Raise(new WorkOrderCreated(number));
        return workOrder;
    }

    /// <summary>Updates planning data. Quantity, customer and legend only while Draft; due date and priority while open.</summary>
    public Result UpdatePlanning(int quantity, DateOnly dueDate, int priority, string? customerName, string? legend)
    {
        if (!IsOpen)
        {
            return WorkOrderErrors.NotEditable(Status);
        }

        var draftOnlyChanged = quantity != Quantity
            || !string.Equals(Normalize(customerName), CustomerName, StringComparison.Ordinal)
            || !string.Equals(Normalize(legend), Legend, StringComparison.Ordinal);
        if (Status != WorkOrderStatus.Draft && draftOnlyChanged)
        {
            return WorkOrderErrors.NotEditable(Status);
        }

        var result = ApplyPlanning(quantity, dueDate, priority, customerName, legend);
        if (result.IsSuccess)
        {
            Raise(new WorkOrderUpdated(Id, Number, Status));
        }

        return result;
    }

    public bool CanBeDeleted => Status == WorkOrderStatus.Draft;

    /// <summary>Version number the next uploaded artwork proof will get.</summary>
    public int NextArtworkVersion => _artworkProofs.Count == 0 ? 1 : _artworkProofs.Max(p => p.Version) + 1;

    /// <summary>Releases a Draft work order: checks the artwork gate and creates one operation per routing step (PT-020).</summary>
    public Result Release(Routing routing, DateTimeOffset nowUtc)
    {
        ArgumentNullException.ThrowIfNull(routing);
        if (Status != WorkOrderStatus.Draft)
        {
            return WorkOrderErrors.InvalidStatusTransition(Status, "release");
        }

        if (routing.ProductType != ProductType)
        {
            return WorkOrderErrors.RoutingProductTypeMismatch;
        }

        if (routing.Steps.Count == 0)
        {
            return RoutingErrors.NoSteps;
        }

        if (RequiresArtworkApproval && !_artworkProofs.Exists(p => p.Status == ArtworkProofStatus.Approved))
        {
            return WorkOrderErrors.ArtworkNotApproved;
        }

        var first = true;
        foreach (var step in routing.Steps.OrderBy(s => s.Sequence))
        {
            _operations.Add(new Operation(
                step,
                first ? OperationStatus.Ready : OperationStatus.Pending,
                first ? Quantity : 0));
            first = false;
        }

        RoutingId = routing.Id;
        Status = WorkOrderStatus.Released;
        ReleasedAtUtc = nowUtc;
        Raise(new WorkOrderReleased(Id, Number));
        return Result.Success();
    }

    /// <summary>Adds a new proof version; a previous pending proof is superseded (PT-023).</summary>
    public Result<ArtworkProof> AddArtworkProof(string fileKey, string contentType, string originalFileName, string uploadedBy, DateTimeOffset nowUtc)
    {
        if (Status != WorkOrderStatus.Draft)
        {
            return WorkOrderErrors.InvalidStatusTransition(Status, "upload artwork for");
        }

        foreach (var pending in _artworkProofs.Where(p => p.Status == ArtworkProofStatus.Pending))
        {
            pending.Supersede();
        }

        var version = NextArtworkVersion;
        var proof = new ArtworkProof(version, fileKey, contentType, originalFileName, uploadedBy, nowUtc);
        _artworkProofs.Add(proof);
        return proof;
    }

    public Result ApproveArtwork(int version, string userId, string? note, DateTimeOffset nowUtc) =>
        DecideArtwork(version, ArtworkProofStatus.Approved, userId, note, nowUtc);

    public Result RejectArtwork(int version, string userId, string reason, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            return Error.Validation("reason", "A reason is required to reject a proof.");
        }

        return DecideArtwork(version, ArtworkProofStatus.Rejected, userId, reason, nowUtc);
    }

    private Result DecideArtwork(int version, ArtworkProofStatus decision, string userId, string? note, DateTimeOffset nowUtc)
    {
        var proof = _artworkProofs.Find(p => p.Version == version);
        if (proof is null)
        {
            return WorkOrderErrors.ArtworkProofNotFound(version);
        }

        if (proof.Status != ArtworkProofStatus.Pending)
        {
            return WorkOrderErrors.ArtworkProofNotPending(version);
        }

        proof.Decide(decision, userId, string.IsNullOrWhiteSpace(note) ? null : note.Trim(), nowUtc);
        return Result.Success();
    }

    private Result ApplyPlanning(int quantity, DateOnly dueDate, int priority, string? customerName, string? legend)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        if (quantity <= 0)
        {
            errors["quantity"] = ["Quantity must be greater than 0."];
        }

        if (priority is < 1 or > 5)
        {
            errors["priority"] = ["Priority must be between 1 (high) and 5 (low)."];
        }

        var normalizedLegend = Normalize(legend);
        if (ProductType == ProductType.PipeMarker && normalizedLegend is null)
        {
            errors["legend"] = ["Legend text is required for pipe markers."];
        }
        else if (normalizedLegend?.Length > LegendMaxLength)
        {
            errors["legend"] = [$"Legend must be at most {LegendMaxLength} characters."];
        }

        var normalizedCustomer = Normalize(customerName);
        if (normalizedCustomer?.Length > CustomerMaxLength)
        {
            errors["customerName"] = [$"Customer name must be at most {CustomerMaxLength} characters."];
        }

        if (errors.Count > 0)
        {
            return new ValidationError(errors);
        }

        Quantity = quantity;
        DueDate = dueDate;
        Priority = priority;
        CustomerName = normalizedCustomer;
        Legend = normalizedLegend;
        return Result.Success();
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
