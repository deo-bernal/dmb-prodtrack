# ADR-0011: Audit trail in SaveChanges; work order numbers from a sequence table

- Status: Accepted
- Date: 2026-09-27

## Context

PT-047 requires an audit row for every insert/update/delete in the same transaction, including database-generated keys. Work order numbers must be `WO-yyyy-nnnnnn` per plant year.

## Decision

`AppDbContext.SaveChangesAsync` captures changes from the change tracker, saves the business changes, then inserts audit rows with parameterized INSERTs in the same transaction (inside the EF execution strategy, changes accepted after commit). Sensitive Identity fields are excluded. Numbers come from a `NumberSequence` row (name + plant year) incremented in the same unit of work and protected by its row version.

## Consequences

+ Provider-neutral, generated keys captured, one transaction. - Concurrent number allocation surfaces as a 409 conflict that the caller retries (acceptable at plant volume).

## Alternatives considered

SaveChanges interceptor; SQL Server SEQUENCE objects (not portable to the SQLite test setup).
