namespace ProdTrack.Infrastructure.Time;

public sealed class PlantOptions
{
    public const string SectionName = "Plant";

    /// <summary>IANA time zone ID (works on Windows and Linux with ICU).</summary>
    public string TimeZone { get; set; } = "Asia/Manila";
}
