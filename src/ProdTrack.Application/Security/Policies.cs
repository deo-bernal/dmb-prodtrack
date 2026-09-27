namespace ProdTrack.Application.Security;

/// <summary>Authorization policies mapping capabilities to roles (docs/02 section 7.5, BRD 6.10).</summary>
public static class Policies
{
    public const string ReadAll = nameof(ReadAll);
    public const string ManageMasterData = nameof(ManageMasterData);
    public const string ManageProductsAndRoutings = nameof(ManageProductsAndRoutings);
    public const string PlanWorkOrders = nameof(PlanWorkOrders);
    public const string ControlWorkOrders = nameof(ControlWorkOrders);
    public const string ExecuteOperations = nameof(ExecuteOperations);
    public const string RecordInspections = nameof(RecordInspections);
    public const string UploadArtwork = nameof(UploadArtwork);
    public const string ApproveArtwork = nameof(ApproveArtwork);
    public const string ViewAudit = nameof(ViewAudit);
    public const string ManageUsers = nameof(ManageUsers);

    /// <summary>Policy name to allowed roles. Used by the Server to register policies and by the role-matrix tests.</summary>
    public static IReadOnlyDictionary<string, string[]> Definitions { get; } = new Dictionary<string, string[]>(StringComparer.Ordinal)
    {
        [ReadAll] = [.. Roles.All],
        [ManageMasterData] = [Roles.Admin],
        [ManageProductsAndRoutings] = [Roles.Admin, Roles.Planner],
        [PlanWorkOrders] = [Roles.Admin, Roles.Planner],
        [ControlWorkOrders] = [Roles.Admin, Roles.Planner, Roles.Supervisor],
        [ExecuteOperations] = [Roles.Admin, Roles.Supervisor, Roles.Operator, Roles.QC],
        [RecordInspections] = [Roles.Admin, Roles.QC],
        [UploadArtwork] = [Roles.Admin, Roles.Planner, Roles.Operator],
        [ApproveArtwork] = [Roles.Admin, Roles.Planner],
        [ViewAudit] = [Roles.Admin, Roles.Supervisor, Roles.QC],
        [ManageUsers] = [Roles.Admin],
    };
}
