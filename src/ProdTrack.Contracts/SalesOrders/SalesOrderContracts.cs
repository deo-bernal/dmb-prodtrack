using ProdTrack.Contracts.Products;

namespace ProdTrack.Contracts.SalesOrders;

/// <summary>Row in the sales order list. Status: Open|Closed|Cancelled.</summary>
public sealed record SalesOrderSummaryDto(int Id, string Number, string CustomerName, string? PoNumber, DateOnly DueDate, string Status, int LineCount, int WorkOrderCount);

public sealed record SalesOrderLineDto(
    int Id,
    int LineNumber,
    int ProductId,
    string ProductSku,
    string ProductName,
    string ProductType,
    int Quantity,
    string? Legend,
    ProductSpecDto Spec,
    int? WorkOrderId,
    string? WorkOrderNumber,
    string? WorkOrderStatus);

/// <summary>Sales order with lines. <see cref="Version"/> is the base64 rowversion, also sent as the ETag header.</summary>
public sealed record SalesOrderDetailDto(
    int Id,
    string Number,
    string CustomerName,
    string? PoNumber,
    DateOnly DueDate,
    string Status,
    DateTimeOffset CreatedAtUtc,
    IReadOnlyList<SalesOrderLineDto> Lines,
    string Version);

/// <summary>Line input: omit <see cref="LineId"/> for a new line; <see cref="Spec"/> overrides the catalog spec for this line only.</summary>
public sealed record SalesOrderLineRequest(int? LineId, int ProductId, int Quantity, string? Legend, ProductSpecDto? Spec);

public sealed record CreateSalesOrderRequest(string CustomerName, string? PoNumber, DateOnly DueDate, IReadOnlyList<SalesOrderLineRequest> Lines);

public sealed record UpdateSalesOrderRequest(string CustomerName, string? PoNumber, DateOnly DueDate, IReadOnlyList<SalesOrderLineRequest> Lines);

public sealed record SalesOrderCreatedResponse(int Id, string Number);

public sealed record GenerateWorkOrdersRequest(int? Priority, IReadOnlyList<int>? LineIds);

public sealed record GeneratedWorkOrderDto(int LineNumber, int WorkOrderId, string WorkOrderNumber);

public sealed record SkippedLineDto(int LineNumber, string WorkOrderNumber);

public sealed record GenerateWorkOrdersResponse(IReadOnlyList<GeneratedWorkOrderDto> Created, IReadOnlyList<SkippedLineDto> Skipped);
