using Microsoft.EntityFrameworkCore;
using ProdTrack.Application.Abstractions;
using ProdTrack.Application.Messaging;
using ProdTrack.Domain.Common;
using ProdTrack.Domain.Routings;
using ProdTrack.Domain.WorkOrders;

namespace ProdTrack.Application.WorkOrders.ReleaseWorkOrder;

internal sealed class ReleaseWorkOrderHandler(IAppDbContext db, TimeProvider timeProvider) : ICommandHandler<ReleaseWorkOrderCommand, Unit>
{
    public async Task<Result<Unit>> HandleAsync(ReleaseWorkOrderCommand command, CancellationToken cancellationToken)
    {
        var workOrder = await db.WorkOrders
            .Include(w => w.ArtworkProofs)
            .Include(w => w.Operations)
            .FirstOrDefaultAsync(w => w.Id == command.Id, cancellationToken);
        if (workOrder is null)
        {
            return WorkOrderErrors.NotFound(command.Id);
        }

        var routing = await db.Routings
            .Include(r => r.Steps)
            .Where(r => r.ProductType == workOrder.ProductType && r.IsCurrent)
            .OrderByDescending(r => r.Version)
            .FirstOrDefaultAsync(cancellationToken);
        if (routing is null)
        {
            return RoutingErrors.NoCurrentRouting(workOrder.ProductType.ToString());
        }

        var result = workOrder.Release(routing, timeProvider.GetUtcNow());
        if (result.IsFailure)
        {
            return result.Error!;
        }

        await db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
