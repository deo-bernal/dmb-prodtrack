using ProdTrack.Application.Abstractions;
using ProdTrack.Application.Messaging;
using ProdTrack.Domain.Common;

namespace ProdTrack.Application.Routings.GetRoutings;

internal sealed class GetRoutingsHandler(IAppDbContext db) : IQueryHandler<GetRoutingsQuery, IReadOnlyList<RoutingModel>>
{
    public async Task<Result<IReadOnlyList<RoutingModel>>> HandleAsync(GetRoutingsQuery query, CancellationToken cancellationToken)
    {
        var routings = db.Routings
            .Where(r => query.ProductType == null || r.ProductType == query.ProductType)
            .Where(r => !query.CurrentOnly || r.IsCurrent);
        return await RoutingQueries.LoadAsync(db, routings, cancellationToken);
    }
}
