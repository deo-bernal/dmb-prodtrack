namespace ProdTrack.Application.Reference;

/// <summary>Generic reference list item (colour schemes, signal words, enum lists).</summary>
public sealed record ReferenceItemModel(string Code, string Name, string? TextColor = null, string? BackgroundColor = null);
