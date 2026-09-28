using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProdTrack.Application.Abstractions;
using ProdTrack.Application.Messaging;
using ProdTrack.Application.Security;
using ProdTrack.Domain.Common;
using ProdTrack.Domain.Quality;
using ProdTrack.Domain.Stations;
using ProdTrack.Domain.WorkOrders;

namespace ProdTrack.Application.Operations;

/// <summary>Starts an operation (PT-031). <see cref="StationCode"/> is the tablet's station; a mismatch is rejected.</summary>
[RequiresPolicy(Policies.ExecuteOperations)]
public sealed record StartOperationCommand(int OperationId, string? StationCode = null, Guid? RequestId = null) : ICommand<Unit>;

[RequiresPolicy(Policies.ExecuteOperations)]
public sealed record PauseOperationCommand(int OperationId, int ReasonCodeId, Guid? RequestId = null) : ICommand<Unit>;

[RequiresPolicy(Policies.ExecuteOperations)]
public sealed record ResumeOperationCommand(int OperationId, Guid? RequestId = null) : ICommand<Unit>;

/// <summary>Completes an operation with its good quantity (PT-033). Inspection stations need a passed inspection first.</summary>
[RequiresPolicy(Policies.ExecuteOperations)]
public sealed record CompleteOperationCommand(int OperationId, int GoodQuantity, Guid? RequestId = null) : ICommand<Unit>;

/// <summary>Logs scrapped units with a Scrap reason code and optional note (PT-034).</summary>
[RequiresPolicy(Policies.ExecuteOperations)]
public sealed record LogScrapCommand(int OperationId, int Quantity, int ReasonCodeId, string? Note) : ICommand<Unit>;

internal sealed class PauseOperationValidator : AbstractValidator<PauseOperationCommand>
{
    public PauseOperationValidator() => RuleFor(c => c.ReasonCodeId).GreaterThan(0).WithMessage("Choose a pause reason.");
}

internal sealed class CompleteOperationValidator : AbstractValidator<CompleteOperationCommand>
{
    public CompleteOperationValidator() => RuleFor(c => c.GoodQuantity).GreaterThanOrEqualTo(0);
}

internal sealed class LogScrapValidator : AbstractValidator<LogScrapCommand>
{
    public LogScrapValidator()
    {
        RuleFor(c => c.Quantity).GreaterThan(0);
        RuleFor(c => c.ReasonCodeId).GreaterThan(0).WithMessage("Choose a scrap reason.");
        RuleFor(c => c.Note).MaximumLength(Operation.NoteMaxLength);
    }
}

