using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using ProdTrack.Application;
using ProdTrack.Application.Abstractions;
using ProdTrack.Application.Messaging;
using ProdTrack.Domain.Common;
using ProdTrack.Infrastructure;
using ProdTrack.Infrastructure.Persistence;
using ProdTrack.Infrastructure.Persistence.Seeding;

namespace ProdTrack.TestSupport;

/// <summary>
/// Application + Infrastructure wired against SQLite in-memory with fakes for the host (user, clock, storage,
/// notifier). Each instance is an isolated database seeded with reference data.
/// </summary>
public sealed class TestApplication : IAsyncDisposable
{
    private readonly SqliteConnection _connection;

    private TestApplication(SqliteConnection connection, ServiceProvider services)
    {
        _connection = connection;
        Services = services;
    }

    /// <summary>Plant time is 2026-03-10 08:00 Asia/Manila.</summary>
    public static readonly DateTimeOffset DefaultNow = new(2026, 3, 10, 0, 0, 0, TimeSpan.Zero);

    public ServiceProvider Services { get; }

    public TestCurrentUser User => Services.GetRequiredService<TestCurrentUser>();

    public FakeTimeProvider Clock => Services.GetRequiredService<FakeTimeProvider>();

    public InMemoryFileStorage Files => Services.GetRequiredService<InMemoryFileStorage>();

    public RecordingNotifier Notifier => Services.GetRequiredService<RecordingNotifier>();

    public static async Task<TestApplication> CreateAsync(bool seedReferenceData = true, bool seedDemoData = false)
    {
        var connection = SqliteDbContextExtensions.OpenInMemoryConnection();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Plant:TimeZone"] = "Asia/Manila",
        }).Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddSingleton(NullLoggerFactory.Instance);
        services.AddProdTrackApplication();
        services.AddProdTrackInfrastructure(configuration);
        services.UseSqliteAppDbContext(connection);

        var clock = new FakeTimeProvider(DefaultNow);
        services.AddSingleton(clock);
        services.AddSingleton<TimeProvider>(clock);
        services.AddSingleton<TestCurrentUser>();
        services.AddSingleton<ICurrentUser>(sp => sp.GetRequiredService<TestCurrentUser>());
        services.AddScoped<IAuthorizationChecker, TestAuthorizationChecker>();
        services.AddSingleton<ICorrelationIdAccessor, TestCorrelationIdAccessor>();
        services.AddSingleton<InMemoryFileStorage>();
        services.AddSingleton<IFileStorage>(sp => sp.GetRequiredService<InMemoryFileStorage>());
        services.AddSingleton<RecordingNotifier>();
        services.AddSingleton<INotifier>(sp => sp.GetRequiredService<RecordingNotifier>());

        var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        var app = new TestApplication(connection, provider);
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();
        if (seedReferenceData)
        {
            await ReferenceDataSeeder.SeedAsync(db, clock, CancellationToken.None);
        }

        if (seedDemoData)
        {
            await DemoDataSeeder.SeedAsync(db, CancellationToken.None);
        }

        return app;
    }

    public async Task<Result<T>> SendAsync<T>(ICommand<T> command)
    {
        await using var scope = Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IDispatcher>().SendAsync(command, CancellationToken.None);
    }

    public async Task<Result<T>> QueryAsync<T>(IQuery<T> query)
    {
        await using var scope = Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IDispatcher>().QueryAsync(query, CancellationToken.None);
    }

    /// <summary>Runs an action against a fresh DbContext (for arranging data and asserting persisted state).</summary>
    public async Task<T> WithDbAsync<T>(Func<AppDbContext, Task<T>> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        await using var scope = Services.CreateAsyncScope();
        return await action(scope.ServiceProvider.GetRequiredService<AppDbContext>());
    }

    public async ValueTask DisposeAsync()
    {
        await Services.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
