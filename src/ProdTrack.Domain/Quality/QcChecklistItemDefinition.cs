namespace ProdTrack.Domain.Quality;

/// <summary>Input for a checklist item. Measured items need a tolerance (min and/or max).</summary>
public sealed record QcChecklistItemDefinition(int Sequence, string Description, QcItemKind Kind, decimal? MinValue, decimal? MaxValue, string? Unit);
