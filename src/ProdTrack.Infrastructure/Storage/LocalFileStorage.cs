using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using ProdTrack.Application.Abstractions;

namespace ProdTrack.Infrastructure.Storage;

/// <summary>Disk-backed <see cref="IFileStorage"/> (default on MonsterASP and locally). Keys cannot escape the root.</summary>
public sealed partial class LocalFileStorage(IOptions<LocalFileStorageOptions> options) : IFileStorage
{
    private readonly string _root = Path.GetFullPath(options.Value.RootPath);

    public async Task SaveAsync(string key, Stream content, string contentType, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(content);
        var path = ResolvePath(key);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temp = path + ".uploading";
        await using (var file = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
        {
            await content.CopyToAsync(file, cancellationToken);
        }

        File.Move(temp, path, overwrite: true);
    }

    public Task<Stream?> OpenReadAsync(string key, CancellationToken cancellationToken)
    {
        var path = ResolvePath(key);
        Stream? stream = File.Exists(path)
            ? new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true)
            : null;
        return Task.FromResult(stream);
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken) =>
        Task.FromResult(File.Exists(ResolvePath(key)));

    public Task<bool> DeleteAsync(string key, CancellationToken cancellationToken)
    {
        var path = ResolvePath(key);
        if (!File.Exists(path))
        {
            return Task.FromResult(false);
        }

        File.Delete(path);
        return Task.FromResult(true);
    }

    internal string ResolvePath(string key)
    {
        if (string.IsNullOrWhiteSpace(key) || !KeyPattern().IsMatch(key) || key.Contains("..", StringComparison.Ordinal))
        {
            throw new ArgumentException($"Invalid storage key '{key}'.", nameof(key));
        }

        var full = Path.GetFullPath(Path.Combine(_root, key.Replace('/', Path.DirectorySeparatorChar)));
        if (!full.StartsWith(_root + Path.DirectorySeparatorChar, StringComparison.Ordinal))
        {
            throw new ArgumentException($"Storage key '{key}' escapes the storage root.", nameof(key));
        }

        return full;
    }

    [GeneratedRegex("^[A-Za-z0-9][A-Za-z0-9._/-]{0,299}$", RegexOptions.CultureInvariant)]
    private static partial Regex KeyPattern();
}
