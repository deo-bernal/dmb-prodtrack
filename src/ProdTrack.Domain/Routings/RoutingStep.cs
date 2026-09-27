using ProdTrack.Domain.Common;

namespace ProdTrack.Domain.Routings;

public sealed class RoutingStep : Entity
{
    private RoutingStep()
    {
    }

    internal RoutingStep(int sequence, int stationId, decimal setupMinutes, decimal stdMinutesPerUnit, bool allowOverlap)
    {
        Sequence = sequence;
        StationId = stationId;
        SetupMinutes = setupMinutes;
        StdMinutesPerUnit = stdMinutesPerUnit;
        AllowOverlap = allowOverlap;
    }

    public int RoutingId { get; private set; }

    public int Sequence { get; private set; }

    public int StationId { get; private set; }

    public decimal SetupMinutes { get; private set; }

    public decimal StdMinutesPerUnit { get; private set; }

    public bool AllowOverlap { get; private set; }
}
