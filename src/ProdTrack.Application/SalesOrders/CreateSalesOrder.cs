using FluentValidation;
using ProdTrack.Application.Abstractions;
using ProdTrack.Application.Messaging;
using ProdTrack.Application.Security;
using ProdTrack.Domain.Common;
using ProdTrack.Domain.SalesOrders;

namespace ProdTrack.Application.SalesOrders;

/// <summary>Enters a customer sales order with lines; it gets number SO-yyyy-nnnnn and status Open (PT-018).</summary>
[RequiresPolicy(Policies.PlanWorkOrders)]
public sealed record CreateSalesOrderCommand(string CustomerName, string? PoNumber, DateOnly DueDate, IReadOnlyList<SalesOrderLineInput> Lines)
    : ICommand<SalesOrderCreatedModel>;

internal sealed class CreateSalesOrderValidator : AbstractValidator<CreateSalesOrderCommand>
{
    public CreateSalesOrderValidator()
    {
        RuleFor(c => c.CustomerName).NotEmpty().MaximumLength(SalesOrder.CustomerMaxLength);
        RuleFor(c => c.PoNumber).MaximumLength(SalesOrder.PoMaxLength);
        RuleFor(c => c.Lines).NotEmpty().WithMessage("A sales order needs at least one line.");
    }
}

internal sealed class CreateSalesOrderHandler(IAppDbContext db, INumberSequenceGenerator numbers, TimeProvider timeProvider)
    : ICommandHandler<CreateSalesOrderCommand, SalesOrderCreatedModel>
{
    public async Task<Result<SalesOrderCreatedModel>> HandleAsync(CreateSalesOrderCommand command, CancellationToken cancellationToken)
    {
        var (definitions, error) = await SalesOrderLoader.ToDefinitionsAsync(db, command.Lines, cancellationToken);
        if (error is not null)
        {
            return error;
        }

        var number = await numbers.NextAsync(NumberSequenceKind.SalesOrder, cancellationToken);
        var created = SalesOrder.Create(number, command.CustomerName, command.PoNumber, command.DueDate, definitions!, timeProvider.GetUtcNow());
        if (created.IsFailure)
        {
            return created.Error!;
        }

        db.SalesOrders.Add(created.Value);
        await db.SaveChangesAsync(cancellationToken);
        return new SalesOrderCreatedModel(created.Value.Id, created.Value.Number);
    }
}
