using ProdTrack.Domain.Common;
using ProdTrack.Domain.Products;
using ProdTrack.Domain.WorkOrders;

namespace ProdTrack.Domain.Tests;

[Trait("Category", "Unit")]
public class WorkOrderTests
{
    private static WorkOrder Draft(Product? product = null, string? legend = "STEAM") =>
        WorkOrder.Create("WO-2026-000001", product ?? TestData.PipeMarker(), 100, TestData.Today.AddDays(5), 3, "ACME", legend, TestData.Now).Value;

    [Fact]
    [Trait("Story", "PT-019")]
    public void Create_snapshots_product_spec_and_starts_as_draft()
    {
        var product = TestData.PipeMarker();

        var wo = Draft(product);

        wo.Status.Should().Be(WorkOrderStatus.Draft);
        wo.Spec.Should().Be(product.DefaultSpec);
        wo.ProductType.Should().Be(ProductType.PipeMarker);
        wo.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<WorkOrderCreated>();
    }

    [Fact]
    [Trait("Story", "PT-019")]
    public void Pipe_marker_requires_legend()
    {
        var result = WorkOrder.Create("WO-2026-000001", TestData.PipeMarker(), 10, TestData.Today, 3, null, " ", TestData.Now);

        result.Error.Should().BeOfType<ValidationError>().Which.Errors.Should().ContainKey("legend");
    }

    [Theory]
    [Trait("Story", "PT-019")]
    [InlineData(0, 3, "quantity")]
    [InlineData(10, 0, "priority")]
    [InlineData(10, 6, "priority")]
    public void Create_validates_quantity_and_priority(int quantity, int priority, string field)
    {
        var result = WorkOrder.Create("WO-2026-000001", TestData.PipeMarker(), quantity, TestData.Today, priority, null, "STEAM", TestData.Now);

        result.Error.Should().BeOfType<ValidationError>().Which.Errors.Should().ContainKey(field);
    }

    [Fact]
    [Trait("Story", "PT-019")]
    public void Create_refuses_inactive_product()
    {
        var product = TestData.PipeMarker();
        product.Update(product.Name, false, product.DefaultSpec, isActive: false);

        var result = WorkOrder.Create("WO-2026-000001", product, 10, TestData.Today, 3, null, "STEAM", TestData.Now);

        result.Error!.Code.Should().Be("Product.Inactive");
    }

    [Fact]
    [Trait("Story", "PT-020")]
    public void Release_creates_operations_with_first_ready_and_input_quantity()
    {
        var wo = Draft();
        var routing = TestData.Routing(ProductType.PipeMarker, "PRINT-01", "LAM-01", "PACK-01");

        var result = wo.Release(routing, TestData.Now);

        result.IsSuccess.Should().BeTrue();
        wo.Status.Should().Be(WorkOrderStatus.Released);
        wo.RoutingId.Should().Be(routing.Id);
        wo.ReleasedAtUtc.Should().Be(TestData.Now);
        wo.Operations.Should().HaveCount(3);
        wo.Operations[0].Status.Should().Be(OperationStatus.Ready);
        wo.Operations[0].InputQuantity.Should().Be(100);
        wo.Operations.Skip(1).Should().OnlyContain(o => o.Status == OperationStatus.Pending && o.InputQuantity == 0);
        wo.DomainEvents.OfType<WorkOrderReleased>().Should().ContainSingle();
    }

    [Fact]
    [Trait("Story", "PT-020")]
    public void Release_twice_is_an_invalid_transition()
    {
        var wo = Draft();
        var routing = TestData.Routing(ProductType.PipeMarker, "PRINT-01");
        wo.Release(routing, TestData.Now);

        var result = wo.Release(routing, TestData.Now);

        result.Error!.Type.Should().Be(ErrorType.BusinessRule);
    }

    [Fact]
    [Trait("Story", "PT-020")]
    public void Release_rejects_routing_for_another_product_type()
    {
        var result = Draft().Release(TestData.Routing(ProductType.Label, "PRINT-01"), TestData.Now);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    [Trait("Story", "PT-024")]
    public void Release_is_blocked_until_artwork_is_approved()
    {
        var wo = Draft(TestData.SafetySign(), legend: null);
        var routing = TestData.Routing(ProductType.SafetySign, "PREPRESS", "PRINT-01");

        var blocked = wo.Release(routing, TestData.Now);
        blocked.Error!.Code.Should().Be("WorkOrder.ArtworkNotApproved");
        blocked.Error.Message.Should().Be("Artwork not approved");

        wo.AddArtworkProof("key-1", "application/pdf", "proof.pdf", "planner", TestData.Now).IsSuccess.Should().BeTrue();
        wo.Release(routing, TestData.Now).Error!.Code.Should().Be("WorkOrder.ArtworkNotApproved");

        wo.ApproveArtwork(1, "planner", "ok", TestData.Now).IsSuccess.Should().BeTrue();
        wo.Release(routing, TestData.Now).IsSuccess.Should().BeTrue();
    }

    [Fact]
    [Trait("Story", "PT-023")]
    public void New_proof_supersedes_pending_proof_and_increments_version()
    {
        var wo = Draft(TestData.SafetySign(), legend: null);

        wo.AddArtworkProof("k1", "application/pdf", "v1.pdf", "planner", TestData.Now);
        var second = wo.AddArtworkProof("k2", "application/pdf", "v2.pdf", "planner", TestData.Now).Value;

        second.Version.Should().Be(2);
        wo.ArtworkProofs.Single(p => p.Version == 1).Status.Should().Be(ArtworkProofStatus.Superseded);
        wo.NextArtworkVersion.Should().Be(3);
    }

    [Fact]
    [Trait("Story", "PT-023")]
    public void Reject_requires_a_reason_and_only_pending_proofs_can_be_decided()
    {
        var wo = Draft(TestData.SafetySign(), legend: null);
        wo.AddArtworkProof("k1", "application/pdf", "v1.pdf", "planner", TestData.Now);

        wo.RejectArtwork(1, "planner", " ", TestData.Now).Error.Should().BeOfType<ValidationError>();
        wo.RejectArtwork(1, "planner", "Wrong colour", TestData.Now).IsSuccess.Should().BeTrue();
        wo.ApproveArtwork(1, "planner", null, TestData.Now).IsFailure.Should().BeTrue();
    }

    [Fact]
    [Trait("Story", "PT-019")]
    public void After_release_only_due_date_and_priority_can_change()
    {
        var wo = Draft();
        wo.Release(TestData.Routing(ProductType.PipeMarker, "PRINT-01"), TestData.Now);

        wo.UpdatePlanning(100, TestData.Today.AddDays(10), 1, "ACME", "STEAM").IsSuccess.Should().BeTrue();
        wo.UpdatePlanning(200, TestData.Today.AddDays(10), 1, "ACME", "STEAM").IsFailure.Should().BeTrue();
        wo.Priority.Should().Be(1);
        wo.Quantity.Should().Be(100);
    }

    [Fact]
    [Trait("Story", "PT-021")]
    public void Is_late_when_open_and_past_due()
    {
        var wo = Draft();

        wo.IsLate(TestData.Today.AddDays(5)).Should().BeFalse();
        wo.IsLate(TestData.Today.AddDays(6)).Should().BeTrue();
    }

    [Fact]
    [Trait("Story", "PT-019")]
    public void Only_drafts_can_be_deleted()
    {
        var wo = Draft();
        wo.CanBeDeleted.Should().BeTrue();

        wo.Release(TestData.Routing(ProductType.PipeMarker, "PRINT-01"), TestData.Now);

        wo.CanBeDeleted.Should().BeFalse();
    }
}
