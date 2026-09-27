using ProdTrack.Application.Abstractions;
using ProdTrack.Application.Messaging;
using ProdTrack.Domain.Common;
using ProdTrack.Domain.Stations;

namespace ProdTrack.Application.Stations.UpdateStation;

internal sealed class UpdateStationHandler(IAppDbContext db) : ICommandHandler<UpdateStationCommand, StationModel>
{
    public async Task<Result<StationModel>> HandleAsync(UpdateStationCommand command, CancellationToken cancellationToken)
    {
        var station = await db.Stations.FindAsync([command.Id], cancellationToken);
        if (station is null)
        {
            return StationErrors.NotFound(command.Id);
        }

        var result = station.Update(command.Name, command.Type, command.WorkCenter);
        if (result.IsFailure)
        {
            return result.Error!;
        }

        if (command.IsActive)
        {
            station.Activate();
        }
        else
        {
            station.Deactivate();
        }

        await db.SaveChangesAsync(cancellationToken);
        return StationModel.From(station);
    }
}
