using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ProdTrack.Application.Abstractions;
using ProdTrack.Infrastructure.Persistence;
using ProdTrack.Server.Components;
using ProdTrack.Server.Errors;
using ProdTrack.Server.Logging;
using ProdTrack.Server.Realtime;
using ProdTrack.Server.Security;
using Serilog;

namespace ProdTrack.Server.Hosting;

internal static class WebSetup
{
    public static IServiceCollection AddProdTrackWeb(this IServiceCollection services)
    {
        services.AddRazorComponents()
            .AddInteractiveServerComponents(o =>
            {
                // Free host: keep disconnected circuits few and short-lived (docs/02 section 8, 256 MB).
                o.DisconnectedCircuitMaxRetained = 20;
                o.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(2);
            });
        services.AddScoped<CircuitHandler, UserCircuitHandler>();
        services.AddScoped<UseCaseDispatcher>();

        services.AddSignalR();
        services.AddSingleton<IRealtimeFeed, RealtimeFeed>();
        services.AddScoped<INotifier, SignalRNotifier>();

        services.AddOpenApi();
        services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        services.AddHealthChecks().AddDbContextCheck<AppDbContext>("database", tags: ["ready"]);
        services.AddHttpContextAccessor();
        services.AddScoped<ICorrelationIdAccessor, CorrelationIdAccessor>();
        return services;
    }

    public static async Task InitializeDatabaseAsync(this WebApplication app, AuthMode authMode)
    {
        Directory.CreateDirectory(Path.Combine(app.Environment.ContentRootPath, "App_Data", "files"));
        await using var scope = app.Services.CreateAsyncScope();
        var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
        await initializer.InitializeAsync(seedDevUsers: authMode == AuthMode.Dev, app.Lifetime.ApplicationStopping);
    }

    public static WebApplication UseProdTrackPipeline(this WebApplication app)
    {
        if (app.Configuration.GetValue<bool>("App:UseForwardedHeaders"))
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto });
        }

        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseSerilogRequestLogging();
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        app.UseWhen(
            context => !ApiPaths.IsApi(context.Request.Path),
            branch => branch.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true));
        if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing"))
        {
            app.UseHsts();
            app.UseHttpsRedirection();
        }

        app.UseMiddleware<SecurityHeadersMiddleware>();
        // The /floor WebAssembly files (framework, compressed variants) are served by MapStaticAssets; the legacy
        // UseBlazorFrameworkFiles middleware conflicts with endpoint routing for pre-compressed assets.
        app.UseStaticFiles();
        app.UseAuthentication();
        app.UseMiddleware<UserLogContextMiddleware>();
        app.UseAuthorization();
        app.UseMiddleware<MustChangePasswordMiddleware>();
        app.UseAntiforgery();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi().AllowAnonymous();
            app.UseSwaggerUI(o =>
            {
                o.SwaggerEndpoint("/openapi/v1.json", "ProdTrack API v1");
                o.RoutePrefix = "swagger";
            });
        }

        return app;
    }

    public static WebApplication MapProdTrackHealthChecks(this WebApplication app)
    {
        // Liveness never touches the database; readiness checks it (PT-003).
        app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false }).AllowAnonymous();
        app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = c => c.Tags.Contains("ready") }).AllowAnonymous();
        app.MapHealthChecks("/health", new HealthCheckOptions { Predicate = c => c.Tags.Contains("ready") }).AllowAnonymous();
        return app;
    }

    public static WebApplication MapProdTrackUi(this WebApplication app)
    {
        app.MapStaticAssets().AllowAnonymous();
        app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
        app.MapFallbackToFile("floor/{*path:nonfile}", "floor/index.html").AllowAnonymous();
        return app;
    }
}
