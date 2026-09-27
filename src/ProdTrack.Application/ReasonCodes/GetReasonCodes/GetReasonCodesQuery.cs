using ProdTrack.Application.Messaging;
using ProdTrack.Application.Security;
using ProdTrack.Domain.ReasonCodes;

namespace ProdTrack.Application.ReasonCodes.GetReasonCodes;

[RequiresPolicy(Policies.ReadAll)]
public sealed record GetReasonCodesQuery(ReasonCategory? Category = null, bool IncludeInactive = false) : IQuery<IReadOnlyList<ReasonCodeModel>>;
