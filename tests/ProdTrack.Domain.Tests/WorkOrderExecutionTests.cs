using ProdTrack.Domain.Common;
using ProdTrack.Domain.Products;
using ProdTrack.Domain.Quality;
using ProdTrack.Domain.ReasonCodes;
using ProdTrack.Domain.WorkOrders;

namespace ProdTrack.Domain.Tests;

[Trait("Category", "Unit")]
public class WorkOrderExecutionTests
{
    private const string User = "op-1";

    private static readonly ReasonCode CustomerChange = TestData.WithId(ReasonCode.Create("CUSTOMER-CHANGE", "Customer change", ReasonCategory.Hold).Value, 1);
    private static readonly ReasonCode Break = TestData.WithId(ReasonCode.Create("BREAK", "Break", ReasonCategory.Pause).Value, 2);
    private static readonly ReasonCode Misprint = TestData.WithId(ReasonCode.Create("MISPRINT", "Misprint", ReasonCategory.Scrap).Value, 3);

    /// <summary>Released 100-piece pipe marker with three steps (ids 101, 102, 103).</summary>
    private static WorkOrder Released(int quantity = 100)
    {
        var wo = WorkOrder.Create("WO-2026-000001", TestData.PipeMarker(), quantity, TestData.Today.AddDays(5), 3, "ACME", "STEAM", TestData.Now).Value;
        wo.Release(TestData.Routing(ProductType.PipeMarker, "PRINT-01", "LAM-01", "PACK-01"), TestData.Now).IsSuccess.Should().BeTrue();
        for (var i = 0; i < wo.Operations.Count; i++)
        {
            TestData.WithId(wo.Operations[i], 101 + i);
        }

        wo.ClearDomainEvents();
        return wo;
    }

    [Fact]
    [Trait("Story", "PT-031")]
    public void Start_moves_ready_step_to_in_progress_and_work_order_to_in_progress()
    {
        var wo = Released();

        wo.StartOperation(101, User, TestData.Now).IsSuccess.Should().BeTrue();

        wo.Operations[0].Status.Should().Be(OperationStatus.InProgress);
        wo.Status.Should().Be(WorkOrderStatus.InProgress);
        wo.DomainEvents.Should().Contain(e => e is OperationChanged);
    }

    [Fact]
    [Trait("Story", "PT-031")]
    public void Cannot_start_a_pending_step_before_the_previous_completes()
    {
        var wo = Released();

        wo.StartOperation(102, User, TestData.Now).Error!.Code.Should().Be("Operation.PreviousStepNotComplete");
    }

    [Fact]
    [Trait("Story", "PT-032")]
    public void Complete_sets_next_step_ready_with_good_quantity_as_input()
    {
        var wo = Released();
        wo.StartOperation(101, User, TestData.Now);
        wo.LogScrap(101, 4, Misprint, "smudge", User, TestData.Now).IsSuccess.Should().BeTrue();

        wo.CompleteOperation(101, 96, User, TestData.Now.AddMinutes(30)).IsSuccess.Should().BeTrue();

        wo.Operations[0].Status.Should().Be(OperationStatus.Completed);
        wo.Operations[0].ScrapQuantity.Should().Be(4);
        wo.Operations[1].Status.Should().Be(OperationStatus.Ready);
        wo.Operations[1].InputQuantity.Should().Be(96);
    }

    [Fact]
    [Trait("Story", "PT-032")]
    public void Good_plus_scrap_cannot_exceed_input()
    {
        var wo = Released();
        wo.StartOperation(101, User, TestData.Now);
        wo.LogScrap(101, 10, Misprint, null, User, TestData.Now);

        wo.CompleteOperation(101, 91, User, TestData.Now).Error.Should().BeOfType<ValidationError>();
    }

    [Fact]
    [Trait("Story", "PT-033")]
    public void Scrap_requires_scrap_category_reason_and_positive_quantity()
    {
        var wo = Released();
        wo.StartOperation(101, User, TestData.Now);

        wo.LogScrap(101, 0, Misprint, null, User, TestData.Now).IsFailure.Should().BeTrue();
        wo.LogScrap(101, 1, Break, null, User, TestData.Now).IsFailure.Should().BeTrue();
    }

