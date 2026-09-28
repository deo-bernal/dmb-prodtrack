using Microsoft.EntityFrameworkCore;
using ProdTrack.Domain.Quality;
using ProdTrack.Domain.ReasonCodes;
using ProdTrack.Domain.Reference;
using ProdTrack.Domain.Routings;
using ProdTrack.Domain.Stations;

namespace ProdTrack.Infrastructure.Persistence.Seeding;

/// <summary>Idempotent reference data seeding: each list is only seeded when its table is empty.</summary>
internal static class ReferenceDataSeeder
{
    public static async Task SeedAsync(AppDbContext db, TimeProvider timeProvider, CancellationToken cancellationToken)
    {
        if (!await db.Stations.AnyAsync(cancellationToken))
        {
            foreach (var (code, name, type) in ReferenceData.Stations)
            {
                db.Stations.Add(Station.Create(code, name, type, workCenter: null).Value);
            }

            await db.SaveChangesAsync(cancellationToken);
        }

        if (!await db.ColorSchemes.AnyAsync(cancellationToken))
        {
            db.ColorSchemes.AddRange(ReferenceData.ColorSchemes.Select(c => new ColorScheme(c.Code, c.Name, c.Text, c.Background)));
            await db.SaveChangesAsync(cancellationToken);
        }

        if (!await db.SignalWords.AnyAsync(cancellationToken))
        {
            db.SignalWords.AddRange(ReferenceData.SignalWords.Select(s => new SignalWord(s.Word, s.Header, s.Text)));
            await db.SaveChangesAsync(cancellationToken);
        }

        if (!await db.ReasonCodes.AnyAsync(cancellationToken))
        {
            db.ReasonCodes.AddRange(ReferenceData.ReasonCodes.Select(r => ReasonCode.Create(r.Code, r.Description, r.Category).Value));
            await db.SaveChangesAsync(cancellationToken);
        }

        if (!await db.Routings.AnyAsync(cancellationToken))
        {
            var stations = await db.Stations.ToDictionaryAsync(s => s.Code, cancellationToken);
            foreach (var (type, name, codes) in ReferenceData.Routings)
            {
                var steps = codes
                    .Select((code, index) => new RoutingStepDefinition((index + 1) * 10, stations[code], SetupMinutes: 10, StdMinutesPerUnit: 0.5m, AllowOverlap: false))
                    .ToList();
                db.Routings.Add(Routing.Create(type, 1, name, steps, timeProvider.GetUtcNow()).Value);
            }

            await db.SaveChangesAsync(cancellationToken);
        }

        if (!await db.QcChecklistTemplates.AnyAsync(cancellationToken))
        {
            foreach (var (type, name, items) in QcTemplateData.Templates)
            {
                db.QcChecklistTemplates.Add(QcChecklistTemplate.Create(type, name, items).Value);
            }

            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
