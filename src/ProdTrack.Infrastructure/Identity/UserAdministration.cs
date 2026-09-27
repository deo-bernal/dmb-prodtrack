using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProdTrack.Application.Users;
using ProdTrack.Domain.Common;
using ProdTrack.Infrastructure.Persistence;

namespace ProdTrack.Infrastructure.Identity;

/// <summary>PT-012 on top of UserManager. Multi-step changes (user + roles) run in one transaction.</summary>
internal sealed class UserAdministration(UserManager<AppUser> userManager, AppDbContext db, TimeProvider timeProvider) : IUserAdministration
{
    public async Task<IReadOnlyList<UserSummaryModel>> ListAsync(CancellationToken cancellationToken)
    {
        var users = await db.Users.AsNoTracking().OrderBy(u => u.DisplayName).ThenBy(u => u.Email).ToListAsync(cancellationToken);
        var roles = await RoleMapAsync(null, cancellationToken);
        var now = timeProvider.GetUtcNow();
        return [.. users.Select(u => new UserSummaryModel(
            u.Id,
            u.Email ?? u.UserName ?? u.Id,
            u.DisplayName,
            u.EmployeeNumber,
            u.BadgeNumber,
            u.HomeStationCode,
            u.IsActive,
            u.LockoutEnd > now,
            u.MustChangePassword,
            roles.GetValueOrDefault(u.Id) ?? []))];
    }

    public async Task<UserDetailModel?> GetAsync(string id, CancellationToken cancellationToken)
    {
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var roles = await RoleMapAsync(id, cancellationToken);
        return new UserDetailModel(
            user.Id,
            user.Email ?? user.UserName ?? user.Id,
            user.DisplayName,
            user.EmployeeNumber,
            user.BadgeNumber,
            user.HomeStationCode,
            user.IsActive,
            user.LockoutEnd > timeProvider.GetUtcNow(),
            user.MustChangePassword,
            roles.GetValueOrDefault(user.Id) ?? [],
            user.ConcurrencyStamp ?? string.Empty);
    }

    public async Task<Result<string>> CreateAsync(NewUser user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        var email = user.Email.Trim();
        if (await userManager.FindByEmailAsync(email) is not null)
        {
            return UserErrors.DuplicateEmail;
        }

        var profileError = await CheckProfileAsync(null, user.BadgeNumber, user.HomeStationCode, cancellationToken);
        if (profileError is not null)
        {
            return profileError;
        }

        var appUser = new AppUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            DisplayName = user.DisplayName.Trim(),
            EmployeeNumber = Normalize(user.EmployeeNumber),
            BadgeNumber = Normalize(user.BadgeNumber),
            HomeStationCode = Normalize(user.HomeStationCode)?.ToUpperInvariant(),
            MustChangePassword = true,
        };

