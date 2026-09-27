using ProdTrack.Domain.Common;

namespace ProdTrack.Domain.Quality;

public sealed class QcChecklistItem : Entity
{
    private QcChecklistItem()
    {
    }

    public int TemplateId { get; private set; }

    public int Sequence { get; private set; }

    public string Description { get; private set; } = string.Empty;

    public QcItemKind Kind { get; private set; }

    public decimal? MinValue { get; private set; }

    public decimal? MaxValue { get; private set; }

    public string? Unit { get; private set; }

    /// <summary>Out-of-tolerance measurements fail the item (FR-QC-02).</summary>
    public bool IsWithinTolerance(decimal value) =>
        (MinValue is null || value >= MinValue) && (MaxValue is null || value <= MaxValue);
}
