using ProdTrack.Domain.Products;
using ProdTrack.Domain.SalesOrders;

namespace ProdTrack.Application.SalesOrders;

public sealed record SalesOrderSummaryModel(
    int Id,
    string Number,
    string CustomerName,
    string? PoNumber,
    DateOnly DueDate,
    SalesOrderStatus Status,
    int LineCount,
    int WorkOrderCount);

public sealed record SalesOrderLineModel(
    int Id,
    int LineNumber,
    int ProductId,
    string ProductSku,
    string ProductName,
    ProductType ProductType,
    int Quantity,
    string? Legend,
    ProductSpec Spec,
    int? WorkOrderId,
    string? WorkOrderNumber,
    string? WorkOrderStatus);

public sealed record SalesOrderDetailModel(
    int Id,
    string Number,
    string CustomerName,
    string? PoNumber,
    DateOnly DueDate,
    SalesOrderStatus Status,
    DateTimeOffset CreatedAtUtc,
    IReadOnlyList<SalesOrderLineModel> Lines,
    byte[] Version);

/// <summary>Line input: <see cref="LineId"/> null for a new line; <see cref="SpecOverride"/> null keeps the catalog spec.</summary>
public sealed record SalesOrderLineInput(int? LineId, int ProductId, int Quantity, string? Legend, ProductSpec? SpecOverride = null);

public sealed record SalesOrderCreatedModel(int Id, string Number);

public sealed record GeneratedWorkOrderModel(int LineNumber, int WorkOrderId, string WorkOrderNumber);

public sealed record SkippedLineModel(int LineNumber, string WorkOrderNumber);

/// <summary>Result of PT-019: created work orders and lines skipped because they already have one.</summary>
public sealed record WorkOrdersGeneratedModel(IReadOnlyList<GeneratedWorkOrderModel> Created, IReadOnlyList<SkippedLineModel> Skipped);
