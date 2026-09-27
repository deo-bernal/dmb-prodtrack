namespace ProdTrack.Application.Reference;

public static class ReferenceLists
{
    public const string ColorSchemes = "color-schemes";
    public const string SignalWords = "signal-words";
    public const string ProductTypes = "product-types";
    public const string StationTypes = "station-types";
    public const string ReasonCategories = "reason-categories";

    public static IReadOnlyList<string> All { get; } = [ColorSchemes, SignalWords, ProductTypes, StationTypes, ReasonCategories];
}
