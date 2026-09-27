using ProdTrack.Application.Messaging;
using ProdTrack.Application.Security;

namespace ProdTrack.Application.Stations.DeactivateStation;

/// <summary>Deactivates a station. Open operations are unaffected; the result tells the UI to warn (PT-013).</summary>
[RequiresPolicy(Policies.ManageMasterData)]
public sealed record DeactivateStationCommand(int Id) : ICommand<DeactivateStationResult>;

public sealed record DeactivateStationResult(StationModel Station, int OpenOperationCount);
