using ProdTrack.Application.Quality;
using ProdTrack.Domain.Products;
using ProdTrack.Domain.Quality;
using ProdTrack.Domain.Stations;
using ProdTrack.Domain.WorkOrders;

namespace ProdTrack.Application.Operations;

public sealed record StationQueueItemModel(
    int OperationId,
    int WorkOrderId,
    string WorkOrderNumber,
    int Sequence,
    string ProductSku,
    string ProductName,
    int Quantity,
    OperationStatus Status,
    WorkOrderStatus WorkOrderStatus,
    int Priority,
    DateOnly DueDate,
    bool IsLate,
    string? HoldReason);

/// <summary>Station queue (PT-030): Ready, In Progress and Paused operations ordered by priority and due date.</summary>
public sealed record StationQueueModel(int StationId, string StationCode, string StationName, StationType StationType, IReadOnlyList<StationQueueItemModel> Items);

public sealed record ScrapEntryModel(int Quantity, string ReasonCode, string ReasonDescription, string? Note, DateTimeOffset OccurredAtUtc);

public sealed record InspectionSummaryModel(int Id, QcResult Result, QcDisposition Disposition, int SampleSize, string? Notes, DateTimeOffset InspectedAtUtc);

/// <summary>Everything the shop-floor operation screen needs (PT-031..PT-036).</summary>
public sealed record OperationDetailModel(
    int Id,
    int WorkOrderId,
    string WorkOrderNumber,
    int Sequence,
    int StationId,
    string StationCode,
    string StationName,
    StationType StationType,
    OperationStatus Status,
    WorkOrderStatus WorkOrderStatus,
    string? HoldReason,
    string ProductSku,
    string ProductName,
    ProductType ProductType,
    string? Legend,
    ProductSpec Spec,
    int InputQuantity,
    int GoodQuantity,
    int ScrapQuantity,
    int WorkOrderQuantity,
    DateOnly DueDate,
    bool CanStart,
    string? StartBlockedReason,
    DateTimeOffset? StartedAtUtc,
    string? StartedBy,
    DateTimeOffset? CompletedAtUtc,
    IReadOnlyList<ScrapEntryModel> Scrap,
    InspectionSummaryModel? LatestInspection,
    QcTemplateModel? Checklist)
{
    public bool IsInspection => StationType == StationType.Inspection;
}
