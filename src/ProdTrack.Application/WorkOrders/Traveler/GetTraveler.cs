using Microsoft.EntityFrameworkCore;
using ProdTrack.Application.Abstractions;
using ProdTrack.Application.Messaging;
using ProdTrack.Application.Scanning;
using ProdTrack.Application.Security;
using ProdTrack.Domain.Common;
using ProdTrack.Domain.WorkOrders;

namespace ProdTrack.Application.WorkOrders.Traveler;

/// <summary>Printable job traveler with QR codes: header WO:{number}, one OP:{number}:{seq} per operation (PT-025).</summary>
[RequiresPolicy(Policies.ControlWorkOrders)]
public sealed record GetTravelerQuery(int WorkOrderId) : IQuery<TravelerModel>;

public sealed record TravelerOperationModel(int Sequence, string StationCode, string StationName, decimal SetupMinutes, decimal StdMinutesPerUnit, string Payload, string QrSvg);

public sealed record TravelerModel(
    WorkOrderDetailModel WorkOrder,
    string HeaderPayload,
    string HeaderQrSvg,
    IReadOnlyList<TravelerOperationModel> Operations,
    DateTimeOffset PrintedAtPlantTime);

internal sealed class GetTravelerHandler(IAppDbContext db, IPlantClock clock, IQrCodeRenderer qr) : IQueryHandler<GetTravelerQuery, TravelerModel>
{
    public async Task<Result<TravelerModel>> HandleAsync(GetTravelerQuery query, CancellationToken cancellationToken)
    {
        var detail = await WorkOrderDetailLoader.LoadAsync(db, clock, db.WorkOrders.Where(w => w.Id == query.WorkOrderId), cancellationToken);
        if (detail is null)
        {
            return WorkOrderErrors.NotFound(query.WorkOrderId);
        }

        if (detail.Status is WorkOrderStatus.Draft or WorkOrderStatus.Cancelled)
        {
            return Error.BusinessRule("Traveler.NotReleased", "Release the work order before printing its traveler.");
        }

        var steps = await db.Operations.AsNoTracking()
            .Where(o => o.WorkOrderId == detail.Id)
            .OrderBy(o => o.Sequence)
            .Select(o => new { o.Sequence, o.StationId, o.SetupMinutes, o.StdMinutesPerUnit })
            .ToListAsync(cancellationToken);
        var header = ScanCodeParser.WorkOrderPayload(detail.Number);
        var operations = steps.Select(s =>
        {
            var op = detail.Operations.First(o => o.Sequence == s.Sequence);
            var payload = ScanCodeParser.OperationPayload(detail.Number, s.Sequence);
            return new TravelerOperationModel(s.Sequence, op.StationCode, op.StationName ?? op.StationCode, s.SetupMinutes, s.StdMinutesPerUnit, payload, qr.RenderSvg(payload));
        }).ToList();

        return new TravelerModel(detail, header, qr.RenderSvg(header), operations, clock.ToPlantTime(clock.UtcNow));
    }
}
