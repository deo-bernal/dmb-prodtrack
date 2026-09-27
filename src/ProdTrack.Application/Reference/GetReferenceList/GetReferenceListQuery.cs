using ProdTrack.Application.Messaging;
using ProdTrack.Application.Security;

namespace ProdTrack.Application.Reference.GetReferenceList;

[RequiresPolicy(Policies.ReadAll)]
public sealed record GetReferenceListQuery(string List) : IQuery<IReadOnlyList<ReferenceItemModel>>;
