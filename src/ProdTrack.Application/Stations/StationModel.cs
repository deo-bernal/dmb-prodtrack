using ProdTrack.Domain.Stations;

namespace ProdTrack.Application.Stations;

public sealed record StationModel(int Id, string Code, string Name, StationType Type, string? WorkCenter, bool IsActive, byte[]? Version = null)
{
    public static StationModel From(Station s) => new(s.Id, s.Code, s.Name, s.Type, s.WorkCenter, s.IsActive, s.RowVersion);
}
