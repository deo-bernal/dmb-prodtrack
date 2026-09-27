using ProdTrack.Application.Messaging;
using ProdTrack.Application.Security;

namespace ProdTrack.Application.WorkOrders.Artwork;

[RequiresPolicy(Policies.ReadAll)]
public sealed record GetArtworkProofFileQuery(int WorkOrderId, int Version) : IQuery<ArtworkFile>;

/// <summary>Streamed proof file; the caller disposes <see cref="Content"/>.</summary>
public sealed record ArtworkFile(Stream Content, string ContentType, string FileName);
