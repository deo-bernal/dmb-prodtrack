namespace ProdTrack.Contracts;

/// <summary>REST route constants (base path /api/v1, docs/02 section 6).</summary>
public static class ApiRoutes
{
    public const string Base = "/api/v1";
    public const string Me = Base + "/me";
    public const string Antiforgery = Base + "/antiforgery";
    public const string Stations = Base + "/stations";
    public const string ReasonCodes = Base + "/reason-codes";
    public const string Reference = Base + "/reference";
    public const string Products = Base + "/products";
    public const string Routings = Base + "/routings";
    public const string SalesOrders = Base + "/sales-orders";
    public const string WorkOrders = Base + "/work-orders";
    public const string Operations = Base + "/operations";
    public const string Scan = Base + "/scan";
    public const string Qc = Base + "/qc";
    public const string Users = Base + "/users";
    public const string Dashboard = Base + "/dashboard";

    public static string StationQueue(string stationCode) => $"{Stations}/{Uri.EscapeDataString(stationCode)}/queue";

    public static string Operation(int id) => $"{Operations}/{id}";
}
