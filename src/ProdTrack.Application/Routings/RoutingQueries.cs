using Microsoft.EntityFrameworkCore;
using ProdTrack.Application.Abstractions;
using ProdTrack.Domain.Routings;

namespace ProdTrack.Application.Routings;

internal static class RoutingQueries
{
    public static async Task<List<RoutingModel>> LoadAsync(IAppDbContext db, IQueryable<Routing> routings, CancellationToken cancellationToken)
    {
        var items = await routings.AsNoTracking()
            .Include(r => r.Steps)
            .ToListAsync(cancellationToken);
        var stationIds = items.SelectMany(r => r.Steps).Select(s => s.StationId).Distinct().ToList();
        var stations = await db.Stations.AsNoTracking()
            .Where(s => stationIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, cancellationToken);

        return [.. items
            .OrderBy(r => r.ProductType).ThenByDescending(r => r.Version)
            .Select(r => new RoutingModel(
                r.Id,
                r.ProductType,
                r.Version,
                r.Name,
                r.IsCurrent,
                r.CreatedAtUtc,
                [.. r.Steps.OrderBy(s => s.Sequence).Select(s => new RoutingStepModel(
                    s.Sequence,
                    s.StationId,
                    stations.TryGetValue(s.StationId, out var st) ? st.Code : "?",
                    stations.TryGetValue(s.StationId, out var st2) ? st2.Name : "?",
                    s.SetupMinutes,
                    s.StdMinutesPerUnit,
                    s.AllowOverlap))]))];
    }
}
