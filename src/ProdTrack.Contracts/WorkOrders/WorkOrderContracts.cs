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

public sealed record OperationDto(int Id, int Sequence, int StationId, string StationCode, string Status, int InputQuantity, int GoodQuantity, int ScrapQuantity);

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
    IReadOnlyList<ArtworkProofDto> ArtworkProofs);

public sealed record CreateWorkOrderRequest(int ProductId, int Quantity, DateOnly DueDate, int Priority, string? CustomerName, string? Legend);

public sealed record UpdateWorkOrderRequest(int Quantity, DateOnly DueDate, int Priority, string? CustomerName, string? Legend);

public sealed record WorkOrderCreatedResponse(int Id, string Number);

public sealed record ApproveArtworkRequest(string? Note);

public sealed record RejectArtworkRequest(string Reason);
