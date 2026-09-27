# ADR-0008: Custom domain deferred

- Status: Accepted
- Date: 2026-09-27

## Context

A custom domain needs a paid plan or DNS changes that are out of scope for the MVP.

## Decision

Use the free `*.runasp.net` subdomain with the manually renewed Let's Encrypt certificate. The public URL is configuration only (`App:PublicBaseUrl`).

## Consequences

+ Free, no DNS work. - 90-day manual renewal (ops runbook, PT-067).

## Alternatives considered

MonsterASP Premium; Azure for Students.
