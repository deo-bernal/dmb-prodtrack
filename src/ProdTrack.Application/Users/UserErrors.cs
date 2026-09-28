using ProdTrack.Domain.Common;

namespace ProdTrack.Application.Users;

public static class UserErrors
{
    public static Error NotFound(string id) => Error.NotFound("User.NotFound", $"User {id} was not found.");

    public static Error CannotDeactivateSelf =>
        Error.BusinessRule("User.CannotDeactivateSelf", "You cannot deactivate your own account.");

    public static Error CannotRemoveOwnAdmin =>
        Error.BusinessRule("User.CannotRemoveOwnAdmin", "You cannot remove the Admin role from your own account.");

    public static ValidationError UnknownRole(string role) => Error.Validation("roles", $"Unknown role '{role}'.");

    public static ValidationError DuplicateEmail => Error.Validation("email", "A user with this email already exists.");

    public static ValidationError DuplicateBadge => Error.Validation("badgeNumber", "This badge number is already assigned to another user.");

    public static ValidationError UnknownStation(string code) => Error.Validation("homeStationCode", $"Station '{code}' does not exist.");
}
