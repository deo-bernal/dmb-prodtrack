using ProdTrack.Domain.ReasonCodes;

namespace ProdTrack.Application.ReasonCodes;

public sealed record ReasonCodeModel(int Id, string Code, string Description, ReasonCategory Category, bool IsActive, byte[]? Version = null)
{
    public static ReasonCodeModel From(ReasonCode r) => new(r.Id, r.Code, r.Description, r.Category, r.IsActive, r.RowVersion);
}
