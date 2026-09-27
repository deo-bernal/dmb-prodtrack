# ADR-0010: SQLite in-memory for application and integration tests

- Status: Accepted
- Date: 2026-09-27

## Context

docs/06 planned Testcontainers SQL Server. The first development PC has no Docker, and tests must pass on a fresh clone and on GitHub with no secrets.

## Decision

Application, Infrastructure and Server integration tests run the real `AppDbContext` on a kept-open SQLite in-memory connection (`tests/ProdTrack.TestSupport`). A test-only `SqliteModelCustomizer` removes T-SQL check constraints, turns `rowversion` into a plain column and stores `DateTimeOffset`/`decimal` in sortable forms. Integration tests use `WebApplicationFactory<Program>` in the `Testing` environment with `Auth:Mode=Test` (header-based users).

## Consequences

+ Fast (whole suite in seconds), no Docker, deterministic. - Does not exercise SQL Server specifics (rowversion concurrency, filtered indexes, JSON check constraints); those are covered by applying the migration to LocalDB and by the planned E2E job against a SQL Server service container (PT-029). Revisit Testcontainers when Docker is available.

## Alternatives considered

Testcontainers.MsSql (planned, needs Docker); LocalDB-only tests (Windows only).
