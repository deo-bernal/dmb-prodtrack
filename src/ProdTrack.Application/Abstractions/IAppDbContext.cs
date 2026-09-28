using Microsoft.EntityFrameworkCore;
using ProdTrack.Domain.Audit;
using ProdTrack.Domain.Products;
using ProdTrack.Domain.Quality;
using ProdTrack.Domain.ReasonCodes;
using ProdTrack.Domain.Reference;
using ProdTrack.Domain.Routings;
using ProdTrack.Domain.SalesOrders;
using ProdTrack.Domain.Stations;
using ProdTrack.Domain.WorkOrders;

namespace ProdTrack.Application.Abstractions;

/// <summary>Unit of work over the ProdTrack database. Implemented by the Infrastructure DbContext.</summary>
public interface IAppDbContext
{
    DbSet<Station> Stations { get; }

    DbSet<ReasonCode> ReasonCodes { get; }

    DbSet<ColorScheme> ColorSchemes { get; }

    DbSet<SignalWord> SignalWords { get; }

    DbSet<Product> Products { get; }

    DbSet<Routing> Routings { get; }

    DbSet<WorkOrder> WorkOrders { get; }

    DbSet<Operation> Operations { get; }

    DbSet<OperationEvent> OperationEvents { get; }

    DbSet<ScrapRecord> ScrapRecords { get; }

    DbSet<SalesOrder> SalesOrders { get; }

    DbSet<QcChecklistTemplate> QcChecklistTemplates { get; }

    DbSet<QcInspection> QcInspections { get; }

    DbSet<AuditEntry> AuditEntries { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
