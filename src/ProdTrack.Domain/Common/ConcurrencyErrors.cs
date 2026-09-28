namespace ProdTrack.Domain.Common;

/// <summary>Optimistic concurrency failures (docs/02 section 6: If-Match with the base64 rowversion).</summary>
public static class ConcurrencyErrors
{
    /// <summary>The If-Match version sent by the client no longer matches the stored version (412).</summary>
    public static Error StaleVersion =>
        Error.PreconditionFailed("Concurrency.StaleVersion", "This record was changed by someone else. Reload it and apply your change again.");

    /// <summary>Another request saved the same record while this one was running (409).</summary>
    public static Error Conflict =>
        Error.Conflict("Concurrency.Conflict", "This record was changed by someone else at the same time. Reload it and try again.");

    /// <summary>True when no version was supplied (last write wins) or the supplied version matches.</summary>
    public static bool Matches(byte[]? current, byte[]? expected) =>
        expected is null || expected.Length == 0 || (current ?? []).AsSpan().SequenceEqual(expected);
}
