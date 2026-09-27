using Microsoft.EntityFrameworkCore;
using ProdTrack.Application.Abstractions;
using ProdTrack.Application.Messaging;
using ProdTrack.Domain.Common;
using ProdTrack.Domain.Stations;

namespace ProdTrack.Application.Stations.CreateStation;

internal sealed class CreateStationHandler(IAppDbContext db) : ICommandHandler<CreateStationCommand, StationModel>
{
    public async Task<Result<StationModel>> HandleAsync(CreateStationCommand command, CancellationToken cancellationToken)
    {
        var created = Station.Create(command.Code, command.Name, command.Type, command.WorkCenter);
        if (created.IsFailure)
        {
            return created.Error!;
        }

        var station = created.Value;
        if (await db.Stations.AnyAsync(s => s.Code == station.Code, cancellationToken))
        {
            return StationErrors.DuplicateCode(station.Code);
        }

        db.Stations.Add(station);
        await db.SaveChangesAsync(cancellationToken);
        return StationModel.From(station);
    }
}
