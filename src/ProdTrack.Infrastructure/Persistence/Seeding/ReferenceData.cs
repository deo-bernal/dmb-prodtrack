using ProdTrack.Domain.Products;
using ProdTrack.Domain.ReasonCodes;
using ProdTrack.Domain.Stations;

namespace ProdTrack.Infrastructure.Persistence.Seeding;

/// <summary>
/// Reference data seeded into an empty database (BRD 5.2, 5.3, PT-016, PT-017). Standards values are high-level
/// summaries and must be verified against the current ASME A13.1 / ANSI Z535 editions before real use.
/// </summary>
internal static class ReferenceData
{
    public static readonly (string Code, string Name, StationType Type)[] Stations =
    [
        ("PREPRESS", "Prepress / artwork", StationType.Prepress),
        ("PRINT-01", "Digital printing", StationType.Printing),
        ("LAM-01", "Laminating", StationType.Laminating),
        ("ENGRAVE-01", "Engraving", StationType.Engraving),
        ("DIECUT-01", "Die-cutting / cutting", StationType.Cutting),
        ("QC-01", "Quality control", StationType.Inspection),
        ("PACK-01", "Packing and shipping", StationType.Packing),
    ];

    public static readonly (string Code, string Name, string Text, string Background)[] ColorSchemes =
    [
        ("FIRE-QUENCHING", "Fire quenching", "White", "Red"),
        ("TOXIC-CORROSIVE", "Toxic and corrosive", "Black", "Orange"),
        ("FLAMMABLE", "Flammable", "Black", "Yellow"),
        ("COMBUSTIBLE", "Combustible", "White", "Brown"),
        ("WATER", "Potable, cooling, boiler feed and other water", "White", "Green"),
        ("COMPRESSED-AIR", "Compressed air", "White", "Blue"),
    ];

    public static readonly (string Word, string Header, string Text)[] SignalWords =
    [
        ("DANGER", "Red", "White"),
        ("WARNING", "Orange", "Black"),
        ("CAUTION", "Yellow", "Black"),
        ("NOTICE", "Blue", "White"),
        ("SAFETY INSTRUCTIONS", "Green", "White"),
    ];

    public static readonly (string Code, string Description, ReasonCategory Category)[] ReasonCodes =
    [
        ("MISPRINT", "Misprint", ReasonCategory.Scrap),
        ("COLOR-MISMATCH", "Colour does not match the approved proof", ReasonCategory.Scrap),
        ("CUT-ERROR", "Cutting or die-cut error", ReasonCategory.Scrap),
        ("MATERIAL-DEFECT", "Material defect", ReasonCategory.Scrap),
        ("BREAK", "Break", ReasonCategory.Pause),
        ("MATERIAL-WAIT", "Waiting for material", ReasonCategory.Pause),
        ("SHIFT-END", "End of shift", ReasonCategory.Pause),
        ("MACHINE-FAULT", "Machine fault", ReasonCategory.Downtime),
        ("MAINTENANCE", "Planned maintenance", ReasonCategory.Downtime),
        ("NO-OPERATOR", "No operator available", ReasonCategory.Downtime),
        ("CUSTOMER-CHANGE", "Customer change", ReasonCategory.Hold),
        ("QC-FAIL", "Failed QC inspection", ReasonCategory.Hold),
    ];

    private static readonly string[] PrintedRoute = ["PREPRESS", "PRINT-01", "LAM-01", "DIECUT-01", "QC-01", "PACK-01"];

    public static readonly (ProductType Type, string Name, string[] StationCodes)[] Routings =
    [
        (ProductType.PipeMarker, "Pipe marker (printed)", PrintedRoute),
        (ProductType.ValveTag, "Valve tag (engraved)", ["PREPRESS", "ENGRAVE-01", "QC-01", "PACK-01"]),
        (ProductType.SafetySign, "Safety sign", PrintedRoute),
        (ProductType.Label, "Label", PrintedRoute),
    ];
}
