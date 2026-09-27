using ProdTrack.Contracts.Products;

namespace ProdTrack.Contracts.WorkOrders;

/// <summary>Row in the work order list. Status: Draft|Released|InProgress|OnHold|Completed|Cancelled.</summary>
public sealed record WorkOrderSummaryDto(
    int Id,
    string Number,
    string ProductSku,
    string ProductName,
    string? CustomerName,
    string Status,
    int Quantity,
    int CompletedQuantity,
    int Priority,
    DateOnly DueDate,
    bool IsLate);

public sealed record OperationDto(
    int Id,
    int Sequence,
    int StationId,
    string StationCode,
    string Status,
    int InputQuantity,
    int GoodQuantity,
    int ScrapQuantity,
    string? StationName = null,
    DateTimeOffset? StartedAtUtc = null,
    DateTimeOffset? CompletedAtUtc = null,
    string? StartedBy = null);

public sealed record ArtworkProofDto(
    int Version,
    string OriginalFileName,
    string ContentType,
    string Status,
    string UploadedBy,
    DateTimeOffset UploadedAtUtc,
    string? DecidedBy,
    DateTimeOffset? DecidedAtUtc,
    string? DecisionNote);

public sealed record WorkOrderDetailDto(
    int Id,
    string Number,
    int ProductId,
    string ProductSku,
    string ProductName,
    string ProductType,
    string? CustomerName,
    string? Legend,
    string Status,
    int Quantity,
    int CompletedQuantity,
    int Priority,
    DateOnly DueDate,
    bool IsLate,
    bool RequiresArtworkApproval,
    ProductSpecDto Spec,
    int? RoutingId,
    int? RoutingVersion,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ReleasedAtUtc,
    IReadOnlyList<OperationDto> Operations,
    IReadOnlyList<ArtworkProofDto> ArtworkProofs,
    string? HoldReason = null,
    string? CancelReason = null,
    int? SalesOrderId = null,
    string? SalesOrderNumber = null,
    string? Version = null);

public sealed record CreateWorkOrderRequest(int ProductId, int Quantity, DateOnly DueDate, int Priority, string? CustomerName, string? Legend);

public sealed record UpdateWorkOrderRequest(int Quantity, DateOnly DueDate, int Priority, string? CustomerName, string? Legend);

public sealed record WorkOrderCreatedResponse(int Id, string Number);

public sealed record ApproveArtworkRequest(string? Note);

public sealed record RejectArtworkRequest(string Reason);

public sealed record HoldWorkOrderRequest(int ReasonCodeId, string? Note);

public sealed record CancelWorkOrderRequest(string Reason);

public sealed record TravelerOperationDto(int Sequence, string StationCode, string StationName, decimal SetupMinutes, decimal StdMinutesPerUnit, string Payload, string QrSvg);

/// <summary>Printable traveler: header QR (WO number) plus one QR per operation (OP:number:seq).</summary>
public sealed record TravelerDto(WorkOrderDetailDto WorkOrder, string HeaderPayload, string HeaderQrSvg, IReadOnlyList<TravelerOperationDto> Operations, DateTimeOffset PrintedAtPlantTime);
