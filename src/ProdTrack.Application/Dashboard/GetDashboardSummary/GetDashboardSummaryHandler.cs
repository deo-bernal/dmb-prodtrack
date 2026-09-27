using Microsoft.EntityFrameworkCore;
using ProdTrack.Application.Abstractions;
using ProdTrack.Application.Messaging;
using ProdTrack.Domain.Common;
using ProdTrack.Domain.WorkOrders;

namespace ProdTrack.Application.Dashboard.GetDashboardSummary;

internal sealed class GetDashboardSummaryHandler(IAppDbContext db, IPlantClock clock)
    : IQueryHandler<GetDashboardSummaryQuery, DashboardSummaryModel>
{
    public async Task<Result<DashboardSummaryModel>> HandleAsync(GetDashboardSummaryQuery query, CancellationToken cancellationToken)
    {
        var today = clock.Today;
        var statusCounts = await db.WorkOrders.AsNoTracking()
            .GroupBy(w => w.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);
        var byStatus = Enum.GetValues<WorkOrderStatus>()
            .ToDictionary(s => s, s => statusCounts.FirstOrDefault(c => c.Status == s)?.Count ?? 0);

        var lateCount = await db.WorkOrders.AsNoTracking().CountAsync(
            w => w.DueDate < today && w.Status != WorkOrderStatus.Completed && w.Status != WorkOrderStatus.Cancelled,
            cancellationToken);

        var operationCounts = await db.Operations.AsNoTracking()
            .Where(o => o.Status != OperationStatus.Completed && o.Status != OperationStatus.Skipped)
            .GroupBy(o => new { o.StationId, o.Status })
            .Select(g => new { g.Key.StationId, g.Key.Status, Count = g.Count() })
            .ToListAsync(cancellationToken);
        var stations = await db.Stations.AsNoTracking().OrderBy(s => s.Code).ToListAsync(cancellationToken);

        int CountFor(int stationId, OperationStatus status) =>
            operationCounts.FirstOrDefault(c => c.StationId == stationId && c.Status == status)?.Count ?? 0;

        var wip = stations
            .Where(s => s.IsActive || operationCounts.Exists(c => c.StationId == s.Id))
            .Select(s => new StationWipModel(
                s.Id,
                s.Code,
                s.Name,
                s.IsActive,
                CountFor(s.Id, OperationStatus.Ready),
                CountFor(s.Id, OperationStatus.InProgress),
                CountFor(s.Id, OperationStatus.Paused),
                CountFor(s.Id, OperationStatus.Pending)))
            .ToList();

        return new DashboardSummaryModel(byStatus, lateCount, wip);
    }
}
