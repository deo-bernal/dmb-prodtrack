using System.Text.RegularExpressions;
using ProdTrack.Domain.Common;

namespace ProdTrack.Domain.Stations;

/// <summary>A production station (equipment area). Unique code; inactive stations cannot be added to routings.</summary>
public sealed partial class Station : AggregateRoot
{
    public const int CodeMaxLength = 20;
    public const int NameMaxLength = 100;

    private Station()
    {
    }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public StationType Type { get; private set; }

    public string? WorkCenter { get; private set; }

    public bool IsActive { get; private set; } = true;

    public static Result<Station> Create(string code, string name, StationType type, string? workCenter)
    {
        var normalized = NormalizeCode(code);
        if (!IsValidCode(normalized))
        {
            return StationErrors.InvalidCode;
        }

        var station = new Station { Code = normalized };
        var result = station.Update(name, type, workCenter);
        return result.IsSuccess ? station : result.Error!;
    }

    public Result Update(string name, StationType type, string? workCenter)
    {
        var errors = new List<KeyValuePair<string, string>>();
        var trimmedName = Guard.Required(name, NameMaxLength, "name", errors);
        if (!Enum.IsDefined(type))
        {
            errors.Add(new("type", "Unknown station type."));
        }

        if (errors.Count > 0)
        {
            return errors.ToValidationError();
        }

        Name = trimmedName;
        Type = type;
        WorkCenter = string.IsNullOrWhiteSpace(workCenter) ? null : workCenter.Trim();
        return Result.Success();
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;

    public static string NormalizeCode(string? code) => (code ?? string.Empty).Trim().ToUpperInvariant();

    public static bool IsValidCode(string code) => CodePattern().IsMatch(code);

    [GeneratedRegex("^[A-Z0-9][A-Z0-9-]{1,19}$", RegexOptions.CultureInvariant)]
    private static partial Regex CodePattern();
}