internal sealed class OperationCommandsHandler(IAppDbContext db, ICurrentUser currentUser, TimeProvider timeProvider)
    : ICommandHandler<StartOperationCommand, Unit>,
      ICommandHandler<PauseOperationCommand, Unit>,
      ICommandHandler<ResumeOperationCommand, Unit>,
      ICommandHandler<CompleteOperationCommand, Unit>,
      ICommandHandler<LogScrapCommand, Unit>
{
    private string UserId => currentUser.UserId ?? "unknown";

    private string UserName => currentUser.UserName ?? UserId;

    public async Task<Result<Unit>> HandleAsync(StartOperationCommand command, CancellationToken cancellationToken)
    {
        if (command.RequestId is { } requestId && await db.OperationEvents.AnyAsync(e => e.RequestId == requestId, cancellationToken))
        {
            return Unit.Value; // idempotent retry from a flaky tablet connection
        }

        var workOrder = await LoadAsync(command.OperationId, cancellationToken);
        if (workOrder is null)
        {
            return OperationErrors.NotFound(command.OperationId);
        }

        var operation = workOrder.FindOperation(command.OperationId)!;
        if (!string.IsNullOrWhiteSpace(command.StationCode))
        {
            var expected = await db.Stations.Where(s => s.Id == operation.StationId).Select(s => s.Code).FirstAsync(cancellationToken);
            if (!string.Equals(expected, Station.NormalizeCode(command.StationCode), StringComparison.Ordinal))
            {
                return OperationErrors.WrongStation(expected);
            }
        }

        return await SaveAsync(workOrder.StartOperation(command.OperationId, UserName, timeProvider.GetUtcNow(), command.RequestId), cancellationToken);
    }

    public async Task<Result<Unit>> HandleAsync(PauseOperationCommand command, CancellationToken cancellationToken)
    {
        var reason = await db.ReasonCodes.FirstOrDefaultAsync(r => r.Id == command.ReasonCodeId, cancellationToken);
        if (reason is null)
        {
            return Error.Validation("reasonCodeId", "Choose a pause reason.");
        }

        var workOrder = await LoadAsync(command.OperationId, cancellationToken);
        return workOrder is null
            ? OperationErrors.NotFound(command.OperationId)
            : await SaveAsync(workOrder.PauseOperation(command.OperationId, reason, UserId, timeProvider.GetUtcNow(), command.RequestId), cancellationToken);
    }

    public async Task<Result<Unit>> HandleAsync(ResumeOperationCommand command, CancellationToken cancellationToken)
    {
        var workOrder = await LoadAsync(command.OperationId, cancellationToken);
        return workOrder is null
            ? OperationErrors.NotFound(command.OperationId)
            : await SaveAsync(workOrder.ResumeOperation(command.OperationId, UserId, timeProvider.GetUtcNow(), command.RequestId), cancellationToken);
    }

    public async Task<Result<Unit>> HandleAsync(CompleteOperationCommand command, CancellationToken cancellationToken)
    {
        var workOrder = await LoadAsync(command.OperationId, cancellationToken);
        if (workOrder is null)
        {
            return OperationErrors.NotFound(command.OperationId);
        }

        var operation = workOrder.FindOperation(command.OperationId)!;
        var stationType = await db.Stations.Where(s => s.Id == operation.StationId).Select(s => s.Type).FirstAsync(cancellationToken);
        if (stationType == StationType.Inspection)
        {
            var latest = await db.QcInspections.AsNoTracking()
                .Where(i => i.OperationId == operation.Id)
                .OrderByDescending(i => i.Id)
                .Select(i => (QcResult?)i.Result)
                .FirstOrDefaultAsync(cancellationToken);
            if (latest != QcResult.Passed)
            {
                return OperationErrors.InspectionRequired;
            }
        }

        return await SaveAsync(workOrder.CompleteOperation(command.OperationId, command.GoodQuantity, UserId, timeProvider.GetUtcNow(), command.RequestId), cancellationToken);
    }

    public async Task<Result<Unit>> HandleAsync(LogScrapCommand command, CancellationToken cancellationToken)
    {
        var reason = await db.ReasonCodes.FirstOrDefaultAsync(r => r.Id == command.ReasonCodeId, cancellationToken);
        if (reason is null)
        {
            return Error.Validation("reasonCodeId", "Choose a scrap reason.");
        }

        var workOrder = await LoadAsync(command.OperationId, cancellationToken);
        return workOrder is null
            ? OperationErrors.NotFound(command.OperationId)
            : await SaveAsync(workOrder.LogScrap(command.OperationId, command.Quantity, reason, command.Note, UserId, timeProvider.GetUtcNow()), cancellationToken);
    }

    private async Task<WorkOrder?> LoadAsync(int operationId, CancellationToken cancellationToken)
    {
        var workOrderId = await db.Operations.Where(o => o.Id == operationId).Select(o => (int?)o.WorkOrderId).FirstOrDefaultAsync(cancellationToken);
        return workOrderId is null
            ? null
            : await db.WorkOrders.Include(w => w.Operations).FirstOrDefaultAsync(w => w.Id == workOrderId, cancellationToken);
    }

    private async Task<Result<Unit>> SaveAsync(Result result, CancellationToken cancellationToken)
    {
        if (result.IsFailure)
        {
            return result.Error!;
        }

        await db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
