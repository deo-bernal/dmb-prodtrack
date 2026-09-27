using Microsoft.EntityFrameworkCore;
using ProdTrack.Application.Abstractions;
using ProdTrack.Application.Messaging;
using ProdTrack.Domain.Common;
using ProdTrack.Domain.WorkOrders;

namespace ProdTrack.Application.WorkOrders.Artwork;

internal sealed class DecideArtworkProofHandler(IAppDbContext db, ICurrentUser currentUser, TimeProvider timeProvider)
    : ICommandHandler<ApproveArtworkProofCommand, Unit>,
      ICommandHandler<RejectArtworkProofCommand, Unit>
{
    public Task<Result<Unit>> HandleAsync(ApproveArtworkProofCommand command, CancellationToken cancellationToken) =>
        DecideAsync(command.WorkOrderId, (w, user, now) => w.ApproveArtwork(command.Version, user, command.Note, now), cancellationToken);

    public Task<Result<Unit>> HandleAsync(RejectArtworkProofCommand command, CancellationToken cancellationToken) =>
        DecideAsync(command.WorkOrderId, (w, user, now) => w.RejectArtwork(command.Version, user, command.Reason, now), cancellationToken);

    private async Task<Result<Unit>> DecideAsync(
        int workOrderId,
        Func<WorkOrder, string, DateTimeOffset, Result> decide,
        CancellationToken cancellationToken)
    {
        var workOrder = await db.WorkOrders.Include(w => w.ArtworkProofs)
            .FirstOrDefaultAsync(w => w.Id == workOrderId, cancellationToken);
        if (workOrder is null)
        {
            return WorkOrderErrors.NotFound(workOrderId);
        }

        var result = decide(workOrder, currentUser.UserName ?? currentUser.UserId ?? "unknown", timeProvider.GetUtcNow());
        if (result.IsFailure)
        {
            return result.Error!;
        }

        await db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
