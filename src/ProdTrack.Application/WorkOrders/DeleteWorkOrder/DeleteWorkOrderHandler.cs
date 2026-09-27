using ProdTrack.Application.Abstractions;
using ProdTrack.Application.Messaging;
using ProdTrack.Domain.Common;
using ProdTrack.Domain.WorkOrders;

namespace ProdTrack.Application.WorkOrders.DeleteWorkOrder;

internal sealed class DeleteWorkOrderHandler(IAppDbContext db) : ICommandHandler<DeleteWorkOrderCommand, Unit>
{
    public async Task<Result<Unit>> HandleAsync(DeleteWorkOrderCommand command, CancellationToken cancellationToken)
    {
        var workOrder = await db.WorkOrders.FindAsync([command.Id], cancellationToken);
        if (workOrder is null)
        {
            return WorkOrderErrors.NotFound(command.Id);
        }

        if (!workOrder.CanBeDeleted)
        {
            return WorkOrderErrors.InvalidStatusTransition(workOrder.Status, "delete");
        }

        db.WorkOrders.Remove(workOrder);
        await db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
