using ProdTrack.Domain.Products;
using ProdTrack.Domain.Quality;

namespace ProdTrack.Infrastructure.Persistence.Seeding;

/// <summary>Default QC checklists per product type (PT-035), seeded when the template table is empty.</summary>
internal static class QcTemplateData
{
    private static QcChecklistItemDefinition Check(int sequence, string description) =>
        new(sequence, description, QcItemKind.PassFail, null, null, null);

    private static QcChecklistItemDefinition Measure(int sequence, string description, decimal min, decimal max, string unit) =>
        new(sequence, description, QcItemKind.Measured, min, max, unit);

    public static readonly (ProductType Type, string Name, QcChecklistItemDefinition[] Items)[] Templates =
    [
        (ProductType.PipeMarker, "Pipe marker inspection", [
            Check(10, "Legend text matches the work order (spelling, flow arrows)"),
            Check(20, "Colour scheme matches ASME A13.1 and the spec"),
            Measure(30, "Length deviation from spec", -2m, 2m, "mm"),
            Check(40, "Adhesion and laminate free of bubbles"),
        ]),
        (ProductType.ValveTag, "Valve tag inspection", [
            Check(10, "Engraved text legible and matches the work order"),
            Measure(20, "Engraving depth", 0.10m, 0.30m, "mm"),
            Check(30, "Hole size and position correct"),
            Check(40, "Edges deburred"),
        ]),
        (ProductType.SafetySign, "Safety sign inspection", [
            Check(10, "Signal word, header colour and symbol per ANSI Z535"),
            Check(20, "Message text matches the approved proof"),
            Measure(30, "Width deviation from spec", -1m, 1m, "mm"),
            Check(40, "Mounting holes and corners correct"),
        ]),
        (ProductType.Label, "Label inspection", [
            Check(10, "Text and barcode legible, matches the proof"),
            Measure(20, "Width deviation from spec", -0.5m, 0.5m, "mm"),
            Check(30, "Adhesive liner intact, labels peel cleanly"),
            Check(40, "Finish / laminate as specified"),
        ]),
    ];
}
