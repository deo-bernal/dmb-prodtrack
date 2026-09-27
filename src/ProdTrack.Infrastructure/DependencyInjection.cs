using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProdTrack.Application.Abstractions;
using ProdTrack.Infrastructure.Identity;
using ProdTrack.Infrastructure.Persistence;
using ProdTrack.Infrastructure.Persistence.Seeding;
using ProdTrack.Infrastructure.Secrets;
using ProdTrack.Infrastructure.Storage;
using ProdTrack.Infrastructure.Time;

namespace ProdTrack.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "ProdTrack";

    /// <summary>EF Core (SQL Server), Identity stores, Data Protection key store, number sequences, plant clock.</summary>
    public static IServiceCollection AddProdTrackInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        var connectionString = configuration.GetConnectionString(ConnectionStringName);

        services.AddDbContext<AppDbContext>(options =>
        {
            static void Configure(Microsoft.EntityFrameworkCore.Infrastructure.SqlServerDbContextOptionsBuilder sql) =>
                sql.EnableRetryOnFailure(maxRetryCount: 5)
                    .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                    .MigrationsHistoryTable("__EFMigrationsHistory", "dbo");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                options.UseSqlServer(Configure);
            }
            else
            {
                options.UseSqlServer(connectionString, Configure);
            }
        });
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<DomainEventDispatcher>();
        services.AddScoped<INumberSequenceGenerator, NumberSequenceGenerator>();
        services.AddScoped<DatabaseInitializer>();

        services.AddSingleton(TimeProvider.System);
        services.AddOptions<PlantOptions>().Bind(configuration.GetSection(PlantOptions.SectionName));
        services.AddSingleton<IPlantClock, PlantClock>();
        services.AddOptions<DatabaseOptions>().Bind(configuration.GetSection(DatabaseOptions.SectionName));
        services.AddOptions<BootstrapAdminOptions>().Bind(configuration.GetSection(BootstrapAdminOptions.SectionName));

        services.AddDataProtection().SetApplicationName("ProdTrack").PersistKeysToDbContext<AppDbContext>();
        return services;
    }

    /// <summary>Identity with admin-created users only, 12+ char passwords and lockout after 5 failures for 15 minutes.</summary>
    public static IdentityBuilder AddProdTrackIdentityStores(this IServiceCollection services)
    {
        return services.AddIdentityCore<AppUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 12;
                options.Password.RequireNonAlphanumeric = false;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddSignInManager<AppSignInManager>()
            .AddClaimsPrincipalFactory<AppUserClaimsPrincipalFactory>()
            .AddDefaultTokenProviders();
    }

    /// <summary>Local (default) host adapters: disk file storage and configuration secrets (docs/02 section 10).</summary>
    public static IServiceCollection AddProdTrackLocalProviders(this IServiceCollection services, IConfiguration configuration, string contentRootPath)
    {
        services.AddOptions<LocalFileStorageOptions>()
            .Bind(configuration.GetSection(LocalFileStorageOptions.SectionName))
            .PostConfigure(o => o.RootPath = Path.IsPathRooted(o.RootPath) ? o.RootPath : Path.Combine(contentRootPath, o.RootPath));
        services.AddSingleton<IFileStorage, LocalFileStorage>();
        services.AddSingleton<ISecretProvider, ConfigurationSecretProvider>();
        return services;
    }
}
