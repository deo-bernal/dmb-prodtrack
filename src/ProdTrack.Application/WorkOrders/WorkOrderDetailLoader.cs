using Microsoft.EntityFrameworkCore;
using ProdTrack.Application.Abstractions;
using ProdTrack.Domain.WorkOrders;

namespace ProdTrack.Application.WorkOrders;

internal static class WorkOrderDetailLoader
{
    public static async Task<WorkOrderDetailModel?> LoadAsync(
        IAppDbContext db,
        IPlantClock clock,
        IQueryable<WorkOrder> query,
        CancellationToken cancellationToken)
    {
        var workOrder = await query.AsNoTracking()
            .Include(w => w.Operations)
            .Include(w => w.ArtworkProofs)
            .FirstOrDefaultAsync(cancellationToken);
        if (workOrder is null)
        {
            return null;
        }

        var product = await db.Products.AsNoTracking()
            .Where(p => p.Id == workOrder.ProductId)
            .Select(p => new { p.Sku, p.Name })
            .FirstAsync(cancellationToken);
        var stationIds = workOrder.Operations.Select(o => o.StationId).Distinct().ToList();
        var stations = await db.Stations.AsNoTracking()
            .Where(s => stationIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => new { s.Code, s.Name }, cancellationToken);
        int? routingVersion = workOrder.RoutingId is null
            ? null
            : await db.Routings.AsNoTracking().Where(r => r.Id == workOrder.RoutingId).Select(r => (int?)r.Version).FirstOrDefaultAsync(cancellationToken);

        var salesOrder = workOrder.SalesOrderLineId is null
            ? null
            : await db.SalesOrders.AsNoTracking()
                .Where(s => s.Lines.Any(l => l.Id == workOrder.SalesOrderLineId))
                .Select(s => new { s.Id, s.Number })
                .FirstOrDefaultAsync(cancellationToken);

        return new WorkOrderDetailModel(
            workOrder.Id,
            workOrder.Number,
            workOrder.ProductId,
            product.Sku,
            product.Name,
            workOrder.ProductType,
            workOrder.CustomerName,
            workOrder.Legend,
            workOrder.Status,
            workOrder.Quantity,
            workOrder.CompletedQuantity,
            workOrder.Priority,
            workOrder.DueDate,
            workOrder.IsLate(clock.Today),
            workOrder.RequiresArtworkApproval,
            workOrder.Spec,
            workOrder.RoutingId,
            routingVersion,
            workOrder.CreatedAtUtc,
            workOrder.ReleasedAtUtc,
            [.. workOrder.Operations.OrderBy(o => o.Sequence).Select(o => new OperationModel(
                o.Id,
                o.Sequence,
                o.StationId,
                stations.GetValueOrDefault(o.StationId)?.Code ?? "?",
                o.Status,
                o.InputQuantity,
                o.GoodQuantity,
                o.ScrapQuantity,
                stations.GetValueOrDefault(o.StationId)?.Name,
                o.StartedAtUtc,
                o.CompletedAtUtc,
                o.StartedBy))],
            [.. workOrder.ArtworkProofs.OrderByDescending(p => p.Version).Select(p => new ArtworkProofModel(
                p.Version, p.OriginalFileName, p.ContentType, p.Status, p.UploadedBy, p.UploadedAtUtc, p.DecidedBy, p.DecidedAtUtc, p.DecisionNote))],
            workOrder.HoldReason,
            workOrder.CancelReason,
            salesOrder?.Id,
            salesOrder?.Number,
            workOrder.CompletedAtUtc,
            workOrder.RowVersion);
    }
}
