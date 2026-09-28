using Microsoft.EntityFrameworkCore;
using ProdTrack.Application.Messaging;
using ProdTrack.Application.Products.GetProducts;
using ProdTrack.Application.WorkOrders.CreateWorkOrder;
using ProdTrack.Application.WorkOrders.UpdateWorkOrder;
using ProdTrack.Domain.Common;
using ProdTrack.TestSupport;

namespace ProdTrack.Application.Tests;

[Trait("Category", "Unit")]
[Trait("Story", "PT-012")]
public sealed class ConcurrencyTests : IAsyncLifetime
{
    private TestApplication _app = null!;

    public async Task InitializeAsync() => _app = await TestApplication.CreateAsync(seedDemoData: true);

    public async Task DisposeAsync() => await _app.DisposeAsync();

    [Fact]
    public async Task Concurrent_write_raises_db_update_concurrency_exception()
    {
        var productId = (await _app.QueryAsync(new GetProductsQuery(Search: "VT-BRASS"))).Value.Single().Id;
        var created = (await _app.SendAsync(new CreateWorkOrderCommand(productId, 5, new DateOnly(2026, 4, 1), 3, null, null))).Value;

        var act = () => _app.WithDbAsync(async db =>
        {
            var stale = await db.WorkOrders.SingleAsync(w => w.Id == created.Id);
            (await _app.SendAsync(new UpdateWorkOrderCommand(created.Id, 6, new DateOnly(2026, 4, 1), 3, null, null))).IsSuccess.Should().BeTrue();
            stale.UpdatePlanning(7, new DateOnly(2026, 4, 1), 3, null, null);
            return await db.SaveChangesAsync();
        });

        await act.Should().ThrowAsync<DbUpdateConcurrencyException>();
    }

    [Fact]
    public async Task Decorator_maps_concurrency_exception_to_conflict()
    {
        var decorator = new ConcurrencyCommandDecorator<FakeCommand, Unit>(new ThrowingHandler());

        var result = await decorator.HandleAsync(new FakeCommand(), CancellationToken.None);

        result.Error!.Type.Should().Be(ErrorType.Conflict);
        result.Error.Code.Should().Be("Concurrency.Conflict");
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData(new byte[] { 1, 2 }, true)]
    [InlineData(new byte[] { 1, 3 }, false)]
    public void Matches_treats_missing_expected_version_as_last_write_wins(byte[]? expected, bool matches) =>
        ConcurrencyErrors.Matches([1, 2], expected).Should().Be(matches);

    internal sealed record FakeCommand : ICommand<Unit>;

    private sealed class ThrowingHandler : ICommandHandler<FakeCommand, Unit>
    {
        public Task<Result<Unit>> HandleAsync(FakeCommand command, CancellationToken cancellationToken) =>
            throw new DbUpdateConcurrencyException("simulated");
    }
}
