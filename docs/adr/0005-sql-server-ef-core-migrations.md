# ADR-0005: SQL Server on all targets; EF Core code-first migrations as idempotent scripts

- Status: Accepted
- Date: 2026-09-27

## Context

The host provides MSSQL; deployments must be repeatable and safe to re-run.

## Decision

EF Core 10 code-first with schemas md/ord/mes/qc/audit/sec; migrations live in `ProdTrack.Infrastructure/Persistence/Migrations`. CI generates `migrations.sql --idempotent`; the deploy workflow applies it. Locally `dotnet ef database update` (or `Database:MigrateOnStartup=true`). Seeding (reference data, roles, bootstrap admin, optional demo data) runs idempotently at startup, not via migrations.

## Consequences

+ Same engine locally (LocalDB/Docker) and in prod. - Tests use SQLite in-memory (ADR-0010), so SQL Server-specific behaviour is covered by running the migration locally/CI publish.

## Alternatives considered

Database-first; `EnsureCreated` in prod.
