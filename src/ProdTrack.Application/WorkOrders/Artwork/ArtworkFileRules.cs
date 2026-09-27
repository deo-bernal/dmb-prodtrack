namespace ProdTrack.Application.WorkOrders.Artwork;

/// <summary>Upload rules for artwork proofs: allow-list of extensions plus magic-byte sniffing (docs/02 section 13).</summary>
public static class ArtworkFileRules
{
    public const long MaxBytes = 20L * 1024 * 1024;

    private static readonly Dictionary<string, string> ContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = "application/pdf",
        [".png"] = "image/png",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".svg"] = "image/svg+xml",
    };

    public static IReadOnlyCollection<string> AllowedExtensions => ContentTypes.Keys;

    public static bool TryGetContentType(string fileName, out string contentType)
    {
        var extension = Path.GetExtension(fileName ?? string.Empty);
        return ContentTypes.TryGetValue(extension, out contentType!);
    }

    /// <summary>Checks that the first bytes match the declared extension.</summary>
    public static bool HeaderMatches(string extension, ReadOnlySpan<byte> header)
    {
        return extension.ToLowerInvariant() switch
        {
            ".pdf" => header.StartsWith("%PDF"u8),
            ".png" => header.StartsWith(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
            ".jpg" or ".jpeg" => header.StartsWith(new byte[] { 0xFF, 0xD8, 0xFF }),
            ".svg" => LooksLikeSvg(header),
            _ => false,
        };
    }

    private static bool LooksLikeSvg(ReadOnlySpan<byte> header)
    {
        var text = System.Text.Encoding.UTF8.GetString(header).TrimStart('\uFEFF', ' ', '\r', '\n', '\t');
        return (text.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase) || text.StartsWith("<svg", StringComparison.OrdinalIgnoreCase))
            && text.Contains("<svg", StringComparison.OrdinalIgnoreCase);
    }
}
