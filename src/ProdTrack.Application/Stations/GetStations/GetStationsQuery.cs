using ProdTrack.Application.Messaging;
using ProdTrack.Application.Security;

namespace ProdTrack.Application.Stations.GetStations;

[RequiresPolicy(Policies.ReadAll)]
public sealed record GetStationsQuery(bool IncludeInactive = false) : IQuery<IReadOnlyList<StationModel>>;
