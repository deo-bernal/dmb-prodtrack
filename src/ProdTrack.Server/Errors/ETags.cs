using Microsoft.Net.Http.Headers;
using ProdTrack.Domain.Common;

namespace ProdTrack.Server.Errors;

/// <summary>
/// Optimistic concurrency over HTTP (docs/02 section 6.3): GET detail responses carry <c>ETag: "&lt;base64 rowversion&gt;"</c>;
/// updates may send <c>If-Match</c>. A stale If-Match returns 412; a concurrent write detected at save returns 409.
/// No If-Match (or <c>*</c>) means last-write-wins.
/// </summary>
internal static class ETags
{
    public static string? Format(byte[]? version) => version is null or { Length: 0 } ? null : Convert.ToBase64String(version);

    public static void Set(HttpResponse response, byte[]? version) => SetRaw(response, Format(version));

    public static void SetRaw(HttpResponse response, string? tag)
    {
        ArgumentNullException.ThrowIfNull(response);
        if (!string.IsNullOrEmpty(tag))
        {
            response.Headers[HeaderNames.ETag] = $"\"{tag}\"";
        }
    }

    /// <summary>Reads If-Match as an opaque string (without quotes / weak prefix); null when absent or <c>*</c>.</summary>
    public static string? ReadRaw(HttpRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var header = request.Headers[HeaderNames.IfMatch].ToString().Trim();
        if (header.Length == 0 || header == "*")
        {
            return null;
        }

        if (header.StartsWith("W/", StringComparison.Ordinal))
        {
            header = header[2..];
        }

        return header.Trim('"');
    }

    /// <summary>Parses If-Match into a rowversion. Malformed values fail with 412 (they can never match).</summary>
    public static Result<byte[]?> ReadVersion(HttpRequest request)
    {
        var raw = ReadRaw(request);
        if (raw is null)
        {
            return Result.Success<byte[]?>(null);
        }

        var buffer = new byte[raw.Length];
        return Convert.TryFromBase64String(raw, buffer, out var written)
            ? Result.Success<byte[]?>(buffer[..written])
            : ConcurrencyErrors.StaleVersion;
    }
}
