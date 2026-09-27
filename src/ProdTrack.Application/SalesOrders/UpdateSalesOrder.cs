using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProdTrack.Application.Abstractions;
using ProdTrack.Application.Messaging;
using ProdTrack.Application.Security;
using ProdTrack.Domain.Common;
using ProdTrack.Domain.SalesOrders;

namespace ProdTrack.Application.SalesOrders;

/// <summary>Updates header and lines of an Open order; lines that already have a work order are locked.</summary>
[RequiresPolicy(Policies.PlanWorkOrders)]
public sealed record UpdateSalesOrderCommand(
    int Id,
    string CustomerName,
    string? PoNumber,
    DateOnly DueDate,
    IReadOnlyList<SalesOrderLineInput> Lines,
    byte[]? ExpectedVersion = null) : ICommand<SalesOrderDetailModel>;

internal sealed class UpdateSalesOrderValidator : AbstractValidator<UpdateSalesOrderCommand>
{
    public UpdateSalesOrderValidator()
    {
        RuleFor(c => c.CustomerName).NotEmpty().MaximumLength(SalesOrder.CustomerMaxLength);
        RuleFor(c => c.PoNumber).MaximumLength(SalesOrder.PoMaxLength);
        RuleFor(c => c.Lines).NotEmpty().WithMessage("A sales order needs at least one line.");
    }
}

internal sealed class UpdateSalesOrderHandler(IAppDbContext db) : ICommandHandler<UpdateSalesOrderCommand, SalesOrderDetailModel>
{
    public async Task<Result<SalesOrderDetailModel>> HandleAsync(UpdateSalesOrderCommand command, CancellationToken cancellationToken)
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

        var (definitions, error) = await SalesOrderLoader.ToDefinitionsAsync(db, command.Lines, cancellationToken);
        if (error is not null)
        {
            return error;
        }

        var lineIds = order.Lines.Select(l => (int?)l.Id).ToList();
        var locked = (await db.WorkOrders.AsNoTracking()
                .Where(w => lineIds.Contains(w.SalesOrderLineId))
                .Select(w => w.SalesOrderLineId!.Value)
                .ToListAsync(cancellationToken))
            .ToHashSet();

        var result = order.Update(command.CustomerName, command.PoNumber, command.DueDate, definitions!, locked);
        if (result.IsFailure)
        {
            return result.Error!;
        }

        await db.SaveChangesAsync(cancellationToken);
        return (await SalesOrderLoader.LoadAsync(db, order.Id, cancellationToken))!;
    }
}
