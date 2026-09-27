using ProdTrack.Application.Messaging;
using ProdTrack.Application.Security;
using ProdTrack.Domain.Stations;

namespace ProdTrack.Application.Stations.CreateStation;

[RequiresPolicy(Policies.ManageMasterData)]
public sealed record CreateStationCommand(string Code, string Name, StationType Type, string? WorkCenter) : ICommand<StationModel>;
