using ProdTrack.Application.Dashboard;
using ProdTrack.Application.Products;
using ProdTrack.Application.ReasonCodes;
using ProdTrack.Application.Reference;
using ProdTrack.Application.Routings;
using ProdTrack.Application.Stations;
using ProdTrack.Application.WorkOrders;
using ProdTrack.Contracts.Dashboard;
using ProdTrack.Contracts.Products;
using ProdTrack.Contracts.ReasonCodes;
using ProdTrack.Contracts.Reference;
using ProdTrack.Contracts.Routings;
using ProdTrack.Contracts.Stations;
using ProdTrack.Contracts.WorkOrders;
using ProdTrack.Domain.Products;
using ProdTrack.Server.Errors;

namespace ProdTrack.Server.Endpoints;

/// <summary>Hand-written mapping between Application models and wire contracts (docs/02 section 4.1).</summary>
internal static partial class ContractMappings
{
    public static StationDto ToDto(this StationModel m) => new(m.Id, m.Code, m.Name, m.Type.ToString(), m.WorkCenter, m.IsActive, ETags.Format(m.Version));

    public static ReasonCodeDto ToDto(this ReasonCodeModel m) => new(m.Id, m.Code, m.Description, m.Category.ToString(), m.IsActive, ETags.Format(m.Version));

    public static ReferenceItemDto ToDto(this ReferenceItemModel m) => new(m.Code, m.Name, m.TextColor, m.BackgroundColor);

    public static ProductDto ToDto(this ProductModel m) =>
        new(m.Id, m.Sku, m.Name, m.ProductType.ToString(), m.RequiresArtworkApproval, m.IsActive, m.Spec.ToDto(), ETags.Format(m.Version));

    public static ProductSpecDto ToDto(this ProductSpec s) => new()
    {
        Material = s.Material,
        WidthMm = s.WidthMm,
        HeightMm = s.HeightMm,
        Mounting = s.Mounting,
        Standard = s.Standard,
        ColorSchemeCode = s.ColorSchemeCode,
        PipeOdRange = s.PipeOdRange,
        LetterHeightMm = s.LetterHeightMm,
        TagShape = s.TagShape,
        DiameterMm = s.DiameterMm,
        ThicknessMm = s.ThicknessMm,
        HoleSizeMm = s.HoleSizeMm,
        SignalWord = s.SignalWord,
        Adhesive = s.Adhesive,
        Finish = s.Finish,
    };

    public static ProductSpec ToDomain(this ProductSpecDto? s) => s is null ? new ProductSpec() : new ProductSpec
    {
        Material = s.Material,
        WidthMm = s.WidthMm,
        HeightMm = s.HeightMm,
        Mounting = s.Mounting,
        Standard = s.Standard,
        ColorSchemeCode = s.ColorSchemeCode,
        PipeOdRange = s.PipeOdRange,
        LetterHeightMm = s.LetterHeightMm,
        TagShape = s.TagShape,
        DiameterMm = s.DiameterMm,
        ThicknessMm = s.ThicknessMm,
        HoleSizeMm = s.HoleSizeMm,
        SignalWord = s.SignalWord,
        Adhesive = s.Adhesive,
        Finish = s.Finish,
    };

    public static RoutingDto ToDto(this RoutingModel m) => new(
        m.Id,
        m.ProductType.ToString(),
        m.Version,
        m.Name,
        m.IsCurrent,
        m.CreatedAtUtc,
        [.. m.Steps.Select(s => new RoutingStepDto(s.Sequence, s.StationId, s.StationCode, s.StationName, s.SetupMinutes, s.StdMinutesPerUnit, s.AllowOverlap))]);

    public static WorkOrderSummaryDto ToDto(this WorkOrderSummaryModel m) => new(
        m.Id, m.Number, m.ProductSku, m.ProductName, m.CustomerName, m.Status.ToString(), m.Quantity, m.CompletedQuantity, m.Priority, m.DueDate, m.IsLate);

    public static ArtworkProofDto ToDto(this ArtworkProofModel p) => new(
        p.Version, p.OriginalFileName, p.ContentType, p.Status.ToString(), p.UploadedBy, p.UploadedAtUtc, p.DecidedBy, p.DecidedAtUtc, p.DecisionNote);

    public static WorkOrderDetailDto ToDto(this WorkOrderDetailModel m) => new(
        m.Id,
        m.Number,
        m.ProductId,
        m.ProductSku,
        m.ProductName,
        m.ProductType.ToString(),
        m.CustomerName,
        m.Legend,
        m.Status.ToString(),
        m.Quantity,
        m.CompletedQuantity,
        m.Priority,
        m.DueDate,
        m.IsLate,
        m.RequiresArtworkApproval,
        m.Spec.ToDto(),
        m.RoutingId,
        m.RoutingVersion,
        m.CreatedAtUtc,
        m.ReleasedAtUtc,
        [.. m.Operations.Select(o => new OperationDto(o.Id, o.Sequence, o.StationId, o.StationCode, o.Status.ToString(), o.InputQuantity, o.GoodQuantity, o.ScrapQuantity, o.StationName, o.StartedAtUtc, o.CompletedAtUtc, o.StartedBy))],
        [.. m.ArtworkProofs.Select(p => p.ToDto())],
        m.HoldReason,
        m.CancelReason,
        m.SalesOrderId,
        m.SalesOrderNumber,
        ETags.Format(m.Version));

    public static DashboardSummaryDto ToDto(this DashboardSummaryModel m) => new(
        m.WorkOrdersByStatus.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value, StringComparer.Ordinal),
        m.LateCount,
        [.. m.WipByStation.Select(s => new StationWipDto(s.StationId, s.StationCode, s.StationName, s.IsActive, s.Ready, s.InProgress, s.Paused, s.Pending))],
        m.CompletedToday,
        m.ScrapUnitsToday,
        m.GoodUnitsToday);
}
