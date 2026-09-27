using ProdTrack.Domain.Common;

namespace ProdTrack.Domain.ReasonCodes;

public static class ReasonCodeErrors
{
    public static Error NotFound(int id) => Error.NotFound("ReasonCode.NotFound", $"Reason code {id} was not found.");

    public static ValidationError DuplicateCode(string code, ReasonCategory category) =>
        Error.Validation("code", $"Reason code '{code}' already exists in category {category}.");

    public static ValidationError InvalidCode => Error.Validation("code", "Reason code must be 2-30 characters: A-Z, 0-9, '-' and '_'.");
}
