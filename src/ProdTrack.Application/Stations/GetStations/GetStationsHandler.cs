using Microsoft.EntityFrameworkCore;
using ProdTrack.Application.Abstractions;
using ProdTrack.Application.Messaging;
using ProdTrack.Domain.Common;

namespace ProdTrack.Application.Stations.GetStations;

internal sealed class GetStationsHandler(IAppDbContext db) : IQueryHandler<GetStationsQuery, IReadOnlyList<StationModel>>
{
    public async Task<Result<IReadOnlyList<StationModel>>> HandleAsync(GetStationsQuery query, CancellationToken cancellationToken)
    {
        var stations = await db.Stations.AsNoTracking()
            .Where(s => query.IncludeInactive || s.IsActive)
            .OrderBy(s => s.Code)
            .Select(s => new StationModel(s.Id, s.Code, s.Name, s.Type, s.WorkCenter, s.IsActive))
            .ToListAsync(cancellationToken);
        return stations;
    }
}
