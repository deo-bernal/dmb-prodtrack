using System.Reflection;
using Serilog;
using Serilog.Formatting.Compact;

namespace ProdTrack.Server.Logging;

/// <summary>
/// Serilog: compact JSON rolling files in App_Data/logs (daily, 10 MB, 14 files) + console locally (PT-004).
/// Telemetry:Exporter None (default) | Console | Otlp. OpenTelemetry export is not bundled yet (off by default to save memory).
/// </summary>
internal static class LoggingSetup
{
    public static WebApplicationBuilder AddProdTrackLogging(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;
        var environment = builder.Environment;
        var exporter = configuration["Telemetry:Exporter"] ?? "None";
        var fileEnabled = configuration.GetValue("LogFile:Enabled", true);
        var filePath = configuration["LogFile:Path"] ?? "App_Data/logs/prodtrack-.json";
        if (!Path.IsPathRooted(filePath))
        {
            filePath = Path.Combine(environment.ContentRootPath, filePath);
        }

        var version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "0.0.0";

        builder.Services.AddSerilog((services, logger) =>
        {
            logger.ReadFrom.Configuration(configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "ProdTrack")
                .Enrich.WithProperty("Environment", environment.EnvironmentName)
                .Enrich.WithProperty("Version", version);

            if (fileEnabled)
            {
                logger.WriteTo.File(
                    new CompactJsonFormatter(),
                    filePath,
                    rollingInterval: RollingInterval.Day,
                    fileSizeLimitBytes: 10 * 1024 * 1024,
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: 14,
                    buffered: true,
                    flushToDiskInterval: TimeSpan.FromSeconds(5));
            }

            if (environment.IsDevelopment() || string.Equals(exporter, "Console", StringComparison.OrdinalIgnoreCase))
            {
                logger.WriteTo.Console();
            }
        });

        return builder;
    }
}
