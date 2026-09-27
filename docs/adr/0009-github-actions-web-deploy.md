# ADR-0009: GitHub Actions: build on ubuntu-latest, manual Web Deploy from windows-latest

- Status: Accepted
- Date: 2026-09-27

## Context

CI/CD must be free and safe on a GitHub Free repository without enforced branch protection.

## Decision

`ci.yml` (restore, format check, build with warnings as errors, unit + integration tests, coverage, vulnerable-package check; on `main` also publishes the `prodtrack-drop` artifact with the idempotent migration script). `deploy.yml` is started manually (deploy gate) and uses Web Deploy from `windows-latest` (FTP alternative). `ops.yml` runs weekly checks once `APP_URL` is configured. CI needs no secrets.

## Consequences

+ Free minutes; clear gate. - Deploy is manual by design until protections are available.

## Alternatives considered

Azure Pipelines on a self-hosted agent (Phase 2, PT-073).
