using ProdTrack.Application.Abstractions;
using ProdTrack.Application.Messaging;
using ProdTrack.Domain.Common;
using ProdTrack.Domain.WorkOrders;

namespace ProdTrack.Application.WorkOrders.UpdateWorkOrder;

internal sealed class UpdateWorkOrderHandler(IAppDbContext db) : ICommandHandler<UpdateWorkOrderCommand, Unit>
{
    public async Task<Result<Unit>> HandleAsync(UpdateWorkOrderCommand command, CancellationToken cancellationToken)
    {
        var workOrder = await db.WorkOrders.FindAsync([command.Id], cancellationToken);
        if (workOrder is null)
        {
            return WorkOrderErrors.NotFound(command.Id);
        }

        if (!ConcurrencyErrors.Matches(workOrder.RowVersion, command.ExpectedVersion))
        {
            return ConcurrencyErrors.StaleVersion;
        }

        var result = workOrder.UpdatePlanning(command.Quantity, command.DueDate, command.Priority, command.CustomerName, command.Legend);
        if (result.IsFailure)
        {
            return result.Error!;
        }

        await db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
