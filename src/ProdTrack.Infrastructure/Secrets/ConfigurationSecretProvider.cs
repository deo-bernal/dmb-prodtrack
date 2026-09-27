using Microsoft.Extensions.Configuration;
using ProdTrack.Application.Abstractions;

namespace ProdTrack.Infrastructure.Secrets;

/// <summary>Reads secrets from configuration (server-only appsettings.Production.json, environment variables, user-secrets).</summary>
public sealed class ConfigurationSecretProvider(IConfiguration configuration) : ISecretProvider
{
    public ValueTask<string?> GetSecretAsync(string name, CancellationToken cancellationToken) =>
        ValueTask.FromResult(configuration[name]);
}