        return await InTransactionAsync<string>(
            async () =>
            {
                var created = await userManager.CreateAsync(appUser, user.TemporaryPassword);
                if (!created.Succeeded)
                {
                    return ToError(created, "temporaryPassword");
                }

                var roles = await userManager.AddToRolesAsync(appUser, user.Roles.Distinct(StringComparer.Ordinal));
                return roles.Succeeded ? appUser.Id : ToError(roles, "roles");
            },
            cancellationToken);
    }

    public async Task<Result> UpdateAsync(string id, UserProfileUpdate update, string? expectedVersion, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(update);
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return UserErrors.NotFound(id);
        }

        if (!string.IsNullOrEmpty(expectedVersion) && !string.Equals(expectedVersion, user.ConcurrencyStamp, StringComparison.Ordinal))
        {
            return ConcurrencyErrors.StaleVersion;
        }

        var profileError = await CheckProfileAsync(id, update.BadgeNumber, update.HomeStationCode, cancellationToken);
        if (profileError is not null)
        {
            return profileError;
        }

        user.DisplayName = update.DisplayName.Trim();
        user.EmployeeNumber = Normalize(update.EmployeeNumber);
        user.BadgeNumber = Normalize(update.BadgeNumber);
        user.HomeStationCode = Normalize(update.HomeStationCode)?.ToUpperInvariant();

        var result = await InTransactionAsync<bool>(
            async () =>
            {
                var saved = await userManager.UpdateAsync(user);
                if (!saved.Succeeded)
                {
                    return ToError(saved, "displayName");
                }

                var current = await userManager.GetRolesAsync(user);
                var wanted = update.Roles.Distinct(StringComparer.Ordinal).ToList();
                var removed = await userManager.RemoveFromRolesAsync(user, current.Except(wanted, StringComparer.Ordinal));
                if (!removed.Succeeded)
                {
                    return ToError(removed, "roles");
                }

                var added = await userManager.AddToRolesAsync(user, wanted.Except(current, StringComparer.Ordinal));
                if (!added.Succeeded)
                {
                    return ToError(added, "roles");
                }

                if (!current.OrderBy(r => r, StringComparer.Ordinal).SequenceEqual(wanted.OrderBy(r => r, StringComparer.Ordinal), StringComparer.Ordinal))
                {
                    await userManager.UpdateSecurityStampAsync(user); // role changes apply at the next revalidation
                }

                return true;
            },
            cancellationToken);
        return result.IsSuccess ? Result.Success() : result.Error!;
    }

    public async Task<Result> SetActiveAsync(string id, bool isActive, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return UserErrors.NotFound(id);
        }

        user.IsActive = isActive;
        var result = isActive ? await userManager.UpdateAsync(user) : await userManager.UpdateSecurityStampAsync(user);
        return result.Succeeded ? Result.Success() : ToError(result, "isActive");
    }

    public async Task<Result> ResetPasswordAsync(string id, string temporaryPassword, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return UserErrors.NotFound(id);
        }

        var result = await InTransactionAsync<bool>(
            async () =>
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                var reset = await userManager.ResetPasswordAsync(user, token, temporaryPassword);
                if (!reset.Succeeded)
                {
                    return ToError(reset, "temporaryPassword");
                }

                user.MustChangePassword = true;
                await userManager.SetLockoutEndDateAsync(user, null);
                await userManager.ResetAccessFailedCountAsync(user);
                var saved = await userManager.UpdateAsync(user);
                return saved.Succeeded ? true : ToError(saved, "temporaryPassword");
            },
            cancellationToken);
        return result.IsSuccess ? Result.Success() : result.Error!;
    }

    private async Task<Error?> CheckProfileAsync(string? userId, string? badge, string? stationCode, CancellationToken cancellationToken)
    {
        var normalizedBadge = Normalize(badge);
        if (normalizedBadge is not null && await db.Users.AnyAsync(u => u.BadgeNumber == normalizedBadge && u.Id != userId, cancellationToken))
        {
            return UserErrors.DuplicateBadge;
        }

        var code = Normalize(stationCode)?.ToUpperInvariant();
        if (code is not null && !await db.Stations.AnyAsync(s => s.Code == code, cancellationToken))
        {
            return UserErrors.UnknownStation(code);
        }

        return null;
    }

    private async Task<Dictionary<string, List<string>>> RoleMapAsync(string? userId, CancellationToken cancellationToken)
    {
        var query = from ur in db.UserRoles
                    join r in db.Roles on ur.RoleId equals r.Id
                    where userId == null || ur.UserId == userId
                    select new { ur.UserId, r.Name };
        var rows = await query.AsNoTracking().ToListAsync(cancellationToken);
        return rows.GroupBy(r => r.UserId).ToDictionary(g => g.Key, g => g.Select(r => r.Name!).OrderBy(n => n, StringComparer.Ordinal).ToList());
    }

    private async Task<Result<T>> InTransactionAsync<T>(Func<Task<Result<T>>> action, CancellationToken cancellationToken)
    {
        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async ct =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(ct);
            var result = await action();
            if (result.IsSuccess)
            {
                await transaction.CommitAsync(ct);
            }

            return result;
        }, cancellationToken);
    }

    private static Error ToError(IdentityResult result, string field)
    {
        if (result.Errors.Any(e => e.Code == nameof(IdentityErrorDescriber.ConcurrencyFailure)))
        {
            return ConcurrencyErrors.Conflict;
        }

        if (result.Errors.Any(e => e.Code is nameof(IdentityErrorDescriber.DuplicateEmail) or nameof(IdentityErrorDescriber.DuplicateUserName)))
        {
            return UserErrors.DuplicateEmail;
        }

        return new ValidationError(new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            [field] = [.. result.Errors.Select(e => e.Description)],
        });
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
