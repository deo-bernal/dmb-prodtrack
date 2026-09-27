using System.Text.RegularExpressions;
using ProdTrack.Domain.Common;

namespace ProdTrack.Domain.ReasonCodes;

/// <summary>Configurable reason (scrap, pause, downtime, hold, skip). Deactivated codes stay in history.</summary>
public sealed partial class ReasonCode : AggregateRoot
{
    public const int CodeMaxLength = 30;
    public const int DescriptionMaxLength = 200;

    private ReasonCode()
    {
    }

    public string Code { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public ReasonCategory Category { get; private set; }

    public bool IsActive { get; private set; } = true;

    public static Result<ReasonCode> Create(string code, string description, ReasonCategory category)
    {
        var normalized = (code ?? string.Empty).Trim().ToUpperInvariant();
        if (!CodePattern().IsMatch(normalized))
        {
            return ReasonCodeErrors.InvalidCode;
        }

        if (!Enum.IsDefined(category))
        {
            return Error.Validation("category", "Unknown reason category.");
        }

        var reason = new ReasonCode { Code = normalized, Category = category };
        var result = reason.Update(description, isActive: true);
        return result.IsSuccess ? reason : result.Error!;
    }

    public Result Update(string description, bool isActive)
    {
        var errors = new List<KeyValuePair<string, string>>();
        var trimmed = Guard.Required(description, DescriptionMaxLength, "description", errors);
        if (errors.Count > 0)
        {
            return errors.ToValidationError();
        }

        Description = trimmed;
        IsActive = isActive;
        return Result.Success();
    }

    [GeneratedRegex("^[A-Z0-9][A-Z0-9_-]{1,29}$", RegexOptions.CultureInvariant)]
    private static partial Regex CodePattern();
}
