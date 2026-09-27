using ProdTrack.Application.Abstractions;
using ProdTrack.Application.Messaging;
using ProdTrack.Domain.Common;
using ProdTrack.Domain.Products;
using ProdTrack.Domain.WorkOrders;

namespace ProdTrack.Application.WorkOrders.CreateWorkOrder;

internal sealed class CreateWorkOrderHandler(IAppDbContext db, INumberSequenceGenerator numbers, TimeProvider timeProvider)
    : ICommandHandler<CreateWorkOrderCommand, WorkOrderCreatedModel>
{
    public async Task<Result<WorkOrderCreatedModel>> HandleAsync(CreateWorkOrderCommand command, CancellationToken cancellationToken)
    {
        var product = await db.Products.FindAsync([command.ProductId], cancellationToken);
        if (product is null)
        {
            return Error.Validation("productId", ProductErrors.NotFound(command.ProductId).Message);
        }

        var number = await numbers.NextAsync(NumberSequenceKind.WorkOrder, cancellationToken);
        var created = WorkOrder.Create(
            number,
            product,
            command.Quantity,
            command.DueDate,
            command.Priority,
            command.CustomerName,
            command.Legend,
            timeProvider.GetUtcNow());
        if (created.IsFailure)
        {
            return created.Error!;
        }

        db.WorkOrders.Add(created.Value);
        await db.SaveChangesAsync(cancellationToken);
        return new WorkOrderCreatedModel(created.Value.Id, created.Value.Number);
    }
}
