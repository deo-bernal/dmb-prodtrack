using Microsoft.EntityFrameworkCore;
using ProdTrack.Application.Abstractions;
using ProdTrack.Application.Messaging;
using ProdTrack.Domain.Common;
using ProdTrack.Domain.WorkOrders;

namespace ProdTrack.Application.WorkOrders.Artwork;

internal sealed class UploadArtworkProofHandler(
    IAppDbContext db,
    IFileStorage storage,
    ICurrentUser currentUser,
    TimeProvider timeProvider) : ICommandHandler<UploadArtworkProofCommand, ArtworkProofModel>
{
    public async Task<Result<ArtworkProofModel>> HandleAsync(UploadArtworkProofCommand command, CancellationToken cancellationToken)
    {
        var fileError = await ValidateFileAsync(command, cancellationToken);
        if (fileError is not null)
        {
            return fileError;
        }

        var workOrder = await db.WorkOrders.Include(w => w.ArtworkProofs)
            .FirstOrDefaultAsync(w => w.Id == command.WorkOrderId, cancellationToken);
        if (workOrder is null)
        {
            return WorkOrderErrors.NotFound(command.WorkOrderId);
        }

        if (workOrder.Status != WorkOrderStatus.Draft)
        {
            return WorkOrderErrors.InvalidStatusTransition(workOrder.Status, "upload artwork for");
        }

        ArtworkFileRules.TryGetContentType(command.FileName, out var contentType);
        var extension = Path.GetExtension(command.FileName).ToLowerInvariant();
        var key = $"workorders/{workOrder.Number}/artwork/v{workOrder.NextArtworkVersion}/{Guid.NewGuid():N}{extension}";
        await storage.SaveAsync(key, command.Content, contentType, cancellationToken);

        var userName = currentUser.UserName ?? currentUser.UserId ?? "unknown";
        var added = workOrder.AddArtworkProof(key, contentType, Path.GetFileName(command.FileName), userName, timeProvider.GetUtcNow());
        if (added.IsFailure)
        {
            await storage.DeleteAsync(key, cancellationToken);
            return added.Error!;
        }

        await db.SaveChangesAsync(cancellationToken);
        var p = added.Value;
        return new ArtworkProofModel(p.Version, p.OriginalFileName, p.ContentType, p.Status, p.UploadedBy, p.UploadedAtUtc, p.DecidedBy, p.DecidedAtUtc, p.DecisionNote);
    }

    private static async Task<Error?> ValidateFileAsync(UploadArtworkProofCommand command, CancellationToken cancellationToken)
    {
        if (!ArtworkFileRules.TryGetContentType(command.FileName, out _))
        {
            return Error.Validation("file", $"Only {string.Join(", ", ArtworkFileRules.AllowedExtensions)} files are allowed.");
        }

        if (command.Length <= 0 || command.Length > ArtworkFileRules.MaxBytes)
        {
            return Error.Validation("file", "The file must be between 1 byte and 20 MB.");
        }

        if (!command.Content.CanSeek)
        {
            return Error.Validation("file", "The upload stream must be seekable.");
        }

        var header = new byte[512];
        var read = await command.Content.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false, cancellationToken);
        command.Content.Seek(0, SeekOrigin.Begin);
        return ArtworkFileRules.HeaderMatches(Path.GetExtension(command.FileName), header.AsSpan(0, read))
            ? null
            : Error.Validation("file", "The file content does not match its extension.");
    }
}
