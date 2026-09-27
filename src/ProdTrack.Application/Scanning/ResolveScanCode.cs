using Microsoft.EntityFrameworkCore;
using ProdTrack.Application.Abstractions;
using ProdTrack.Application.Messaging;
using ProdTrack.Application.Security;
using ProdTrack.Domain.Common;
using ProdTrack.Domain.Stations;
using ProdTrack.Domain.WorkOrders;

namespace ProdTrack.Application.Scanning;

/// <summary>Resolves a scanned code to the entity to open on the shop floor (PT-028).</summary>
[RequiresPolicy(Policies.ExecuteOperations)]
public sealed record ResolveScanCodeQuery(string Code) : IQuery<ScanResolutionModel>;

public sealed record ScanResolutionModel(
    ScanKind Kind,
    string Code,
    int? WorkOrderId,
    string? WorkOrderNumber,
    int? OperationId,
    string? StationCode);

internal sealed class ResolveScanCodeHandler(IAppDbContext db) : IQueryHandler<ResolveScanCodeQuery, ScanResolutionModel>
{
    public async Task<Result<ScanResolutionModel>> HandleAsync(ResolveScanCodeQuery query, CancellationToken cancellationToken)
    {
        var parsed = ScanCodeParser.Parse(query.Code);
        if (parsed.IsFailure)
        {
            return parsed.Error!;
        }

        var code = parsed.Value;
        switch (code.Kind)
        {
            case ScanKind.Station:
                var station = await db.Stations.AsNoTracking().FirstOrDefaultAsync(s => s.Code == code.Value, cancellationToken);
                return station is null
                    ? StationErrors.NotFoundByCode(code.Value)
                    : new ScanResolutionModel(ScanKind.Station, query.Code, null, null, null, station.Code);

            case ScanKind.WorkOrder:
                var wo = await db.WorkOrders.AsNoTracking().Where(w => w.Number == code.Value).Select(w => new { w.Id, w.Number }).FirstOrDefaultAsync(cancellationToken);
                return wo is null
                    ? WorkOrderErrors.NotFoundByNumber(code.Value)
                    : new ScanResolutionModel(ScanKind.WorkOrder, query.Code, wo.Id, wo.Number, null, null);

            default:
                var op = await db.Operations.AsNoTracking()
                    .Join(db.WorkOrders.AsNoTracking(), o => o.WorkOrderId, w => w.Id, (o, w) => new { o.Id, o.Sequence, o.StationId, WorkOrderId = w.Id, w.Number })
                    .Join(db.Stations.AsNoTracking(), x => x.StationId, s => s.Id, (x, s) => new { x.Id, x.Sequence, x.WorkOrderId, x.Number, s.Code })
                    .FirstOrDefaultAsync(x => x.Number == code.Value && x.Sequence == code.Sequence, cancellationToken);
                return op is null
                    ? Error.NotFound("Operation.NotFound", $"Operation {code.Sequence} of {code.Value} was not found (is the work order released?).")
                    : new ScanResolutionModel(ScanKind.Operation, query.Code, op.WorkOrderId, op.Number, op.Id, op.Code);
        }
    }
}
