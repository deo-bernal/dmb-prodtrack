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
    public const string WorkOrders = Base + "/work-orders";
    public const string Dashboard = Base + "/dashboard";
}
