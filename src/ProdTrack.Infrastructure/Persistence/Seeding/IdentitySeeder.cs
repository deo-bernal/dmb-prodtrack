using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ProdTrack.Application.Security;
using ProdTrack.Infrastructure.Identity;

namespace ProdTrack.Infrastructure.Persistence.Seeding;

/// <summary>Roles, the first-run bootstrap admin (PT-009) and Development-only picker users.</summary>
internal sealed partial class IdentitySeeder(
    UserManager<AppUser> userManager,
    RoleManager<IdentityRole> roleManager,
    ILogger<IdentitySeeder> logger)
{
    public const string DevUserDomain = DevUsers.EmailDomain;

    public async Task SeedRolesAsync()
    {
        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    /// <summary>Creates the bootstrap admin only when no user exists yet (empty database).</summary>
    public async Task SeedBootstrapAdminAsync(BootstrapAdminOptions options, bool anyUserExists)
    {
        if (anyUserExists || string.IsNullOrWhiteSpace(options.Email))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(options.Password))
        {
            LogBootstrapPasswordMissing(logger, options.Email);
            return;
        }

        var admin = new AppUser
        {
            UserName = options.Email,
            Email = options.Email,
            EmailConfirmed = true,
            DisplayName = "Administrator",
            MustChangePassword = true,
        };
        var result = await userManager.CreateAsync(admin, options.Password);
        if (!result.Succeeded)
        {
            LogBootstrapFailed(logger, string.Join("; ", result.Errors.Select(e => e.Description)));
            return;
        }

        await userManager.AddToRoleAsync(admin, Roles.Admin);
        LogBootstrapCreated(logger, options.Email);
    }

    /// <summary>One passwordless user per role for the Development user picker (Auth:Mode=Dev).</summary>
    public async Task SeedDevUsersAsync()
    {
        foreach (var role in Roles.All)
        {
            var email = $"{role.ToLowerInvariant()}@{DevUserDomain}";
            if (await userManager.FindByEmailAsync(email) is not null)
            {
                continue;
            }

            var user = new AppUser { UserName = email, Email = email, EmailConfirmed = true, DisplayName = $"Dev {role}" };
            var created = await userManager.CreateAsync(user);
            if (created.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Auth:BootstrapAdmin:Email is {Email} but no password is configured; bootstrap admin not created")]
    private static partial void LogBootstrapPasswordMissing(ILogger logger, string email);

    [LoggerMessage(Level = LogLevel.Error, Message = "Bootstrap admin could not be created: {Errors}")]
    private static partial void LogBootstrapFailed(ILogger logger, string errors);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Bootstrap admin {Email} created; the password must be changed at first sign-in. Remove Auth:BootstrapAdmin:Password from configuration.")]
    private static partial void LogBootstrapCreated(ILogger logger, string email);
}
