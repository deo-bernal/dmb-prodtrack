using ProdTrack.Domain.Common;

namespace ProdTrack.Domain.Stations;

public static class StationErrors
{
    public static Error NotFound(int id) => Error.NotFound("Station.NotFound", $"Station {id} was not found.");

    public static ValidationError DuplicateCode(string code) => Error.Validation("code", $"Station code '{code}' already exists.");

    public static ValidationError InvalidCode => Error.Validation("code", "Station code must be 2-20 characters: A-Z, 0-9 and '-'.");

    public static Error Inactive(string code) => Error.BusinessRule("Station.Inactive", $"Station '{code}' is inactive and cannot be used in new routings.");
}
