using ProdTrack.Application.Abstractions;
using ProdTrack.Application.Messaging;
using ProdTrack.Domain.Common;
using ProdTrack.Domain.Routings;

namespace ProdTrack.Application.Routings.GetRouting;

internal sealed class GetRoutingHandler(IAppDbContext db) : IQueryHandler<GetRoutingQuery, RoutingModel>
{
    public async Task<Result<RoutingModel>> HandleAsync(GetRoutingQuery query, CancellationToken cancellationToken)
    {
        var items = await RoutingQueries.LoadAsync(db, db.Routings.Where(r => r.Id == query.Id), cancellationToken);
        return items.Count == 0 ? RoutingErrors.NotFound(query.Id) : items[0];
    }
}
