namespace ProdTrack.Application.Security;

/// <summary>Identity role names (BRD 6.10).</summary>
public static class Roles
{
    public const string Admin = "Admin";
    public const string Planner = "Planner";
    public const string Supervisor = "Supervisor";
    public const string Operator = "Operator";
    public const string QC = "QC";
    public const string Viewer = "Viewer";

    public static IReadOnlyList<string> All { get; } = [Admin, Planner, Supervisor, Operator, QC, Viewer];
}
