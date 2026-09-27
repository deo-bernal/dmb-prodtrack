using ProdTrack.Application.Operations;
using ProdTrack.Application.Quality;
using ProdTrack.Application.SalesOrders;
using ProdTrack.Application.Scanning;
using ProdTrack.Application.Users;
using ProdTrack.Application.WorkOrders.Traveler;
using ProdTrack.Contracts.Operations;
using ProdTrack.Contracts.SalesOrders;
using ProdTrack.Contracts.Users;
using ProdTrack.Contracts.WorkOrders;
using ProdTrack.Server.Errors;

namespace ProdTrack.Server.Endpoints;

internal static partial class ContractMappings
{
    public static SalesOrderSummaryDto ToDto(this SalesOrderSummaryModel m) =>
        new(m.Id, m.Number, m.CustomerName, m.PoNumber, m.DueDate, m.Status.ToString(), m.LineCount, m.WorkOrderCount);

    public static SalesOrderDetailDto ToDto(this SalesOrderDetailModel m) => new(
        m.Id,
        m.Number,
        m.CustomerName,
        m.PoNumber,
        m.DueDate,
        m.Status.ToString(),
        m.CreatedAtUtc,
        [.. m.Lines.Select(l => new SalesOrderLineDto(
            l.Id, l.LineNumber, l.ProductId, l.ProductSku, l.ProductName, l.ProductType.ToString(), l.Quantity, l.Legend, l.Spec.ToDto(), l.WorkOrderId, l.WorkOrderNumber, l.WorkOrderStatus))],
        ETags.Format(m.Version) ?? string.Empty);

    public static StationQueueDto ToDto(this StationQueueModel m) => new(
        m.StationId,
        m.StationCode,
        m.StationName,
        m.StationType.ToString(),
        [.. m.Items.Select(i => new StationQueueItemDto(
            i.OperationId, i.WorkOrderId, i.WorkOrderNumber, i.Sequence, i.ProductSku, i.ProductName, i.Quantity, i.Status.ToString(), i.WorkOrderStatus.ToString(), i.Priority, i.DueDate, i.IsLate, i.HoldReason))]);

    public static QcTemplateDto ToDto(this QcTemplateModel m) => new(
        m.Id,
        m.ProductType.ToString(),
        m.Name,
        m.IsActive,
        [.. m.Items.Select(i => new QcChecklistItemDto(i.Id, i.Sequence, i.Description, i.Kind.ToString(), i.MinValue, i.MaxValue, i.Unit))]);

    public static OperationDetailDto ToDto(this OperationDetailModel m) => new(
        m.Id,
        m.WorkOrderId,
        m.WorkOrderNumber,
        m.Sequence,
        m.StationId,
        m.StationCode,
        m.StationName,
        m.StationType.ToString(),
        m.Status.ToString(),
        m.WorkOrderStatus.ToString(),
        m.HoldReason,
        m.ProductSku,
        m.ProductName,
        m.ProductType.ToString(),
        m.Legend,
        m.Spec.ToDto(),
        m.InputQuantity,
        m.GoodQuantity,
        m.ScrapQuantity,
        m.WorkOrderQuantity,
        m.DueDate,
        m.CanStart,
        m.StartBlockedReason,
        m.StartedAtUtc,
        m.StartedBy,
        m.CompletedAtUtc,
        [.. m.Scrap.Select(s => new ScrapEntryDto(s.Quantity, s.ReasonCode, s.ReasonDescription, s.Note, s.OccurredAtUtc))],
        m.LatestInspection is null
            ? null
            : new InspectionSummaryDto(m.LatestInspection.Id, m.LatestInspection.Result.ToString(), m.LatestInspection.Disposition.ToString(), m.LatestInspection.SampleSize, m.LatestInspection.Notes, m.LatestInspection.InspectedAtUtc),
        m.Checklist?.ToDto(),
        m.IsInspection);

    public static ScanResolutionDto ToDto(this ScanResolutionModel m) =>
        new(m.Kind.ToString(), m.Code, m.WorkOrderId, m.WorkOrderNumber, m.OperationId, m.StationCode);

    public static UserSummaryDto ToDto(this UserSummaryModel m) =>
        new(m.Id, m.Email, m.DisplayName, m.EmployeeNumber, m.BadgeNumber, m.HomeStationCode, m.IsActive, m.IsLockedOut, m.MustChangePassword, m.Roles);

    public static UserDetailDto ToDto(this UserDetailModel m) =>
        new(m.Id, m.Email, m.DisplayName, m.EmployeeNumber, m.BadgeNumber, m.HomeStationCode, m.IsActive, m.IsLockedOut, m.MustChangePassword, m.Roles, m.Version);

    public static TravelerDto ToDto(this TravelerModel m) => new(
        m.WorkOrder.ToDto(),
        m.HeaderPayload,
        m.HeaderQrSvg,
        [.. m.Operations.Select(o => new TravelerOperationDto(o.Sequence, o.StationCode, o.StationName, o.SetupMinutes, o.StdMinutesPerUnit, o.Payload, o.QrSvg))],
        m.PrintedAtPlantTime);
}
