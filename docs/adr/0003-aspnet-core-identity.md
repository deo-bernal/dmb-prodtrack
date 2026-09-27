# ADR-0003: ASP.NET Core Identity with roles, admin-created accounts

- Status: Accepted
- Date: 2026-09-27

## Context

Plant staff need accounts without email (the free plan has no mail service) and six roles.

## Decision

ASP.NET Core Identity (EF stores, schema `sec`) with cookie auth `__Host-ProdTrack`, roles Admin/Planner/Supervisor/Operator/QC/Viewer, named policies (`Policies.Definitions`), lockout after 5 failures for 15 minutes, 12+ character passwords, no self-registration and no email flows. A bootstrap admin is created on first run from `Auth:BootstrapAdmin` (user-secrets / host config) and must change the password at first sign-in. `Auth:Mode=Dev` (user picker) and `Test` (header auth for tests) are refused outside Development/Testing.

## Consequences

+ Free, self-contained. - Password resets are done by an admin (PT-012). Google sign-in (PT-068) and TOTP remain follow-ups.

## Alternatives considered

Entra ID (Phase 2 option), Google-only sign-in.
