using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace ProdTrack.E2E.Tests;

[Collection(E2ECollection.Name)]
[Trait("Category", "E2E")]
[Trait("Story", "PT-029")]
public sealed class ProductionFlowTests(E2EServer server)
{
    [Fact]
    public Task Sign_in_create_and_release_a_work_order_then_run_an_operation_on_the_floor() =>
        Ui.WithFailureScreenshotAsync(server, nameof(Sign_in_create_and_release_a_work_order_then_run_an_operation_on_the_floor), RunProductionFlowAsync);

    private async Task RunProductionFlowAsync(IPage page)
    {

        // Sign in (bootstrap admin, forced password change on first sign-in).
        await Ui.SignInAsync(page, server);

        // Create and release a work order (valve tag: no artwork gate).
        var number = await Ui.CreateWorkOrderAsync(page, "VT-BRASS-RND-38", 25, "E2E Refinery");
        number.Should().NotBeNullOrEmpty();
        await page.GetByTestId("release").ClickAsync();
        await Expect(page.GetByTestId("operations-table")).ToBeVisibleAsync();
        await Expect(page.GetByTestId("status-badge").First).ToHaveTextAsync("Released");

        // Shop floor PWA: pick the first station, open the work order from the queue, start and complete it.
        await page.GotoAsync("/floor/");
        await page.GetByTestId("station-PREPRESS").ClickAsync(new() { Timeout = 60_000 });
        await Expect(page.GetByTestId("station-queue")).ToBeVisibleAsync();
        await page.GetByTestId($"queue-item-{number}").ClickAsync();
        await Expect(page.GetByTestId("op-title")).ToContainTextAsync(number);

        await page.GetByTestId("op-start").ClickAsync();
        await Expect(page.GetByTestId("op-notice")).ToHaveTextAsync("Started.");
        await page.GetByTestId("op-good-input").FillAsync("25");
        await page.GetByTestId("op-complete").ClickAsync();
        await Expect(page.GetByTestId("op-notice")).ToContainTextAsync("Step completed");
        await Expect(page.GetByTestId("op-done")).ToContainTextAsync("completed");

        // Back office sees the progress: step 10 completed, step 20 ready, work order in progress.
        await page.GotoAsync("/work-orders");
        await page.GetByRole(AriaRole.Link, new() { Name = number }).ClickAsync();
        await Expect(page.GetByTestId("operations-table")).ToContainTextAsync("Completed");
        await Expect(page.GetByTestId("status-badge").First).ToHaveTextAsync("In progress");
    }

    [Fact]
    public Task Scan_box_opens_an_operation_by_code() =>
        Ui.WithFailureScreenshotAsync(server, nameof(Scan_box_opens_an_operation_by_code), RunScanAsync);

    private async Task RunScanAsync(IPage page)
    {
        await Ui.SignInAsync(page, server);
        var number = await Ui.CreateWorkOrderAsync(page, "VT-BRASS-RND-38", 5, "Scan Co.");
        await page.GetByTestId("release").ClickAsync();
        await Expect(page.GetByTestId("operations-table")).ToBeVisibleAsync();

        await page.GotoAsync("/floor/station/PREPRESS");
        await page.GetByTestId("scan-input").FillAsync($"OP:{number}:10", new() { Timeout = 60_000 });
        await page.GetByTestId("scan-input").PressAsync("Enter");

        await Expect(page.GetByTestId("op-title")).ToContainTextAsync(number);
        await Expect(page.GetByTestId("op-start")).ToBeVisibleAsync();
    }
}
