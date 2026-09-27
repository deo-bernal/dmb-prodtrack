using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProdTrack.Application.Abstractions;
using ProdTrack.Application.Messaging;
using ProdTrack.Application.Security;
using ProdTrack.Domain.Common;
using ProdTrack.Domain.WorkOrders;

namespace ProdTrack.Application.WorkOrders.HoldResumeCancel;

/// <summary>Puts a work order on hold with a Hold reason code and optional note (PT-022).</summary>
[RequiresPolicy(Policies.ControlWorkOrders)]
public sealed record HoldWorkOrderCommand(int Id, int ReasonCodeId, string? Note, byte[]? ExpectedVersion = null) : ICommand<Unit>;

[RequiresPolicy(Policies.ControlWorkOrders)]
public sealed record ResumeWorkOrderCommand(int Id, byte[]? ExpectedVersion = null) : ICommand<Unit>;

/// <summary>Cancels a work order without completed operations; a reason is required (PT-022).</summary>
[RequiresPolicy(Policies.ControlWorkOrders)]
public sealed record CancelWorkOrderCommand(int Id, string Reason, byte[]? ExpectedVersion = null) : ICommand<Unit>;

internal sealed class HoldWorkOrderValidator : AbstractValidator<HoldWorkOrderCommand>
{
    public HoldWorkOrderValidator()
    {
        RuleFor(c => c.ReasonCodeId).GreaterThan(0).WithMessage("Choose a hold reason.");
        RuleFor(c => c.Note).MaximumLength(WorkOrder.ReasonMaxLength);
    }
}

internal sealed class CancelWorkOrderValidator : AbstractValidator<CancelWorkOrderCommand>
{
    public CancelWorkOrderValidator() => RuleFor(c => c.Reason).NotEmpty().MaximumLength(WorkOrder.ReasonMaxLength);
}

internal sealed class WorkOrderControlHandler(IAppDbContext db)
    : ICommandHandler<HoldWorkOrderCommand, Unit>,
      ICommandHandler<ResumeWorkOrderCommand, Unit>,
      ICommandHandler<CancelWorkOrderCommand, Unit>
{
    public async Task<Result<Unit>> HandleAsync(HoldWorkOrderCommand command, CancellationToken cancellationToken)
    {
        var reason = await db.ReasonCodes.FirstOrDefaultAsync(r => r.Id == command.ReasonCodeId, cancellationToken);
        if (reason is null)
        {
            return Error.Validation("reasonCodeId", "Choose a hold reason.");
        }

        return await ApplyAsync(command.Id, command.ExpectedVersion, w => w.Hold(reason, command.Note), cancellationToken);
    }

    public Task<Result<Unit>> HandleAsync(ResumeWorkOrderCommand command, CancellationToken cancellationToken) =>
        ApplyAsync(command.Id, command.ExpectedVersion, w => w.Resume(), cancellationToken);

    public Task<Result<Unit>> HandleAsync(CancelWorkOrderCommand command, CancellationToken cancellationToken) =>
        ApplyAsync(command.Id, command.ExpectedVersion, w => w.Cancel(command.Reason), cancellationToken);

    private async Task<Result<Unit>> ApplyAsync(int id, byte[]? expectedVersion, Func<WorkOrder, Result> action, CancellationToken cancellationToken)
    {
        var workOrder = await db.WorkOrders.Include(w => w.Operations).FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
        if (workOrder is null)
        {
            return WorkOrderErrors.NotFound(id);
        }

        if (!ConcurrencyErrors.Matches(workOrder.RowVersion, expectedVersion))
        {
            return ConcurrencyErrors.StaleVersion;
        }

        var result = action(workOrder);
        if (result.IsFailure)
        {
            return result.Error!;
        }

        await db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
