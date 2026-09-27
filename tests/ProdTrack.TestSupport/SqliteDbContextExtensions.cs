using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using ProdTrack.Infrastructure;
using ProdTrack.Infrastructure.Persistence;

namespace ProdTrack.TestSupport;

public static class SqliteDbContextExtensions
{
    /// <summary>Replaces the SQL Server registration of <see cref="AppDbContext"/> with SQLite on the given open connection.</summary>
    public static IServiceCollection UseSqliteAppDbContext(this IServiceCollection services, SqliteConnection connection)
    {
        foreach (var descriptor in services.Where(d => d.ServiceType == typeof(IDbContextOptionsConfiguration<AppDbContext>)).ToList())
        {
            services.Remove(descriptor);
        }

        services.AddDbContext<AppDbContext>(options => options
            .UseSqlite(connection)
            .ReplaceService<IModelCustomizer, SqliteModelCustomizer>()
            .AddInterceptors(new SqliteRowVersionInterceptor())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));
        return services;
    }

    public static SqliteConnection OpenInMemoryConnection()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        return connection;
    }

    /// <summary>Creates the schema on the connection (used before a WebApplicationFactory host runs its startup seeding).</summary>
    public static async Task EnsureSchemaAsync(SqliteConnection connection)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddProdTrackInfrastructure(new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build());
        services.UseSqliteAppDbContext(connection);
        services.AddSingleton<ProdTrack.Application.Abstractions.ICurrentUser, TestCurrentUser>();
        services.AddSingleton<ProdTrack.Application.Abstractions.ICorrelationIdAccessor, TestCorrelationIdAccessor>();
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreatedAsync();
    }
}
