namespace ProdTrack.Application.Abstractions;

/// <summary>Host-portable file storage (docs/02 section 10). Local disk by default.</summary>
public interface IFileStorage
{
    /// <summary>Saves the content under the given object key (e.g. workorders/WO-2026-000001/artwork/v1/{guid}.pdf).</summary>
    Task SaveAsync(string key, Stream content, string contentType, CancellationToken cancellationToken);

    /// <summary>Opens the object for streaming, or returns null when it does not exist.</summary>
    Task<Stream?> OpenReadAsync(string key, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(string key, CancellationToken cancellationToken);
}
