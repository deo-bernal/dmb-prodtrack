using ProdTrack.Application.Messaging;
using ProdTrack.Application.Security;

namespace ProdTrack.Application.WorkOrders.Artwork;

/// <summary>Uploads a proof (PDF, PNG, JPG, SVG; max 20 MB) as the next version, status Pending (PT-023).</summary>
[RequiresPolicy(Policies.UploadArtwork)]
public sealed record UploadArtworkProofCommand(int WorkOrderId, string FileName, long Length, Stream Content) : ICommand<ArtworkProofModel>;
