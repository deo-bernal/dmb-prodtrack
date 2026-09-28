using ProdTrack.Domain.Products;
using ProdTrack.Domain.WorkOrders;

namespace ProdTrack.Application.WorkOrders;

public sealed record WorkOrderSummaryModel(
    int Id,
    string Number,
    string ProductSku,
    string ProductName,
    string? CustomerName,
    WorkOrderStatus Status,
    int Quantity,
    int CompletedQuantity,
    int Priority,
    DateOnly DueDate,
    bool IsLate);

public sealed record WorkOrderDetailModel(
    int Id,
    string Number,
    int ProductId,
    string ProductSku,
    string ProductName,
    ProductType ProductType,
    string? CustomerName,
    string? Legend,
    WorkOrderStatus Status,
    int Quantity,
    int CompletedQuantity,
    int Priority,
    DateOnly DueDate,
    bool IsLate,
    bool RequiresArtworkApproval,
    ProductSpec Spec,
    int? RoutingId,
    int? RoutingVersion,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ReleasedAtUtc,
    IReadOnlyList<OperationModel> Operations,
    IReadOnlyList<ArtworkProofModel> ArtworkProofs,
    string? HoldReason = null,
    string? CancelReason = null,
    int? SalesOrderId = null,
    string? SalesOrderNumber = null,
    DateTimeOffset? CompletedAtUtc = null,
    byte[]? Version = null);

public sealed record OperationModel(
    int Id,
    int Sequence,
    int StationId,
    string StationCode,
    OperationStatus Status,
    int InputQuantity,
    int GoodQuantity,
    int ScrapQuantity,
    string? StationName = null,
    DateTimeOffset? StartedAtUtc = null,
    DateTimeOffset? CompletedAtUtc = null,
    string? StartedBy = null);

public sealed record ArtworkProofModel(
    int Version,
    string OriginalFileName,
    string ContentType,
    ArtworkProofStatus Status,
    string UploadedBy,
    DateTimeOffset UploadedAtUtc,
    string? DecidedBy,
    DateTimeOffset? DecidedAtUtc,
    string? DecisionNote);

/// <summary>Result of creating a work order.</summary>
public sealed record WorkOrderCreatedModel(int Id, string Number);
