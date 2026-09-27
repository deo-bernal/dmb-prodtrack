namespace ProdTrack.Infrastructure.Persistence;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    /// <summary>Apply EF migrations at startup. false in prod (the deploy workflow applies an idempotent script).</summary>
    public bool MigrateOnStartup { get; set; }

    /// <summary>Seed the fictitious demo catalog (local / practice site only).</summary>
    public bool SeedDemoData { get; set; }
}
