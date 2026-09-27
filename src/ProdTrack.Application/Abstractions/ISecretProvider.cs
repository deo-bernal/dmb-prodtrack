namespace ProdTrack.Application.Abstractions;

/// <summary>Runtime secret lookups (rare). Configuration-backed by default; Key Vault / Secret Manager only on alternatives.</summary>
public interface ISecretProvider
{
    ValueTask<string?> GetSecretAsync(string name, CancellationToken cancellationToken);
}
