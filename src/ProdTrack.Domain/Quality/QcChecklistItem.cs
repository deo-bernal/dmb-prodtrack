using ProdTrack.Domain.Common;

namespace ProdTrack.Domain.Quality;

public sealed class QcChecklistItem : Entity
{
    public const int DescriptionMaxLength = 200;

    private QcChecklistItem()
    {
    }

    internal QcChecklistItem(QcChecklistItemDefinition definition)
    {
        Sequence = definition.Sequence;
        Description = definition.Description.Trim();
        Kind = definition.Kind;
        MinValue = definition.MinValue;
        MaxValue = definition.MaxValue;
        Unit = string.IsNullOrWhiteSpace(definition.Unit) ? null : definition.Unit.Trim();
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
