using ProdTrack.Domain.Common;

namespace ProdTrack.Server.Endpoints;

/// <summary>Parses wire enum strings (case-insensitive) into domain enums with a 400 validation error on failure.</summary>
internal static class EnumBinding
{
    public static Result<TEnum> Parse<TEnum>(string? value, string field)
        where TEnum : struct, Enum
    {
        if (Enum.TryParse<TEnum>(value, ignoreCase: true, out var parsed) && Enum.IsDefined(parsed) && !int.TryParse(value, out _))
        {
            return parsed;
        }

        return Error.Validation(field, $"'{value}' is not valid. Allowed: {string.Join(", ", Enum.GetNames<TEnum>())}.");
    }

    public static Result<TEnum?> ParseOptional<TEnum>(string? value, string field)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<TEnum?>.Success(null);
        }

        var parsed = Parse<TEnum>(value, field);
        return parsed.IsSuccess ? Result<TEnum?>.Success(parsed.Value) : Result<TEnum?>.Failure(parsed.Error!);
    }
}
