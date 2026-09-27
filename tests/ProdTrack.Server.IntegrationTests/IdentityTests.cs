using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using ProdTrack.Application.Security;
using ProdTrack.Infrastructure.Identity;

namespace ProdTrack.Server.IntegrationTests;

[Collection(ServerCollection.Name)]
[Trait("Category", "Integration")]
[Trait("Story", "PT-009")]
public sealed partial class IdentityTests(ProdTrackFactory factory)
{
    [Fact]
    public async Task Bootstrap_admin_is_created_on_first_run_with_temporary_password()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        var admin = await users.FindByEmailAsync(ProdTrackFactory.BootstrapEmail);

        admin.Should().NotBeNull();
        admin!.MustChangePassword.Should().BeTrue();
        (await users.IsInRoleAsync(admin, Roles.Admin)).Should().BeTrue();
        (await users.CheckPasswordAsync(admin, ProdTrackFactory.BootstrapPassword)).Should().BeTrue();
    }

    [Fact]
    public async Task Five_failed_sign_ins_lock_the_account()
    {
        const string email = "lockout@prodtrack.test";
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var user = new AppUser { UserName = email, Email = email, DisplayName = "Lockout Test" };
            (await users.CreateAsync(user, "Correct!Horse2026")).Succeeded.Should().BeTrue();
            await users.AddToRoleAsync(user, Roles.Operator);
        }

        using var client = factory.CreateAnonymousClient();
        for (var attempt = 1; attempt <= 5; attempt++)
        {
            using var response = await PostLoginAsync(client, email, "Wrong-password-1");
            response.StatusCode.Should().Be(attempt < 5 ? HttpStatusCode.OK : HttpStatusCode.Redirect);
        }

        using var locked = await PostLoginAsync(client, email, "Correct!Horse2026");
        locked.StatusCode.Should().Be(HttpStatusCode.Redirect);
        locked.Headers.Location!.ToString().Should().Contain("account/lockout");
    }

    [Fact]
    public async Task Successful_sign_in_forces_password_change_for_temporary_accounts()
    {
        const string email = "temp@prodtrack.test";
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var user = new AppUser { UserName = email, Email = email, DisplayName = "Temp", MustChangePassword = true };
            (await users.CreateAsync(user, "Temporary!Pass2026")).Succeeded.Should().BeTrue();
            await users.AddToRoleAsync(user, Roles.Planner);
        }

        using var client = factory.CreateAnonymousClient();
        using var login = await PostLoginAsync(client, email, "Temporary!Pass2026");
        login.StatusCode.Should().Be(HttpStatusCode.Redirect);
        login.Headers.Location!.ToString().Should().Contain("account/change-password");

        using var api = await client.GetAsync(new Uri("/api/v1/me", UriKind.Relative));
        api.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await api.Content.ReadAsStringAsync()).Should().Contain("Account.PasswordChangeRequired");

        using var page = await client.GetAsync(new Uri("/work-orders", UriKind.Relative));
        page.StatusCode.Should().Be(HttpStatusCode.Redirect);
        page.Headers.Location!.ToString().Should().Contain("/account/change-password");
    }

    [Fact]
    public async Task Deactivated_users_cannot_sign_in()
    {
        const string email = "inactive@prodtrack.test";
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var user = new AppUser { UserName = email, Email = email, DisplayName = "Inactive", IsActive = false };
            (await users.CreateAsync(user, "Inactive!Pass2026")).Succeeded.Should().BeTrue();
        }

        using var client = factory.CreateAnonymousClient();
        using var response = await PostLoginAsync(client, email, "Inactive!Pass2026");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Contain("deactivated");
    }

    private static async Task<HttpResponseMessage> PostLoginAsync(HttpClient client, string email, string password)
    {
        var html = await client.GetStringAsync(new Uri("/account/login", UriKind.Relative));
        var token = AntiforgeryInput().Match(html).Groups["token"].Value;
        token.Should().NotBeNullOrEmpty();
        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["_handler"] = "login",
            ["__RequestVerificationToken"] = token,
            ["Input.Email"] = email,
            ["Input.Password"] = password,
        });
        return await client.PostAsync(new Uri("/account/login", UriKind.Relative), form);
    }

    [GeneratedRegex("name=\"__RequestVerificationToken\" value=\"(?<token>[^\"]+)\"")]
    private static partial Regex AntiforgeryInput();
}
