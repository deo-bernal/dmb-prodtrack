using Microsoft.EntityFrameworkCore;
using ProdTrack.Application.Abstractions;
using ProdTrack.Application.Messaging;
using ProdTrack.Application.Quality;
using ProdTrack.Application.Security;
using ProdTrack.Domain.Common;
using ProdTrack.Domain.Stations;
using ProdTrack.Domain.WorkOrders;

namespace ProdTrack.Application.Operations;

[RequiresPolicy(Policies.ReadAll)]
public sealed record GetStationQueueQuery(string StationCode) : IQuery<StationQueueModel>;

[RequiresPolicy(Policies.ReadAll)]
public sealed record GetOperationQuery(int OperationId) : IQuery<OperationDetailModel>;

internal sealed class OperationQueriesHandler(IAppDbContext db, IPlantClock clock)
    : IQueryHandler<GetStationQueueQuery, StationQueueModel>,
      IQueryHandler<GetOperationQuery, OperationDetailModel>
{
    public async Task<Result<StationQueueModel>> HandleAsync(GetStationQueueQuery query, CancellationToken cancellationToken)
    {
        var code = Station.NormalizeCode(query.StationCode);
        var station = await db.Stations.AsNoTracking().FirstOrDefaultAsync(s => s.Code == code, cancellationToken);
        if (station is null)
        {
            return StationErrors.NotFoundByCode(code);
        }

        var today = clock.Today;
        var rows = await db.Operations.AsNoTracking()
            .Where(o => o.StationId == station.Id
                && (o.Status == OperationStatus.Ready || o.Status == OperationStatus.InProgress || o.Status == OperationStatus.Paused))
            .Join(
                db.WorkOrders.AsNoTracking().Where(w => w.Status == WorkOrderStatus.Released || w.Status == WorkOrderStatus.InProgress || w.Status == WorkOrderStatus.OnHold),
                o => o.WorkOrderId,
                w => w.Id,
                (o, w) => new { Operation = o, WorkOrder = w })
            .Join(db.Products.AsNoTracking(), x => x.WorkOrder.ProductId, p => p.Id, (x, p) => new { x.Operation, x.WorkOrder, p.Sku, p.Name })
            .OrderBy(x => x.WorkOrder.Priority).ThenBy(x => x.WorkOrder.DueDate).ThenBy(x => x.WorkOrder.Number)
            .Select(x => new StationQueueItemModel(
                x.Operation.Id,
                x.WorkOrder.Id,
                x.WorkOrder.Number,
                x.Operation.Sequence,
                x.Sku,
                x.Name,
                x.Operation.InputQuantity,
                x.Operation.Status,
                x.WorkOrder.Status,
                x.WorkOrder.Priority,
                x.WorkOrder.DueDate,
                x.WorkOrder.DueDate < today,
                x.WorkOrder.HoldReason))
            .ToListAsync(cancellationToken);

        return new StationQueueModel(station.Id, station.Code, station.Name, station.Type, rows);
    }

    public async Task<Result<OperationDetailModel>> HandleAsync(GetOperationQuery query, CancellationToken cancellationToken)
    {
        var workOrderId = await db.Operations.AsNoTracking().Where(o => o.Id == query.OperationId).Select(o => (int?)o.WorkOrderId).FirstOrDefaultAsync(cancellationToken);
        if (workOrderId is null)
        {
            return OperationErrors.NotFound(query.OperationId);
        }

        var workOrder = await db.WorkOrders.AsNoTracking().Include(w => w.Operations).FirstAsync(w => w.Id == workOrderId, cancellationToken);
        var operation = workOrder.Operations.First(o => o.Id == query.OperationId);
        var station = await db.Stations.AsNoTracking().FirstAsync(s => s.Id == operation.StationId, cancellationToken);
        var product = await db.Products.AsNoTracking().Where(p => p.Id == workOrder.ProductId).Select(p => new { p.Sku, p.Name }).FirstAsync(cancellationToken);

        var scrap = await db.ScrapRecords.AsNoTracking()
            .Where(s => s.OperationId == operation.Id)
            .Join(db.ReasonCodes.AsNoTracking(), s => s.ReasonCodeId, r => r.Id, (s, r) => new ScrapEntryModel(s.Quantity, r.Code, r.Description, s.Note, s.OccurredAtUtc))
            .ToListAsync(cancellationToken);
        var inspections = await db.QcInspections.AsNoTracking()
            .Where(i => i.OperationId == operation.Id)
            .Select(i => new InspectionSummaryModel(i.Id, i.Result, i.Disposition, i.SampleSize, i.Notes, i.InspectedAtUtc))
            .ToListAsync(cancellationToken);
        var latest = inspections.OrderByDescending(i => i.InspectedAtUtc).ThenByDescending(i => i.Id).FirstOrDefault();

        QcTemplateModel? checklist = null;
        if (station.Type == StationType.Inspection)
        {
            checklist = await QcTemplateQueries.ActiveForAsync(db, workOrder.ProductType, cancellationToken);
        }

        var (canStart, blocked) = StartCheck(workOrder, operation);
        return new OperationDetailModel(
            operation.Id,
            workOrder.Id,
            workOrder.Number,
            operation.Sequence,
            station.Id,
            station.Code,
            station.Name,
            station.Type,
            operation.Status,
            workOrder.Status,
            workOrder.HoldReason,
            product.Sku,
            product.Name,
            workOrder.ProductType,
            workOrder.Legend,
            workOrder.Spec,
            operation.InputQuantity,
            operation.GoodQuantity,
            operation.ScrapQuantity,
            workOrder.Quantity,
            workOrder.DueDate,
            canStart,
            blocked,
            operation.StartedAtUtc,
            operation.StartedBy,
            operation.CompletedAtUtc,
            scrap,
            latest,
            checklist);
    }

    private static (bool CanStart, string? Reason) StartCheck(WorkOrder workOrder, Operation operation)
    {
        if (workOrder.Status == WorkOrderStatus.OnHold)
        {
            return (false, $"On hold: {workOrder.HoldReason}");
        }

        if (workOrder.Status is not (WorkOrderStatus.Released or WorkOrderStatus.InProgress))
        {
            return (false, $"The work order is {workOrder.Status}.");
        }

        if (operation.Status == OperationStatus.Ready)
        {
            return (true, null);
        }

        if (operation.Status == OperationStatus.Pending)
        {
            var previous = workOrder.Operations.Where(o => o.Sequence < operation.Sequence).OrderByDescending(o => o.Sequence).FirstOrDefault();
            var overlap = operation.AllowOverlap && previous is { Status: OperationStatus.InProgress or OperationStatus.Paused or OperationStatus.Completed };
            return overlap ? (true, null) : (false, "The previous operation is not complete yet.");
        }

        return (false, null);
    }
}
