using Microsoft.EntityFrameworkCore;
using ProdTrack.Application.Abstractions;
using ProdTrack.Application.Messaging;
using ProdTrack.Domain.Common;
using ProdTrack.Domain.Stations;
using ProdTrack.Domain.WorkOrders;

namespace ProdTrack.Application.Stations.DeactivateStation;

internal sealed class DeactivateStationHandler(IAppDbContext db) : ICommandHandler<DeactivateStationCommand, DeactivateStationResult>
{
    public async Task<Result<DeactivateStationResult>> HandleAsync(DeactivateStationCommand command, CancellationToken cancellationToken)
    {
        var station = await db.Stations.FindAsync([command.Id], cancellationToken);
        if (station is null)
        {
            return StationErrors.NotFound(command.Id);
        }

        var openOperations = await db.Operations.CountAsync(
            o => o.StationId == station.Id && o.Status != OperationStatus.Completed && o.Status != OperationStatus.Skipped,
            cancellationToken);

        station.Deactivate();
        await db.SaveChangesAsync(cancellationToken);
        return new DeactivateStationResult(StationModel.From(station), openOperations);
    }
}
