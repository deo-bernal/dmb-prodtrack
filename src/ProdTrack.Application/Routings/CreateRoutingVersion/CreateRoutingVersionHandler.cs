using Microsoft.EntityFrameworkCore;
using ProdTrack.Application.Abstractions;
using ProdTrack.Application.Messaging;
using ProdTrack.Domain.Common;
using ProdTrack.Domain.Routings;
using ProdTrack.Domain.Stations;

namespace ProdTrack.Application.Routings.CreateRoutingVersion;

internal sealed class CreateRoutingVersionHandler(IAppDbContext db, TimeProvider timeProvider)
    : ICommandHandler<CreateRoutingVersionCommand, RoutingModel>
{
    public async Task<Result<RoutingModel>> HandleAsync(CreateRoutingVersionCommand command, CancellationToken cancellationToken)
    {
        var codes = command.Steps.Select(s => Station.NormalizeCode(s.StationCode)).Distinct().ToList();
        var stations = await db.Stations.Where(s => codes.Contains(s.Code)).ToDictionaryAsync(s => s.Code, cancellationToken);
        var missing = codes.FirstOrDefault(c => !stations.ContainsKey(c));
        if (missing is not null)
        {
            return Error.Validation("steps", $"Station '{missing}' does not exist.");
        }

        var previous = await db.Routings.Where(r => r.ProductType == command.ProductType).ToListAsync(cancellationToken);
        var nextVersion = previous.Count == 0 ? 1 : previous.Max(r => r.Version) + 1;

        var definitions = command.Steps
            .Select(s => new RoutingStepDefinition(
                s.Sequence,
                stations[Station.NormalizeCode(s.StationCode)],
                s.SetupMinutes,
                s.StdMinutesPerUnit,
                s.AllowOverlap))
            .ToList();
        var created = Routing.Create(command.ProductType, nextVersion, command.Name, definitions, timeProvider.GetUtcNow());
        if (created.IsFailure)
        {
            return created.Error!;
        }

        foreach (var routing in previous.Where(r => r.IsCurrent))
        {
            routing.Supersede();
        }

        db.Routings.Add(created.Value);
        await db.SaveChangesAsync(cancellationToken);

        var models = await RoutingQueries.LoadAsync(db, db.Routings.Where(r => r.Id == created.Value.Id), cancellationToken);
        return models[0];
    }
}
