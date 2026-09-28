using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProdTrack.Application.Abstractions;
using ProdTrack.Application.Messaging;
using ProdTrack.Application.Security;
using ProdTrack.Domain.Common;
using ProdTrack.Domain.Quality;
using ProdTrack.Domain.ReasonCodes;
using ProdTrack.Domain.Stations;
using ProdTrack.Domain.WorkOrders;

namespace ProdTrack.Application.Quality;

/// <summary>
/// Records a QC inspection on a running inspection operation (PT-036). Passed: the operation completes with
/// input minus scrap as good quantity and the next step becomes Ready. Failed: the work order goes on hold with reason
/// QC-FAIL (PT-037).
/// </summary>
[RequiresPolicy(Policies.RecordInspections)]
public sealed record RecordInspectionCommand(int OperationId, int SampleSize, IReadOnlyList<QcItemResultInput> Items, string? Notes)
    : ICommand<InspectionRecordedModel>;

internal sealed class RecordInspectionValidator : AbstractValidator<RecordInspectionCommand>
{
    public RecordInspectionValidator()
    {
        RuleFor(c => c.SampleSize).GreaterThan(0);
        RuleFor(c => c.Notes).MaximumLength(QcInspection.NotesMaxLength);
    }
}

internal sealed class RecordInspectionHandler(IAppDbContext db, ICurrentUser currentUser, TimeProvider timeProvider)
    : ICommandHandler<RecordInspectionCommand, InspectionRecordedModel>
{
    public const string QcFailReasonCode = "QC-FAIL";

    public async Task<Result<InspectionRecordedModel>> HandleAsync(RecordInspectionCommand command, CancellationToken cancellationToken)
    {
        var workOrderId = await db.Operations.Where(o => o.Id == command.OperationId).Select(o => (int?)o.WorkOrderId).FirstOrDefaultAsync(cancellationToken);
        if (workOrderId is null)
        {
            return OperationErrors.NotFound(command.OperationId);
        }

        var workOrder = await db.WorkOrders.Include(w => w.Operations).FirstAsync(w => w.Id == workOrderId, cancellationToken);
        var operation = workOrder.FindOperation(command.OperationId)!;
        var stationType = await db.Stations.Where(s => s.Id == operation.StationId).Select(s => s.Type).FirstAsync(cancellationToken);
        if (stationType != StationType.Inspection)
        {
            return Error.BusinessRule("Qc.NotInspectionOperation", "Inspections can only be recorded on QC station operations.");
        }

        if (workOrder.Status == WorkOrderStatus.OnHold)
        {
            return WorkOrderErrors.OnHold(workOrder.HoldReason);
        }

        if (operation.Status != OperationStatus.InProgress)
        {
            return OperationErrors.InvalidTransition(operation.Status, "inspect");
        }

        var template = await db.QcChecklistTemplates.Include(t => t.Items)
            .Where(t => t.ProductType == workOrder.ProductType && t.IsActive)
            .OrderByDescending(t => t.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (template is null)
        {
            return Error.BusinessRule("Qc.NoTemplate", $"No active QC checklist exists for {workOrder.ProductType}.");
        }

        var now = timeProvider.GetUtcNow();
        var userId = currentUser.UserId ?? "unknown";
        var recorded = QcInspection.Record(operation.Id, template, command.Items, command.SampleSize, userId, command.Notes, now);
        if (recorded.IsFailure)
        {
            return recorded.Error!;
        }

        var inspection = recorded.Value;
        db.QcInspections.Add(inspection);
        Result follow;
        if (inspection.Result == QcResult.Passed)
        {
            follow = workOrder.CompleteOperation(operation.Id, operation.InputQuantity - operation.ScrapQuantity, userId, now);
        }
        else
        {
            var reason = await db.ReasonCodes.FirstOrDefaultAsync(r => r.Code == QcFailReasonCode && r.Category == ReasonCategory.Hold, cancellationToken);
            if (reason is null)
            {
                return Error.BusinessRule("Qc.NoHoldReason", $"Reason code {QcFailReasonCode} (Hold) is missing.");
            }

            follow = workOrder.Hold(reason, $"QC inspection failed at operation {operation.Sequence}");
        }

        if (follow.IsFailure)
        {
            return follow.Error!;
        }

        await db.SaveChangesAsync(cancellationToken);
        return new InspectionRecordedModel(inspection.Id, inspection.Result, inspection.Disposition);
    }
}
