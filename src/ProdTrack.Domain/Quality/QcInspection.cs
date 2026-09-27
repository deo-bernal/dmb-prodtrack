using ProdTrack.Domain.Common;

namespace ProdTrack.Domain.Quality;

/// <summary>QC inspection of an operation (schema only; behaviour arrives with PT-036/PT-037).</summary>
public sealed class QcInspection : AggregateRoot
{
    private readonly List<QcResultItem> _resultItems = [];

    private QcInspection()
    {
    }

    public int OperationId { get; private set; }

    public int SampleSize { get; private set; }

    public QcResult Result { get; private set; }

    public QcDisposition Disposition { get; private set; }

    public string InspectorId { get; private set; } = string.Empty;

    public string? Notes { get; private set; }

    public DateTimeOffset InspectedAtUtc { get; private set; }

    public IReadOnlyList<QcResultItem> ResultItems => _resultItems;
}
