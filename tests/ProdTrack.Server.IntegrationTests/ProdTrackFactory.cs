using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using ProdTrack.Infrastructure.Persistence;
using ProdTrack.TestSupport;

namespace ProdTrack.Server.IntegrationTests;

/// <summary>
/// Runs the real Server pipeline in the Testing environment with Auth:Mode=Test (header-based users) and a shared
/// SQLite in-memory database per factory.
/// </summary>
public sealed class ProdTrackFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string BootstrapEmail = "admin@prodtrack.test";
    public const string BootstrapPassword = "Bootstrap!Pass2026";

    private readonly SqliteConnection _connection = SqliteDbContextExtensions.OpenInMemoryConnection();
    private readonly string _storageRoot = Path.Combine(Path.GetTempPath(), "prodtrack-it", Guid.NewGuid().ToString("N"));

    public async Task InitializeAsync() => await SqliteDbContextExtensions.EnsureSchemaAsync(_connection);

    async Task IAsyncLifetime.DisposeAsync()
    {
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
        builder.UseSetting("Auth:Mode", "Test");
        builder.UseSetting("LogFile:Enabled", "false");
        builder.UseSetting("Database:SeedDemoData", "true");
        builder.UseSetting("Database:MigrateOnStartup", "false");
        builder.UseSetting("Auth:BootstrapAdmin:Email", BootstrapEmail);
        builder.UseSetting("Auth:BootstrapAdmin:Password", BootstrapPassword);
        builder.UseSetting("Storage:Local:RootPath", _storageRoot);
        builder.ConfigureTestServices(services => services.UseSqliteAppDbContext(_connection));
    }

    /// <summary>Client authenticated through the Test scheme as a user with the given roles.</summary>
    public HttpClient CreateClientAs(params string[] roles)
    {
        var client = CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, BaseAddress = new Uri("https://localhost") });
        client.DefaultRequestHeaders.Add("X-Test-User", $"it-{string.Join('-', roles).ToLowerInvariant()}");
        client.DefaultRequestHeaders.Add("X-Test-Roles", string.Join(',', roles));
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return client;
    }

    public HttpClient CreateAnonymousClient() =>
        CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, BaseAddress = new Uri("https://localhost"), HandleCookies = true });

    public async Task<T> WithDbAsync<T>(Func<AppDbContext, Task<T>> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        await using var scope = Services.CreateAsyncScope();
        return await action(scope.ServiceProvider.GetRequiredService<AppDbContext>());
    }
}

[CollectionDefinition(Name)]
public sealed class ServerCollection : ICollectionFixture<ProdTrackFactory>
{
    public const string Name = "Server";
}
