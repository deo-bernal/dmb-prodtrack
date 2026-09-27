using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ProdTrack.Domain.Audit;
using ProdTrack.Domain.Stations;
using ProdTrack.TestSupport;

namespace ProdTrack.Infrastructure.Tests;

[Trait("Category", "Unit")]
[Trait("Story", "PT-047")]
public sealed class AuditTrailTests : IAsyncLifetime
{
    private TestApplication _app = null!;

    public async Task InitializeAsync() => _app = await TestApplication.CreateAsync(seedReferenceData: false);

    public async Task DisposeAsync() => await _app.DisposeAsync();

    [Fact]
    public async Task Insert_and_update_write_audit_entries_with_user_correlation_and_changes()
    {
        _app.User.UserId = "user-42";
        var id = await _app.WithDbAsync(async db =>
        {
            var station = Station.Create("PRINT-01", "Printer", StationType.Printing, "WC-A").Value;
            db.Stations.Add(station);
            await db.SaveChangesAsync();
            return station.Id;
        });

        await _app.WithDbAsync(async db =>
        {
            var station = await db.Stations.SingleAsync(s => s.Id == id);
            station.Update("Digital printer", StationType.Printing, "WC-A");
            return await db.SaveChangesAsync();
        });

        var entries = await _app.WithDbAsync(db => db.AuditEntries.OrderBy(a => a.Id).ToListAsync());
        entries.Should().HaveCount(2);

        var insert = entries[0];
        insert.EntityName.Should().Be(nameof(Station));
        insert.EntityKey.Should().Be(id.ToString(System.Globalization.CultureInfo.InvariantCulture));
        insert.Action.Should().Be(AuditAction.Insert);
        insert.UserId.Should().Be("user-42");
        insert.CorrelationId.Should().Be("test-correlation");
        insert.OccurredAtUtc.Should().Be(TestApplication.DefaultNow);

        var update = entries[1];
        update.Action.Should().Be(AuditAction.Update);
        using var changes = JsonDocument.Parse(update.ChangesJson);
        var name = changes.RootElement.GetProperty("Name");
        name.GetProperty("old").GetString().Should().Be("Printer");
        name.GetProperty("new").GetString().Should().Be("Digital printer");
        changes.RootElement.TryGetProperty("Code", out _).Should().BeFalse("unchanged fields are not logged");
    }

    [Fact]
    public async Task Save_without_changes_writes_nothing()
    {
        await _app.WithDbAsync(db => db.SaveChangesAsync());

        (await _app.WithDbAsync(db => db.AuditEntries.CountAsync())).Should().Be(0);
    }
}