    [Fact]
    [Trait("Story", "PT-031")]
    public void Pause_and_resume_require_pause_reason()
    {
        var wo = Released();
        wo.StartOperation(101, User, TestData.Now);

        wo.PauseOperation(101, Misprint, User, TestData.Now).IsFailure.Should().BeTrue();
        wo.PauseOperation(101, Break, User, TestData.Now).IsSuccess.Should().BeTrue();
        wo.Operations[0].Status.Should().Be(OperationStatus.Paused);
        wo.ResumeOperation(101, User, TestData.Now).IsSuccess.Should().BeTrue();
        wo.Operations[0].Status.Should().Be(OperationStatus.InProgress);
    }

    [Fact]
    [Trait("Story", "PT-032")]
    public void Completing_the_last_step_completes_the_work_order_with_good_quantity()
    {
        var wo = Released(10);
        foreach (var id in new[] { 101, 102, 103 })
        {
            wo.StartOperation(id, User, TestData.Now).IsSuccess.Should().BeTrue();
            wo.CompleteOperation(id, id == 103 ? 9 : 10, User, TestData.Now).IsSuccess.Should().BeTrue();
        }

        wo.Status.Should().Be(WorkOrderStatus.Completed);
        wo.CompletedQuantity.Should().Be(9);
    }

    [Fact]
    [Trait("Story", "PT-022")]
    public void Hold_blocks_execution_and_resume_restores_previous_status()
    {
        var wo = Released();
        wo.StartOperation(101, User, TestData.Now);

        wo.Hold(CustomerChange, "customer revising legend").IsSuccess.Should().BeTrue();

        wo.Status.Should().Be(WorkOrderStatus.OnHold);
        wo.HoldReason.Should().Contain("CUSTOMER-CHANGE");
        wo.CompleteOperation(101, 100, User, TestData.Now).Error!.Code.Should().Be("WorkOrder.OnHold");
        wo.Resume().IsSuccess.Should().BeTrue();
        wo.Status.Should().Be(WorkOrderStatus.InProgress);
        wo.HoldReason.Should().BeNull();
    }

    [Fact]
    [Trait("Story", "PT-022")]
    public void Hold_requires_hold_category_reason_and_released_status()
    {
        Released().Hold(Break, null).IsFailure.Should().BeTrue();

        var draft = WorkOrder.Create("WO-2026-000002", TestData.PipeMarker(), 5, TestData.Today, 3, null, "X", TestData.Now).Value;
        draft.Hold(CustomerChange, null).Error!.Code.Should().Be("WorkOrder.InvalidStatusTransition");
    }

    [Fact]
    [Trait("Story", "PT-022")]
    public void Cancel_requires_reason_and_no_completed_steps()
    {
        var wo = Released();
        wo.Cancel(" ").Error.Should().BeOfType<ValidationError>();

        wo.StartOperation(101, User, TestData.Now);
        wo.CompleteOperation(101, 100, User, TestData.Now);

        wo.Cancel("customer cancelled").Error!.Code.Should().Be("WorkOrder.HasCompletedOperations");
    }

    [Fact]
    [Trait("Story", "PT-022")]
    public void Cancel_skips_open_operations()
    {
        var wo = Released();

        wo.Cancel("duplicate order").IsSuccess.Should().BeTrue();

        wo.Status.Should().Be(WorkOrderStatus.Cancelled);
        wo.CancelReason.Should().Be("duplicate order");
        wo.Operations.Should().OnlyContain(o => o.Status == OperationStatus.Skipped);
    }

    [Fact]
    [Trait("Story", "PT-034")]
    public void Qc_inspection_fails_when_a_measurement_is_out_of_tolerance()
    {
        var template = QcChecklistTemplate.Create(ProductType.PipeMarker, "Pipe marker QC",
        [
            new QcChecklistItemDefinition(1, "Legend matches", QcItemKind.PassFail, null, null, null),
            new QcChecklistItemDefinition(2, "Width", QcItemKind.Measured, 99m, 101m, "mm"),
        ]).Value;
        TestData.WithId(template.Items[0], 1);
        TestData.WithId(template.Items[1], 2);

        var pass = QcInspection.Record(5, template, [new(1, true, null), new(2, null, 100.4m)], 5, User, null, TestData.Now).Value;
        var fail = QcInspection.Record(5, template, [new(1, true, null), new(2, null, 102m)], 5, User, "too wide", TestData.Now).Value;
        var missing = QcInspection.Record(5, template, [new(1, true, null)], 5, User, null, TestData.Now);

        pass.Result.Should().Be(QcResult.Passed);
        fail.Result.Should().Be(QcResult.Failed);
        fail.Disposition.Should().Be(QcDisposition.Hold);
        missing.Error.Should().BeOfType<ValidationError>();
    }
}
