using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProdTrack.Application.Abstractions;
using ProdTrack.Application.Messaging;
using ProdTrack.Application.Security;
using ProdTrack.Domain.Common;
using ProdTrack.Domain.SalesOrders;
using ProdTrack.Domain.WorkOrders;

namespace ProdTrack.Application.SalesOrders;

/// <summary>
/// Creates one Draft work order per sales order line (PT-019). Lines that already have a work order are skipped and
/// reported. <see cref="LineIds"/> limits generation to the given lines (all lines when null or empty).
/// </summary>
[RequiresPolicy(Policies.PlanWorkOrders)]
public sealed record CreateWorkOrdersFromSalesOrderCommand(int SalesOrderId, int Priority = 3, IReadOnlyList<int>? LineIds = null)
    : ICommand<WorkOrdersGeneratedModel>;

internal sealed class CreateWorkOrdersFromSalesOrderValidator : AbstractValidator<CreateWorkOrdersFromSalesOrderCommand>
{
    public CreateWorkOrdersFromSalesOrderValidator() => RuleFor(c => c.Priority).InclusiveBetween(1, 5);
}

internal sealed class CreateWorkOrdersFromSalesOrderHandler(IAppDbContext db, INumberSequenceGenerator numbers, TimeProvider timeProvider)
    : ICommandHandler<CreateWorkOrdersFromSalesOrderCommand, WorkOrdersGeneratedModel>
{
    public async Task<Result<WorkOrdersGeneratedModel>> HandleAsync(CreateWorkOrdersFromSalesOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await db.SalesOrders.Include(s => s.Lines).FirstOrDefaultAsync(s => s.Id == command.SalesOrderId, cancellationToken);
        if (order is null)
        {
            return SalesOrderErrors.NotFound(command.SalesOrderId);
        }

        if (order.Status != SalesOrderStatus.Open)
        {
            return SalesOrderErrors.NotOpen(order.Status);
        }

        var lines = order.Lines
            .Where(l => command.LineIds is null || command.LineIds.Count == 0 || command.LineIds.Contains(l.Id))
            .OrderBy(l => l.LineNumber)
            .ToList();
        var lineIds = lines.Select(l => (int?)l.Id).ToList();
        var existing = await db.WorkOrders.AsNoTracking()
            .Where(w => lineIds.Contains(w.SalesOrderLineId))
            .Select(w => new { LineId = w.SalesOrderLineId!.Value, w.Number })
            .ToListAsync(cancellationToken);
        var productIds = lines.Select(l => l.ProductId).Distinct().ToList();
        var products = await db.Products.Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, cancellationToken);

        var now = timeProvider.GetUtcNow();
        var created = new List<(SalesOrderLine Line, WorkOrder WorkOrder)>();
        var skipped = new List<SkippedLineModel>();
        foreach (var line in lines)
        {
            var already = existing.FirstOrDefault(e => e.LineId == line.Id);
            if (already is not null)
            {
                skipped.Add(new SkippedLineModel(line.LineNumber, already.Number));
                continue;
            }

            var number = await numbers.NextAsync(NumberSequenceKind.WorkOrder, cancellationToken);
            var workOrder = WorkOrder.CreateFromSalesOrderLine(number, products[line.ProductId], order, line, command.Priority, now);
            if (workOrder.IsFailure)
            {
                return workOrder.Error!;
            }

            db.WorkOrders.Add(workOrder.Value);
            created.Add((line, workOrder.Value));
        }

        if (created.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        return new WorkOrdersGeneratedModel(
            [.. created.Select(c => new GeneratedWorkOrderModel(c.Line.LineNumber, c.WorkOrder.Id, c.WorkOrder.Number))],
            skipped);
    }
}
