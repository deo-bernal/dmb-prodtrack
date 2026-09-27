using ProdTrack.Application.Messaging;
using ProdTrack.Application.Security;

namespace ProdTrack.Application.WorkOrders.Artwork;

/// <summary>Approves a pending proof (PT-024).</summary>
[RequiresPolicy(Policies.ApproveArtwork)]
public sealed record ApproveArtworkProofCommand(int WorkOrderId, int Version, string? Note) : ICommand<Unit>;

/// <summary>Rejects a pending proof with a reason; a new version must be uploaded (PT-024).</summary>
[RequiresPolicy(Policies.ApproveArtwork)]
public sealed record RejectArtworkProofCommand(int WorkOrderId, int Version, string Reason) : ICommand<Unit>;
