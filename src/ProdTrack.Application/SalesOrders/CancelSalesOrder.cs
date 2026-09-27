using Microsoft.EntityFrameworkCore;
using ProdTrack.Application.Abstractions;
using ProdTrack.Application.Messaging;
using ProdTrack.Application.Security;
using ProdTrack.Domain.Common;
using ProdTrack.Domain.SalesOrders;
using ProdTrack.Domain.WorkOrders;

namespace ProdTrack.Application.SalesOrders;

/// <summary>Cancels an Open sales order that has no open work orders.</summary>
[RequiresPolicy(Policies.PlanWorkOrders)]
public sealed record CancelSalesOrderCommand(int Id, byte[]? ExpectedVersion = null) : ICommand<Unit>;

internal sealed class CancelSalesOrderHandler(IAppDbContext db) : ICommandHandler<CancelSalesOrderCommand, Unit>
{
    public async Task<Result<Unit>> HandleAsync(CancelSalesOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await db.SalesOrders.Include(s => s.Lines).FirstOrDefaultAsync(s => s.Id == command.Id, cancellationToken);
        if (order is null)
        {
            return SalesOrderErrors.NotFound(command.Id);
        }

        if (!ConcurrencyErrors.Matches(order.RowVersion, command.ExpectedVersion))
        {
            return ConcurrencyErrors.StaleVersion;
        }

        var lineIds = order.Lines.Select(l => (int?)l.Id).ToList();
        var hasOpen = await db.WorkOrders.AnyAsync(
            w => lineIds.Contains(w.SalesOrderLineId) && w.Status != WorkOrderStatus.Cancelled,
            cancellationToken);
        var result = order.Cancel(hasOpen);
        if (result.IsFailure)
        {
            return result.Error!;
        }

        await db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
