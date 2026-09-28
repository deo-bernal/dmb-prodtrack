using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.Playwright;
using ProdTrack.TestSupport;

namespace ProdTrack.E2E.Tests;

/// <summary>
/// Hosts the real Server on Kestrel with Identity sign-in and a seeded SQLite in-memory database, and launches
/// headless Chromium. One server + browser per test class.
/// </summary>
public sealed class E2EServer : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string AdminEmail = "admin@prodtrack.e2e";
    public static readonly string InitialPassword = TestPasswords.Generate();
    public static readonly string NewPassword = TestPasswords.Generate();

    private readonly SqliteConnection _connection = SqliteDbContextExtensions.OpenInMemoryConnection();
    private readonly string _storageRoot = Path.Combine(Path.GetTempPath(), "prodtrack-e2e", Guid.NewGuid().ToString("N"));
    private readonly int _port = FreePort();
    private IPlaywright? _playwright;

    public string BaseUrl => $"http://localhost:{_port}";

    public IBrowser Browser { get; private set; } = null!;

    /// <summary>The bootstrap admin must change the password at first sign-in; tracks the current one.</summary>
    public string CurrentPassword { get; set; } = InitialPassword;

    public async Task InitializeAsync()
    {
        await SqliteDbContextExtensions.EnsureSchemaAsync(_connection);
        UseKestrel(_port);
        StartServer();

        PlaywrightInstaller.EnsureChromium();
        _playwright = await Playwright.CreateAsync();
        var headed = Environment.GetEnvironmentVariable("PRODTRACK_E2E_HEADED") == "1";
        Browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = !headed });
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        if (Browser is not null)
        {
            await Browser.DisposeAsync();
        }

        _playwright?.Dispose();
        await base.DisposeAsync();
        await _connection.DisposeAsync();
        if (Directory.Exists(_storageRoot))
        {
            Directory.Delete(_storageRoot, recursive: true);
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseStaticWebAssets();
        builder.UseSetting("Auth:Mode", "Identity");
        builder.UseSetting("LogFile:Enabled", Environment.GetEnvironmentVariable("PRODTRACK_E2E_SERVER_LOG") is null ? "false" : "true");
        builder.UseSetting("LogFile:Path", Environment.GetEnvironmentVariable("PRODTRACK_E2E_SERVER_LOG") ?? string.Empty);
        builder.UseSetting("Database:SeedDemoData", "true");
        builder.UseSetting("Database:MigrateOnStartup", "false");
        builder.UseSetting("Auth:BootstrapAdmin:Email", AdminEmail);
        builder.UseSetting("Auth:BootstrapAdmin:Password", InitialPassword);
        builder.UseSetting("Storage:Local:RootPath", _storageRoot);
        builder.ConfigureTestServices(services => services.UseSqliteAppDbContext(_connection));
    }

    public async Task<IPage> NewPageAsync()
    {
        var context = await Browser.NewContextAsync(new BrowserNewContextOptions { BaseURL = BaseUrl, ViewportSize = new ViewportSize { Width = 1366, Height = 900 } });
        context.SetDefaultTimeout(30_000);
        var page = await context.NewPageAsync();
        var log = Environment.GetEnvironmentVariable("PRODTRACK_E2E_BROWSER_LOG");
        if (!string.IsNullOrEmpty(log))
        {
            page.Console += (_, message) => File.AppendAllText(log, $"console {message.Type}: {message.Text}{Environment.NewLine}");
            page.RequestFailed += (_, request) => File.AppendAllText(log, $"failed {request.Url} {request.Failure}{Environment.NewLine}");
            page.Response += (_, response) =>
            {
                if (response.Status >= 400)
                {
                    File.AppendAllText(log, $"http {response.Status} {response.Url}{Environment.NewLine}");
                }
            };
        }

        return page;
    }

    private static int FreePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }
}

[CollectionDefinition(Name)]
public sealed class E2ECollection : ICollectionFixture<E2EServer>
{
    public const string Name = "E2E";
}
