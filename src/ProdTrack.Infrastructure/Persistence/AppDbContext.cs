using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using ProdTrack.Application.Abstractions;
using ProdTrack.Domain.Audit;
using ProdTrack.Domain.Common;
using ProdTrack.Domain.Products;
using ProdTrack.Domain.Quality;
using ProdTrack.Domain.ReasonCodes;
using ProdTrack.Domain.Reference;
using ProdTrack.Domain.Routings;
using ProdTrack.Domain.SalesOrders;
using ProdTrack.Domain.Stations;
using ProdTrack.Domain.WorkOrders;
using ProdTrack.Infrastructure.Identity;
using ProdTrack.Infrastructure.Persistence.Auditing;

namespace ProdTrack.Infrastructure.Persistence;

/// <summary>
/// EF Core unit of work. Every save writes audit entries in the same transaction (PT-047) and dispatches domain
/// events after the commit (docs/02 section 8).
/// </summary>
public sealed class AppDbContext(
    DbContextOptions<AppDbContext> options,
    ICurrentUser currentUser,
    ICorrelationIdAccessor correlation,
    TimeProvider timeProvider,
    DomainEventDispatcher dispatcher)
    : IdentityDbContext<AppUser>(options), IAppDbContext, IDataProtectionKeyContext
{
    private bool _saving;

    public DbSet<Station> Stations => Set<Station>();

    public DbSet<ReasonCode> ReasonCodes => Set<ReasonCode>();

    public DbSet<ColorScheme> ColorSchemes => Set<ColorScheme>();

    public DbSet<SignalWord> SignalWords => Set<SignalWord>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Routing> Routings => Set<Routing>();

    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();

    public DbSet<Operation> Operations => Set<Operation>();

    public DbSet<OperationEvent> OperationEvents => Set<OperationEvent>();

    public DbSet<ScrapRecord> ScrapRecords => Set<ScrapRecord>();

    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();

    public DbSet<QcChecklistTemplate> QcChecklistTemplates => Set<QcChecklistTemplate>();

    public DbSet<QcInspection> QcInspections => Set<QcInspection>();

    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

    public DbSet<NumberSequence> NumberSequences => Set<NumberSequence>();

    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    public override int SaveChanges(bool acceptAllChangesOnSuccess) =>
        SaveChangesAsync(acceptAllChangesOnSuccess).GetAwaiter().GetResult();

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        if (_saving)
        {
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        int result;
        List<IDomainEvent> events;
        _saving = true;
        try
        {
            ChangeTracker.DetectChanges();
            var pending = AuditTrail.Capture(ChangeTracker);
            events = CollectDomainEvents();
            result = pending.Count == 0
                ? await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken)
                : await SaveWithAuditAsync(pending, acceptAllChangesOnSuccess, cancellationToken);
        }
        finally
        {
            _saving = false;
        }

        // Dispatched after the commit; handlers that save again get their own audit entries.
        if (events.Count > 0)
        {
            await dispatcher.DispatchAsync(events, cancellationToken);
        }

        return result;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        ConfigureIdentity(builder);

        foreach (var entityType in builder.Model.GetEntityTypes().Where(t => typeof(AggregateRoot).IsAssignableFrom(t.ClrType)))
        {
            builder.Entity(entityType.ClrType).Ignore(nameof(AggregateRoot.DomainEvents));
            builder.Entity(entityType.ClrType).Property(nameof(AggregateRoot.RowVersion)).IsRowVersion();
        }
    }

    private static void ConfigureIdentity(ModelBuilder builder)
    {
        builder.Entity<AppUser>(b =>
        {
            b.ToTable("AspNetUsers", Schemas.Security);
            b.Property(u => u.DisplayName).HasMaxLength(200);
            b.Property(u => u.EmployeeNumber).HasMaxLength(20);
            b.Property(u => u.BadgeNumber).HasMaxLength(20);
            b.Property(u => u.HomeStationCode).HasMaxLength(20);
            b.HasIndex(u => u.BadgeNumber).IsUnique().HasFilter("[BadgeNumber] IS NOT NULL");
        });
        builder.Entity<IdentityRole>().ToTable("AspNetRoles", Schemas.Security);
        builder.Entity<IdentityUserRole<string>>().ToTable("AspNetUserRoles", Schemas.Security);
        builder.Entity<IdentityUserClaim<string>>().ToTable("AspNetUserClaims", Schemas.Security);
        builder.Entity<IdentityUserLogin<string>>().ToTable("AspNetUserLogins", Schemas.Security);
        builder.Entity<IdentityUserToken<string>>().ToTable("AspNetUserTokens", Schemas.Security);
        builder.Entity<IdentityRoleClaim<string>>().ToTable("AspNetRoleClaims", Schemas.Security);
        builder.Entity<DataProtectionKey>().ToTable("DataProtectionKeys", Schemas.Security);
    }

    private List<IDomainEvent> CollectDomainEvents()
    {
        var aggregates = ChangeTracker.Entries<AggregateRoot>().Select(e => e.Entity).Where(a => a.DomainEvents.Count > 0).ToList();
        var events = aggregates.SelectMany(a => a.DomainEvents).ToList();
        aggregates.ForEach(a => a.ClearDomainEvents());
        return events;
    }

    private async Task<int> SaveWithAuditAsync(List<PendingAudit> pending, bool acceptAllChangesOnSuccess, CancellationToken cancellationToken)
    {
        int result;
        if (Database.CurrentTransaction is not null)
        {
            result = await SaveAttemptAsync(pending, cancellationToken);
        }
        else
        {
            // Documented EF retry pattern: changes are accepted only after the commit, so a retried attempt replays them.
            var strategy = Database.CreateExecutionStrategy();
            result = await strategy.ExecuteAsync(
                pending,
                async (_, state, ct) =>
                {
                    await using var transaction = await Database.BeginTransactionAsync(ct);
                    var count = await SaveAttemptAsync(state, ct);
                    await transaction.CommitAsync(ct);
                    return count;
                },
                verifySucceeded: null,
                cancellationToken);
        }

        if (acceptAllChangesOnSuccess)
        {
            ChangeTracker.AcceptAllChanges();
        }

        return result;
    }

    /// <summary>
    /// One attempt: save business changes (generated keys become known), then insert the audit rows with plain
    /// parameterized INSERTs in the same transaction. Audit rows bypass the change tracker so the business entities
    /// are not saved twice and a retry starts from a clean state.
    /// </summary>
    private async Task<int> SaveAttemptAsync(List<PendingAudit> pending, CancellationToken cancellationToken)
    {
        var count = await base.SaveChangesAsync(acceptAllChangesOnSuccess: false, cancellationToken);
        var now = timeProvider.GetUtcNow();
        var (sql, properties) = AuditInsertCommand();
        foreach (var item in pending)
        {
            var auditEntry = AuditTrail.ToAuditEntry(item, currentUser.UserId, correlation.CorrelationId, now);
            var parameters = properties.Select((p, i) => CreateParameter(i, ToProviderValue(p, p.PropertyInfo!.GetValue(auditEntry)))).ToArray();
            await Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);
        }

        return count;
    }

    private (string Sql, IReadOnlyList<IProperty> Properties) AuditInsertCommand()
    {
        var entityType = Model.FindEntityType(typeof(AuditEntry))!;
        var table = StoreObjectIdentifier.Table(entityType.GetTableName()!, entityType.GetSchema());
        var sqlHelper = this.GetService<ISqlGenerationHelper>();
        var properties = entityType.GetProperties().Where(p => p.ValueGenerated == ValueGenerated.Never && p.PropertyInfo is not null).ToList();
        var columns = string.Join(", ", properties.Select(p => sqlHelper.DelimitIdentifier(p.GetColumnName(table)!)));
        var placeholders = string.Join(", ", properties.Select((_, i) => ParameterName(i)));
        var sql = string.Concat("INSERT INTO ", sqlHelper.DelimitIdentifier(table.Name, table.Schema), " (", columns, ") VALUES (", placeholders, ")");
        return (sql, properties);
    }

    private static string ParameterName(int index) => "@audit" + index.ToString(System.Globalization.CultureInfo.InvariantCulture);

    private System.Data.Common.DbParameter CreateParameter(int index, object value)
    {
        using var command = Database.GetDbConnection().CreateCommand();
        var parameter = command.CreateParameter();
        parameter.ParameterName = ParameterName(index);
        parameter.Value = value;
        return parameter;
    }

    private static object ToProviderValue(IProperty property, object? value)
    {
        var converter = property.GetTypeMapping().Converter;
        var providerValue = value is null || converter is null ? value : converter.ConvertToProvider(value);
        return providerValue ?? DBNull.Value;
    }
}
