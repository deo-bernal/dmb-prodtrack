using System.Globalization;
using ProdTrack.Domain.Common;

namespace ProdTrack.Application.Scanning;

public enum ScanKind
{
    WorkOrder,
    Operation,
    Station,
}

/// <summary>A parsed traveler/station code (docs/02 section 9).</summary>
public sealed record ScanCode(ScanKind Kind, string Value, int? Sequence = null);

/// <summary>
/// Parses scanned payloads: <c>WO:{number}</c>, <c>OP:{number}:{seq}</c>, <c>ST:{stationCode}</c>. For manual entry a
/// bare work order number (<c>WO-2026-000123</c>) or <c>WO-2026-000123:20</c> is accepted too.
/// </summary>
public static class ScanCodeParser
{
    public const int MaxLength = 64;

    public static string WorkOrderPayload(string number) => $"WO:{number}";

    public static string OperationPayload(string number, int sequence) =>
        string.Create(CultureInfo.InvariantCulture, $"OP:{number}:{sequence}");

    public static string StationPayload(string stationCode) => $"ST:{stationCode}";

    public static Result<ScanCode> Parse(string? raw)
    {
        var code = (raw ?? string.Empty).Trim().ToUpperInvariant();
        if (code.Length == 0 || code.Length > MaxLength)
        {
            return Invalid(raw);
        }

        var parts = code.Split(':');
        switch (parts[0])
        {
            case "WO" when parts.Length == 2 && IsWorkOrderNumber(parts[1]):
                return new ScanCode(ScanKind.WorkOrder, parts[1]);
            case "OP" when parts.Length == 3 && IsWorkOrderNumber(parts[1]) && TryParseSequence(parts[2], out var seq):
                return new ScanCode(ScanKind.Operation, parts[1], seq);
            case "ST" when parts.Length == 2 && parts[1].Length is > 0 and <= 20:
                return new ScanCode(ScanKind.Station, parts[1]);
        }

        if (parts.Length == 1 && IsWorkOrderNumber(parts[0]))
        {
            return new ScanCode(ScanKind.WorkOrder, parts[0]);
        }

        if (parts.Length == 2 && IsWorkOrderNumber(parts[0]) && TryParseSequence(parts[1], out var sequence))
        {
            return new ScanCode(ScanKind.Operation, parts[0], sequence);
        }

        return Invalid(raw);
    }

    private static bool TryParseSequence(string value, out int sequence) =>
        int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out sequence) && sequence > 0;

    private static bool IsWorkOrderNumber(string value) =>
        value.Length is >= 8 and <= 20 && value.StartsWith("WO-", StringComparison.Ordinal) && value[3..].All(c => char.IsAsciiDigit(c) || c == '-');

    private static ValidationError Invalid(string? raw) =>
        Error.Validation("code", $"'{raw?.Trim()}' is not a ProdTrack code. Scan the traveler QR code or type the work order number.");
}
