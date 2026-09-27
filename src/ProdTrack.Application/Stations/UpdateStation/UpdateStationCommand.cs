using ProdTrack.Application.Messaging;
using ProdTrack.Application.Security;
using ProdTrack.Domain.Stations;

namespace ProdTrack.Application.Stations.UpdateStation;

[RequiresPolicy(Policies.ManageMasterData)]
public sealed record UpdateStationCommand(int Id, string Name, StationType Type, string? WorkCenter, bool IsActive) : ICommand<StationModel>;
