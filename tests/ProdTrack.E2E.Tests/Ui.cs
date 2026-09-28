using System.Text.RegularExpressions;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace ProdTrack.E2E.Tests;

/// <summary>Shared UI steps.</summary>
internal static partial class Ui
{
    /// <summary>Signs in as the bootstrap admin, changing the initial password when the app asks for it.</summary>
    public static async Task SignInAsync(IPage page, E2EServer server)
    {
        await page.GotoAsync("/account/login");
        await page.Locator("#email").FillAsync(E2EServer.AdminEmail);
        await page.Locator("#password").FillAsync(server.CurrentPassword);
        await page.GetByTestId("login-submit").ClickAsync();
        await page.WaitForURLAsync(url => !url.Contains("/account/login", StringComparison.Ordinal));

        if (page.Url.Contains("/account/change-password", StringComparison.Ordinal))
        {
            await page.Locator("#old").FillAsync(server.CurrentPassword);
            await page.Locator("#new").FillAsync(E2EServer.NewPassword);
            await page.Locator("#confirm").FillAsync(E2EServer.NewPassword);
            await page.GetByRole(AriaRole.Button, new() { Name = "Update password" }).ClickAsync();
            await page.WaitForURLAsync(url => !url.Contains("/account/", StringComparison.Ordinal));
            server.CurrentPassword = E2EServer.NewPassword;
        }

        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Dashboard" })).ToBeVisibleAsync();
    }

    /// <summary>Runs a browser scenario on a fresh page; on failure saves a full-page screenshot to %TEMP%/prodtrack-e2e-failures.</summary>
    public static async Task WithFailureScreenshotAsync(E2EServer server, string name, Func<IPage, Task> scenario)
    {
        var page = await server.NewPageAsync();
        try
        {
            await scenario(page);
        }
        catch (Exception)
        {
            var folder = Path.Combine(Path.GetTempPath(), "prodtrack-e2e-failures");
            Directory.CreateDirectory(folder);
            await page.ScreenshotAsync(new() { Path = Path.Combine(folder, $"{name}.png"), FullPage = true });
            throw;
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    /// <summary>Navigates and waits until the Blazor Server circuit is connected (interactive handlers attached).</summary>
    public static async Task GotoInteractiveAsync(IPage page, string path)
    {
        await page.GotoAsync(path);
        await WaitInteractiveAsync(page);
    }

    /// <summary>Waits for the marker rendered by ErrorAlert once the Blazor Server circuit is interactive.</summary>
    public static Task WaitInteractiveAsync(IPage page) =>
        page.Locator("[data-interactive]").First.WaitForAsync(new() { State = WaitForSelectorState.Attached });

    /// <summary>Creates a Draft work order through the UI and returns its number (WO-yyyy-nnnnnn).</summary>
    public static async Task<string> CreateWorkOrderAsync(IPage page, string sku, int quantity, string customer, string? legend = null)
    {
        await GotoInteractiveAsync(page, "/work-orders/new");
        var product = page.Locator("#product");
        var value = await product.Locator("option", new() { HasTextString = sku }).GetAttributeAsync("value");
        await product.SelectOptionAsync(value!);
        await page.Locator("#qty").FillAsync(quantity.ToString(System.Globalization.CultureInfo.InvariantCulture));
        await page.Locator("#cust").FillAsync(customer);
        if (legend is not null)
        {
            await page.Locator("#legend").FillAsync(legend);
        }

        await page.GetByTestId("create-work-order").ClickAsync();
        await Expect(page.Locator("h1").First).ToContainTextAsync(WorkOrderNumber());
        await WaitInteractiveAsync(page);
        var heading = await page.Locator("h1").First.InnerTextAsync();
        return WorkOrderNumber().Match(heading).Value;
    }

    public static int OperationIdFromUrl(string url) =>
        int.Parse(OperationUrl().Match(url).Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);

    [GeneratedRegex(@"/operations/(\d+)")]
    private static partial Regex OperationUrl();

    [GeneratedRegex(@"WO-\d{4}-\d{6}")]
    public static partial Regex WorkOrderNumber();
}
