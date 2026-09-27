using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ProdTrack.TestSupport;

/// <summary>
/// Adapts the SQL Server model to SQLite for fast tests: drops T-SQL check constraints, turns rowversion into a plain
/// nullable column, and stores DateTimeOffset/decimal in sortable/aggregatable forms. Production always uses SQL Server.
/// </summary>
public sealed class SqliteModelCustomizer(ModelCustomizerDependencies dependencies) : RelationalModelCustomizer(dependencies)
{
    public override void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        base.Customize(modelBuilder, context);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes().ToList())
        {
            foreach (var check in entityType.GetCheckConstraints().ToList())
            {
                entityType.RemoveCheckConstraint(check.ModelName);
            }

            foreach (var property in entityType.GetProperties())
            {
                if (property.Name == "RowVersion" && property.ClrType == typeof(byte[]))
                {
                    property.IsConcurrencyToken = false;
                    property.ValueGenerated = ValueGenerated.Never;
                    property.IsNullable = true;
                }
                else if (property.ClrType == typeof(DateTimeOffset) || property.ClrType == typeof(DateTimeOffset?))
                {
                    property.SetValueConverter(new DateTimeOffsetToBinaryConverter());
                }
                else if (property.ClrType == typeof(decimal) || property.ClrType == typeof(decimal?))
                {
                    property.SetValueConverter(new CastingConverter<decimal, double>());
                }
            }
        }
    }
}
