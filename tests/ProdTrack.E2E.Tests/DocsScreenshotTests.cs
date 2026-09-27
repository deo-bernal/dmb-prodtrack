using System.Text.Json;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace ProdTrack.E2E.Tests;

/// <summary>
/// Captures the screenshots used in Documentations/ProdTrack-User-Guide.pdf from the running app. Runs only when
/// PRODTRACK_DOCS_SCREENSHOTS points to an output folder (otherwise it returns immediately).
/// </summary>
[Collection(E2ECollection.Name)]
[Trait("Category", "E2E")]
public sealed class DocsScreenshotTests(E2EServer server)
{
    private static readonly string? OutputFolder = Environment.GetEnvironmentVariable("PRODTRACK_DOCS_SCREENSHOTS");

    [Fact]
    public async Task Capture_user_guide_screenshots()
    {
        if (string.IsNullOrWhiteSpace(OutputFolder))
        {
            return;
        }

        Directory.CreateDirectory(OutputFolder);
        await Ui.WithFailureScreenshotAsync(server, nameof(Capture_user_guide_screenshots), CaptureAsync);
    }

    private static async Task ShotAsync(IPage page, string name, bool fullPage = false) =>
        await page.ScreenshotAsync(new() { Path = Path.Combine(OutputFolder!, name + ".png"), FullPage = fullPage });

