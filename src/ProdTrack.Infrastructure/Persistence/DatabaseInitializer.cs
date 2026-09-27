using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProdTrack.Infrastructure.Identity;
using ProdTrack.Infrastructure.Persistence.Seeding;

namespace ProdTrack.Infrastructure.Persistence;

/// <summary>
/// Startup database work: optional migration, then idempotent seeding (reference data, roles, bootstrap admin,
/// dev users, demo data). Never throws when the database is unreachable, so /health/ready can report it.
/// </summary>
public sealed partial class DatabaseInitializer(
    AppDbContext db,
    UserManager<AppUser> userManager,
    RoleManager<IdentityRole> roleManager,
    TimeProvider timeProvider,
    IOptions<DatabaseOptions> databaseOptions,
    IOptions<BootstrapAdminOptions> bootstrapOptions,
    ILoggerFactory loggerFactory)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<DatabaseInitializer>();

    public async Task InitializeAsync(bool seedDevUsers, CancellationToken cancellationToken)
    {
        try
        {
            if (databaseOptions.Value.MigrateOnStartup)
            {
                await db.Database.MigrateAsync(cancellationToken);
            }

            // Probe the schema; when the database is missing or not migrated we skip seeding.
            _ = await db.Stations.AnyAsync(cancellationToken);
        }
#pragma warning disable CA1031 // Any failure here (missing DB, retry limit, not migrated) must not stop the host; /health/ready reports it.
        catch (Exception ex) when (ex is not OperationCanceledException)
#pragma warning restore CA1031
        {
            LogDatabaseUnavailable(_logger, ex);
            return;
        }

        await ReferenceDataSeeder.SeedAsync(db, timeProvider, cancellationToken);
        if (databaseOptions.Value.SeedDemoData)
        {
            await DemoDataSeeder.SeedAsync(db, cancellationToken);
        }

        var identity = new IdentitySeeder(userManager, roleManager, loggerFactory.CreateLogger<IdentitySeeder>());
        await identity.SeedRolesAsync();
        var anyUser = await userManager.Users.AnyAsync(u => !u.Email!.EndsWith("@" + IdentitySeeder.DevUserDomain), cancellationToken);
        await identity.SeedBootstrapAdminAsync(bootstrapOptions.Value, anyUser);
        if (seedDevUsers)
        {
            await identity.SeedDevUsersAsync();
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Database not reachable or not migrated; skipping seeding. Run 'dotnet ef database update' or set Database:MigrateOnStartup=true")]
    private static partial void LogDatabaseUnavailable(ILogger logger, Exception exception);
}
