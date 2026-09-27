using Microsoft.EntityFrameworkCore;
using ProdTrack.Application.Abstractions;
using ProdTrack.Application.Messaging;
using ProdTrack.Domain.Common;
using ProdTrack.Domain.WorkOrders;

namespace ProdTrack.Application.WorkOrders.Artwork;

internal sealed class GetArtworkProofFileHandler(IAppDbContext db, IFileStorage storage) : IQueryHandler<GetArtworkProofFileQuery, ArtworkFile>
{
    public async Task<Result<ArtworkFile>> HandleAsync(GetArtworkProofFileQuery query, CancellationToken cancellationToken)
    {
        var proof = await db.WorkOrders.AsNoTracking()
            .Where(w => w.Id == query.WorkOrderId)
            .SelectMany(w => w.ArtworkProofs)
            .Where(p => p.Version == query.Version)
            .Select(p => new { p.FileKey, p.ContentType, p.OriginalFileName })
            .FirstOrDefaultAsync(cancellationToken);
        if (proof is null)
        {
            return WorkOrderErrors.ArtworkProofNotFound(query.Version);
        }

        var stream = await storage.OpenReadAsync(proof.FileKey, cancellationToken);
        return stream is null
            ? WorkOrderErrors.ArtworkProofNotFound(query.Version)
            : new ArtworkFile(stream, proof.ContentType, proof.OriginalFileName);
    }
}
