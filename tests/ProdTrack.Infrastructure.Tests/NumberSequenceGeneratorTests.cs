using Microsoft.Extensions.DependencyInjection;
using ProdTrack.Application.Abstractions;
using ProdTrack.Infrastructure.Persistence;
using ProdTrack.TestSupport;

namespace ProdTrack.Infrastructure.Tests;

[Trait("Category", "Unit")]
[Trait("Story", "PT-019")]
public sealed class NumberSequenceGeneratorTests : IAsyncLifetime
{
    private TestApplication _app = null!;

    public async Task InitializeAsync() => _app = await TestApplication.CreateAsync(seedReferenceData: false);

    public async Task DisposeAsync() => await _app.DisposeAsync();

    private async Task<string> NextAsync(NumberSequenceKind kind)
    {
        await using var scope = _app.Services.CreateAsyncScope();
        var generator = scope.ServiceProvider.GetRequiredService<INumberSequenceGenerator>();
        var number = await generator.NextAsync(kind, CancellationToken.None);
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().SaveChangesAsync();
        return number;
    }

    [Fact]
    public async Task Numbers_are_sequential_and_formatted_per_kind()
    {
        (await NextAsync(NumberSequenceKind.WorkOrder)).Should().Be("WO-2026-000001");
        (await NextAsync(NumberSequenceKind.WorkOrder)).Should().Be("WO-2026-000002");
        (await NextAsync(NumberSequenceKind.SalesOrder)).Should().Be("SO-2026-00001");
    }

    [Fact]
    public async Task Counter_restarts_in_a_new_plant_year()
    {
        await NextAsync(NumberSequenceKind.WorkOrder);

        // 2026-12-31 16:00 UTC is already 2027-01-01 00:00 in Asia/Manila.
        _app.Clock.SetUtcNow(new DateTimeOffset(2026, 12, 31, 16, 0, 0, TimeSpan.Zero));

        (await NextAsync(NumberSequenceKind.WorkOrder)).Should().Be("WO-2027-000001");
    }
}
