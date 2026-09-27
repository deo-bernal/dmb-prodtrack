# ADR-0004: Hosting on the free MonsterASP.NET plan

- Status: Accepted
- Date: 2026-09-27

## Context

The project must cost US$0.

## Decision

One IIS site on `*.runasp.net` (framework-dependent `win-x86`, InProcess), one 1 GB MSSQL database; environments Local and Prod. Portability is limited to `IFileStorage`, `ISecretProvider` and telemetry, with Local implementations by default. Details in docs/03 and docs/08.

## Consequences

+ Free. - 256 MB RAM, sleep after 30 minutes, no backups, manual HTTPS renewal (handled by the `ops` workflow and runbooks).

## Alternatives considered

Azure App Service / Google Cloud Run (documented optional alternatives, Sprint 8).