    private async Task CaptureAsync(IPage page)
    {
        // Sign-in and forced password change.
        await page.GotoAsync("/account/login");
        await ShotAsync(page, "01-login");
        if (server.CurrentPassword == E2EServer.InitialPassword)
        {
            await page.Locator("#email").FillAsync(E2EServer.AdminEmail);
            await page.Locator("#password").FillAsync(server.CurrentPassword);
            await page.GetByTestId("login-submit").ClickAsync();
            await page.WaitForURLAsync("**/account/change-password");
            await ShotAsync(page, "02-change-password");
        }

        await Ui.SignInAsync(page, server);

        // Sales order with two lines, then work orders from it.
        await Ui.GotoInteractiveAsync(page, "/sales-orders/new");
        await page.GetByTestId("so-customer").FillAsync("Petron Bataan Refinery");
        await page.Locator("#so-po").FillAsync("PO-2026-0917");
        var rows = page.GetByTestId("so-lines").Locator("tbody tr");
        await SelectProductAsync(rows.Nth(0), "PM-VIN-FLAM-2");
        await rows.Nth(0).GetByLabel("Quantity").FillAsync("120");
        await rows.Nth(0).GetByLabel("Legend").FillAsync("FUEL GAS");
        await page.GetByTestId("so-add-line").ClickAsync();
        await SelectProductAsync(rows.Nth(1), "VT-BRASS-RND-38");
        await rows.Nth(1).GetByLabel("Quantity").FillAsync("40");
        await ShotAsync(page, "05-sales-order-new");
        await page.GetByTestId("so-save").ClickAsync();
        await Expect(page.GetByTestId("so-lines-table")).ToBeVisibleAsync();
        await Ui.WaitInteractiveAsync(page);
        await page.GetByTestId("so-create-work-orders").ClickAsync();
        await Expect(page.GetByTestId("so-notice")).ToBeVisibleAsync();
        await ShotAsync(page, "06-sales-order-detail", fullPage: true);

        await Ui.GotoInteractiveAsync(page, "/sales-orders");
        await ShotAsync(page, "04-sales-orders-list");

        // Work orders.
        await Ui.GotoInteractiveAsync(page, "/work-orders/new");
        await ShotAsync(page, "08-work-order-new");
        var artworkNumber = await Ui.CreateWorkOrderAsync(page, "SS-ALU-DANGER-A4", 30, "San Miguel Brewery");
        await ShotAsync(page, "09-work-order-draft-artwork", fullPage: true);

        var number = await Ui.CreateWorkOrderAsync(page, "VT-BRASS-RND-38", 40, "Meralco Substation");
        await page.GetByTestId("release").ClickAsync();
        await Expect(page.GetByTestId("operations-table")).ToBeVisibleAsync();
        var workOrderUrl = page.Url;
        await ShotAsync(page, "10-work-order-released", fullPage: true);

        await Ui.GotoInteractiveAsync(page, "/work-orders");
        await ShotAsync(page, "07-work-orders-list");

        // Traveler (print view).
        await page.GotoAsync(workOrderUrl + "/traveler");
        await Expect(page.GetByTestId("traveler")).ToBeVisibleAsync();
        await ShotAsync(page, "11-traveler", fullPage: true);

        // Hold and resume on a second released work order.
        var holdNumber = await Ui.CreateWorkOrderAsync(page, "PM-VIN-FLAM-2", 60, "Shell Tabangao", legend: "STEAM 150 PSI");
        await page.GetByTestId("release").ClickAsync();
        await Expect(page.GetByTestId("control-panel")).ToBeVisibleAsync();
        var holdSelect = page.GetByTestId("hold-reason-select");
        await holdSelect.SelectOptionAsync((await holdSelect.Locator("option", new() { HasTextString = "CUSTOMER-CHANGE" }).GetAttributeAsync("value"))!);
        await page.Locator("#hold-note").FillAsync("Customer revising legend text");
        await page.GetByTestId("hold").ClickAsync();
        await Expect(page.GetByTestId("hold-reason")).ToBeVisibleAsync();
        await ShotAsync(page, "12-work-order-on-hold");

        // Master data and admin screens.
        await Ui.GotoInteractiveAsync(page, "/routings");
        await ShotAsync(page, "13-routings");
        await Ui.GotoInteractiveAsync(page, "/routings/ValveTag/edit");
        await ShotAsync(page, "14-routing-editor", fullPage: true);
        await Ui.GotoInteractiveAsync(page, "/products");
        await ShotAsync(page, "15-products");
        await Ui.GotoInteractiveAsync(page, "/admin/stations");
        await ShotAsync(page, "16-stations");
        await Ui.GotoInteractiveAsync(page, "/admin/reason-codes");
        await ShotAsync(page, "17-reason-codes", fullPage: true);
        await Ui.GotoInteractiveAsync(page, "/admin/users/new");
        await page.GetByTestId("user-email").FillAsync("maria.santos@dmb.local");
        await page.GetByTestId("user-name").FillAsync("Maria Santos");
        await page.Locator("#u-badge").FillAsync("B-0142");
        await page.GetByTestId("role-Operator").CheckAsync();
        await page.GetByTestId("user-password").FillAsync((System.Guid.NewGuid().ToString("B").ToUpperInvariant() + System.Guid.NewGuid().ToString("N")));
        await ShotAsync(page, "19-user-new");
        await page.GetByTestId("user-save").ClickAsync();
        await Expect(page.GetByTestId("user-disable")).ToBeVisibleAsync();
        await ShotAsync(page, "20-user-edit", fullPage: true);
        await Ui.GotoInteractiveAsync(page, "/admin/users");
        await ShotAsync(page, "18-users");
        await Ui.GotoInteractiveAsync(page, "/qc/templates");
        await ShotAsync(page, "21-qc-checklists", fullPage: true);

        // Shop floor: station picker, queue, operation execution with scrap.
        await page.GotoAsync("/floor/");
        await Expect(page.GetByTestId("station-PREPRESS")).ToBeVisibleAsync(new() { Timeout = 60_000 });
        await ShotAsync(page, "22-floor-stations");
        await page.GetByTestId("station-PREPRESS").ClickAsync();
        await Expect(page.GetByTestId($"queue-item-{number}")).ToBeVisibleAsync();
        await ShotAsync(page, "23-floor-queue");
        await page.GetByTestId($"queue-item-{number}").ClickAsync();
        await Expect(page.GetByTestId("op-start")).ToBeVisibleAsync();
        await ShotAsync(page, "24-floor-operation-ready");
        await page.GetByTestId("op-start").ClickAsync();
        await Expect(page.GetByTestId("op-complete")).ToBeVisibleAsync();
        await page.GetByTestId("scrap-qty").FillAsync("2");
        var scrapSelect = page.GetByTestId("scrap-reason");
        await scrapSelect.SelectOptionAsync((await scrapSelect.Locator("option", new() { HasTextString = "MISPRINT" }).GetAttributeAsync("value"))!);
        await page.GetByTestId("scrap-submit").ClickAsync();
        await Expect(page.GetByTestId("scrap-list")).ToBeVisibleAsync();
        await ShotAsync(page, "25-floor-operation-in-progress", fullPage: true);
        await page.GetByTestId("op-good-input").FillAsync("38");
        await page.GetByTestId("op-complete").ClickAsync();
        await Expect(page.GetByTestId("op-done")).ToBeVisibleAsync();

        // Engrave step via scan, then the QC checklist at QC-01.
        await page.GotoAsync("/floor/station/ENGRAVE-01");
        await page.GetByTestId("scan-input").FillAsync("WO:WO-2026-999999");
        await page.GetByTestId("scan-input").PressAsync("Enter");
        await Expect(page.GetByTestId("scan-message")).ToBeVisibleAsync();
        await ShotAsync(page, "27-floor-scan-not-found");
        await page.GetByTestId("scan-input").FillAsync($"OP:{number}:20");
        await page.GetByTestId("scan-input").PressAsync("Enter");
        await page.GetByTestId("op-start").ClickAsync();
        await page.GetByTestId("op-good-input").FillAsync("38");
        await page.GetByTestId("op-complete").ClickAsync();
        await Expect(page.GetByTestId("op-done")).ToBeVisibleAsync();

        await page.GotoAsync("/floor/station/QC-01");
        await page.GetByTestId($"queue-item-{number}").ClickAsync();
        await page.GetByTestId("op-start").ClickAsync();
        await Expect(page.GetByTestId("qc-form")).ToBeVisibleAsync();
        await FillChecklistAsync(page);
        await ShotAsync(page, "26-floor-qc-checklist", fullPage: true);
        await page.GetByTestId("qc-submit").ClickAsync();
        await Expect(page.GetByTestId("op-notice")).ToHaveTextAsync("Inspection recorded.");

        // Dashboard after activity (live KPIs).
        await Ui.GotoInteractiveAsync(page, "/");
        await ShotAsync(page, "03-dashboard");
        _ = artworkNumber + holdNumber;
    }

    private static async Task SelectProductAsync(ILocator row, string sku)
    {
        var select = row.GetByLabel("Product");
        var value = await select.Locator("option", new() { HasTextString = sku }).GetAttributeAsync("value");
        await select.SelectOptionAsync(value!);
    }

    /// <summary>Answers every checklist item as a pass (mid-tolerance for measurements).</summary>
    private static async Task FillChecklistAsync(IPage page)
    {
        var id = Ui.OperationIdFromUrl(page.Url);
        var response = await page.Context.APIRequest.GetAsync($"/api/v1/operations/{id}");
        using var json = JsonDocument.Parse(await response.TextAsync());
        foreach (var item in json.RootElement.GetProperty("checklist").GetProperty("items").EnumerateArray())
        {
            var itemId = item.GetProperty("id").GetInt32();
            if (item.GetProperty("kind").GetString() == "Measured")
            {
                var mid = (item.GetProperty("minValue").GetDecimal() + item.GetProperty("maxValue").GetDecimal()) / 2;
                await page.GetByTestId($"qc-measure-{itemId}").FillAsync(mid.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            else
            {
                await page.GetByTestId($"qc-pass-{itemId}").ClickAsync();
            }
        }
    }
}
