using System.Text;
using Microsoft.Extensions.Options;
using ProdTrack.Infrastructure.Storage;

namespace ProdTrack.Infrastructure.Tests;

[Trait("Category", "Unit")]
[Trait("Story", "PT-023")]
public sealed class LocalFileStorageTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "prodtrack-tests", Guid.NewGuid().ToString("N"));
    private readonly LocalFileStorage _storage;

    public LocalFileStorageTests() =>
        _storage = new LocalFileStorage(Options.Create(new LocalFileStorageOptions { RootPath = _root }));

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    [Fact]
    public async Task Save_open_exists_delete_round_trip()
    {
        const string key = "workorders/WO-2026-000001/artwork/v1/proof.pdf";
        await using (var content = new MemoryStream(Encoding.ASCII.GetBytes("%PDF-1.7")))
        {
            await _storage.SaveAsync(key, content, "application/pdf", CancellationToken.None);
        }

        (await _storage.ExistsAsync(key, CancellationToken.None)).Should().BeTrue();
        await using (var stream = await _storage.OpenReadAsync(key, CancellationToken.None))
        {
            using var reader = new StreamReader(stream!);
            (await reader.ReadToEndAsync()).Should().Be("%PDF-1.7");
        }

        (await _storage.DeleteAsync(key, CancellationToken.None)).Should().BeTrue();
        (await _storage.ExistsAsync(key, CancellationToken.None)).Should().BeFalse();
        (await _storage.OpenReadAsync(key, CancellationToken.None)).Should().BeNull();
    }

    [Theory]
    [InlineData("../outside.txt")]
    [InlineData("workorders/../../outside.txt")]
    [InlineData("/etc/passwd")]
    [InlineData("C:\\Windows\\win.ini")]
    [InlineData("")]
    public async Task Keys_cannot_escape_the_root(string key)
    {
        var act = () => _storage.ExistsAsync(key, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }
}
