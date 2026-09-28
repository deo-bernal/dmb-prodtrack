using ProdTrack.Contracts.Products;

namespace ProdTrack.Contracts.Operations;

public sealed record StationQueueItemDto(
    int OperationId,
    int WorkOrderId,
    string WorkOrderNumber,
    int Sequence,
    string ProductSku,
    string ProductName,
    int Quantity,
    string Status,
    string WorkOrderStatus,
    int Priority,
    DateOnly DueDate,
    bool IsLate,
    string? HoldReason);

public sealed record StationQueueDto(int StationId, string StationCode, string StationName, string StationType, IReadOnlyList<StationQueueItemDto> Items);

public sealed record ScrapEntryDto(int Quantity, string ReasonCode, string ReasonDescription, string? Note, DateTimeOffset OccurredAtUtc);

public sealed record InspectionSummaryDto(int Id, string Result, string Disposition, int SampleSize, string? Notes, DateTimeOffset InspectedAtUtc);

public sealed record QcChecklistItemDto(int Id, int Sequence, string Description, string Kind, decimal? MinValue, decimal? MaxValue, string? Unit);

public sealed record QcTemplateDto(int Id, string ProductType, string Name, bool IsActive, IReadOnlyList<QcChecklistItemDto> Items);

public sealed record OperationDetailDto(
    int Id,
    int WorkOrderId,
    string WorkOrderNumber,
    int Sequence,
    int StationId,
    string StationCode,
    string StationName,
    string StationType,
    string Status,
    string WorkOrderStatus,
    string? HoldReason,
    string ProductSku,
    string ProductName,
    string ProductType,
    string? Legend,
    ProductSpecDto Spec,
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
    IReadOnlyList<ScrapEntryDto> Scrap,
    InspectionSummaryDto? LatestInspection,
    QcTemplateDto? Checklist,
    bool IsInspection);

public sealed record StartOperationRequest(string? StationCode, Guid? RequestId);

public sealed record PauseOperationRequest(int ReasonCodeId, Guid? RequestId);

public sealed record ResumeOperationRequest(Guid? RequestId);

public sealed record CompleteOperationRequest(int GoodQuantity, Guid? RequestId);

public sealed record LogScrapRequest(int Quantity, int ReasonCodeId, string? Note);

public sealed record QcItemResultRequest(int ChecklistItemId, bool? Passed, decimal? MeasuredValue);

public sealed record RecordInspectionRequest(int SampleSize, IReadOnlyList<QcItemResultRequest> Items, string? Notes);

public sealed record InspectionRecordedResponse(int InspectionId, string Result, string Disposition);

/// <summary>Resolved scan (PT-028). Kind: WorkOrder|Operation|Station.</summary>
public sealed record ScanResolutionDto(string Kind, string Code, int? WorkOrderId, string? WorkOrderNumber, int? OperationId, string? StationCode);
