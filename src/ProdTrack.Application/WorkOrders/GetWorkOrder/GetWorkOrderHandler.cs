using ProdTrack.Application.Abstractions;
using ProdTrack.Application.Messaging;
using ProdTrack.Domain.Common;
using ProdTrack.Domain.WorkOrders;

namespace ProdTrack.Application.WorkOrders.GetWorkOrder;

internal sealed class GetWorkOrderHandler(IAppDbContext db, IPlantClock clock)
    : IQueryHandler<GetWorkOrderQuery, WorkOrderDetailModel>,
      IQueryHandler<GetWorkOrderByNumberQuery, WorkOrderDetailModel>
{
    public async Task<Result<WorkOrderDetailModel>> HandleAsync(GetWorkOrderQuery query, CancellationToken cancellationToken)
    {
        var model = await WorkOrderDetailLoader.LoadAsync(db, clock, db.WorkOrders.Where(w => w.Id == query.Id), cancellationToken);
        return model is null ? WorkOrderErrors.NotFound(query.Id) : model;
    }

    public async Task<Result<WorkOrderDetailModel>> HandleAsync(GetWorkOrderByNumberQuery query, CancellationToken cancellationToken)
    {
        var number = (query.Number ?? string.Empty).Trim().ToUpperInvariant();
        var model = await WorkOrderDetailLoader.LoadAsync(db, clock, db.WorkOrders.Where(w => w.Number == number), cancellationToken);
        return model is null ? WorkOrderErrors.NotFoundByNumber(number) : model;
    }
}
