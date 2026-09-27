using ProdTrack.Domain.Common;

namespace ProdTrack.Domain.WorkOrders;

/// <summary>Versioned artwork proof stored through IFileStorage (PT-023/PT-024).</summary>
public sealed class ArtworkProof : Entity
{
    private ArtworkProof()
    {
    }

    internal ArtworkProof(int version, string fileKey, string contentType, string originalFileName, string uploadedBy, DateTimeOffset uploadedAtUtc)
    {
        Version = version;
        FileKey = fileKey;
        ContentType = contentType;
        OriginalFileName = originalFileName;
        UploadedBy = uploadedBy;
        UploadedAtUtc = uploadedAtUtc;
        Status = ArtworkProofStatus.Pending;
    }

    public int WorkOrderId { get; private set; }

    public int Version { get; private set; }

    public string FileKey { get; private set; } = string.Empty;

    public string ContentType { get; private set; } = string.Empty;

    public string OriginalFileName { get; private set; } = string.Empty;

    public ArtworkProofStatus Status { get; private set; }

    public string? DecisionNote { get; private set; }

    public string? DecidedBy { get; private set; }

    public DateTimeOffset? DecidedAtUtc { get; private set; }

    public string UploadedBy { get; private set; } = string.Empty;

    public DateTimeOffset UploadedAtUtc { get; private set; }

    internal void Decide(ArtworkProofStatus status, string userId, string? note, DateTimeOffset nowUtc)
    {
        Status = status;
        DecidedBy = userId;
        DecisionNote = note;
        DecidedAtUtc = nowUtc;
    }

    internal void Supersede() => Status = ArtworkProofStatus.Superseded;
}
