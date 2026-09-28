# AGENTS.md - Instructions for AI coding agents

You are working on **DMB ProdTrack**, a practice .NET 10 application (production work orders and shop-floor tracking for a safety-identification-products plant). The documentation in `docs/` is the source of truth. Read this file fully before changing anything.

## 1. Before you write code

1. Identify the story key (e.g. `PT-031`) you are asked to implement. If none is given, ask for one or pick the next unfinished story of the current sprint in `docs/05-backlog.md` and say which one you picked.
2. Read: the story (and its acceptance criteria) in `docs/05-backlog.md`; the relevant sections of `docs/02-technical-design.md` (architecture, ERD, API table, auth, portability); business rules in `docs/01-business-requirements.md` (sections 5-9); `docs/09-coding-standards.md`.
3. Check that the story is Ready (dependencies done). If an earlier story it depends on is missing, stop and report.
4. Write a short plan (files to add/change, tests to write, docs to update) before editing.

## 2. Non-negotiable rules

- **Architecture:** Domain <- Application <- Infrastructure / Server (see `02` section 4). `Domain` and `Application` must not reference EF Core providers, ASP.NET Core, or any cloud SDK. **No cloud SDKs by default** (the MonsterASP host uses local providers). Cloud SDKs (`Azure.*`, `Google.Cloud.*`) only in the optional adapter projects `ProdTrack.Infrastructure.Azure` / `ProdTrack.Infrastructure.Gcp` (Sprint 8) and their registration in the Server composition root.
- **Portability:** file storage via `IFileStorage`, secrets via configuration / `ISecretProvider`, telemetry via `ILogger`/`ActivitySource`/`Meter`. Provider chosen by `Cloud:Provider` (`Local|Azure|Gcp`, default **`Local`** = disk under `App_Data`, config-file secrets, file logs - used on MonsterASP). Never hard-code a host or cloud.
- **Auth:** ASP.NET Core Identity (`Auth:Mode=Identity`) is the default; optional Google external login (`Auth:Google:*`); Entra ID only if a story asks for it (PT-069). Application code depends only on `ICurrentUser` and policy names.
- **Security:** every endpoint has an authorization policy (fallback policy = authenticated); write paths are audited automatically (don't bypass the DbContext); no secrets in code, committed config files, logs or workflow YAML; validate uploads.
- **Time:** use injected `TimeProvider`; store `datetimeoffset` UTC (`...AtUtc`); display in `Plant:TimeZone`.
- **Errors:** return `Result`/typed errors from handlers; endpoints map to ProblemDetails (400/403/404/409/422).
- **Tests first where practical:** each Given/When/Then scenario -> at least one automated test (unit or integration), tagged `[Trait("Story","PT-xxx")]`.
- **Licensing:** don't add FluentAssertions v8+, MediatR v13+, AutoMapper or other commercially licensed packages. Use AwesomeAssertions, own handler interfaces, Mapperly. Add packages only via `Directory.Packages.props`.
- **No scope creep:** implement exactly the story. Put ideas in your summary as suggested follow-up stories.
- **Do not**, unless explicitly asked: create or push git tags, force-push, push to `main`, run the `deploy` or `ops` workflow, run mutating `gh` commands (issues, labels, milestones, releases, repo settings, rulesets, secrets, variables), change anything in the MonsterASP control panel, or create/modify cloud resources (Azure, Google Cloud, Azure DevOps). Read-only `gh` commands (view/list) are fine.
- **Never** reference or modify the GCP project `find-an-agent` or its OAuth clients, and never create, change or delete **any** DNS record of `dmbwebsolutions.com` (the custom domain is deferred). The app lives at `https://dmb-prodtrack.runasp.net`.
- **Free-host limits (MonsterASP free plan):** 256 MB RAM app pool, 1 GB database, sleeps after 30 min idle, no email/SMTP, no scheduled tasks, no backups. So: no email features (admin resets passwords), background work must be idempotent and survive restarts, no keep-alive pings, stream uploads, keep files/logs out of SQL, no `MigrateOnStartup` in prod (the deploy workflow applies an idempotent migration script).
- **Cost:** everything stays US$0. Keep GitHub Actions usage within the 2,000 free minutes/month (Windows jobs may count more; avoid needless matrix builds or long-running jobs). Don't add paid services or packages without noting the cost in `docs/03` and asking.

## 3. Commands

```bash
dotnet restore
dotnet build ProdTrack.slnx -warnaserror
dotnet test ProdTrack.slnx --filter "Category!=E2E"        # SQLite in-memory, no Docker needed (ADR-0010)
dotnet test tests/ProdTrack.E2E.Tests                       # Playwright/Chromium E2E (first run downloads Chromium user-locally)
dotnet ef database drop -f -p src/ProdTrack.Infrastructure -s src/ProdTrack.Server   # reset local DB (Development re-migrates + seeds on next run)
dotnet format ProdTrack.slnx --verify-no-changes
dotnet tool restore                                         # dotnet-ef (local tool manifest)
dotnet ef migrations add <Name> -p src/ProdTrack.Infrastructure -s src/ProdTrack.Server -o Persistence/Migrations
docker compose up -d sql                                    # local SQL Server (or use LocalDB)
dotnet run --project src/ProdTrack.Server                   # Auth:Mode=Dev via user-secrets for local work
```

Run build + tests before declaring a story done. Tests do not need Docker or SQL Server.

## 4. Definition of Done (checklist to report at the end)

- [ ] Acceptance criteria implemented and covered by tests (list test names)
- [ ] `dotnet build` with no warnings; tests pass; formatting clean
- [ ] Authorization + audit on new write paths; ProblemDetails errors
- [ ] EF migration added and reviewed (if schema changed)
- [ ] Docs updated: API table in `docs/02` section 6, user guide `docs/07` (user-visible behaviour), status table in `docs/05` section 6 (and close the GitHub issue via the PR), ADR in `docs/adr/` for significant decisions, `docs/08` for ops changes
- [ ] Summary with: what changed, how to test manually, follow-ups/risks

## 5. Where things go

| Thing | Location |
|---|---|
| Entity, value object, domain event, domain errors | `src/ProdTrack.Domain/<Aggregate>/` |
| Use case (command/query + handler + validator) | `src/ProdTrack.Application/<Feature>/<UseCase>/` |
| Port interfaces | `src/ProdTrack.Application/Abstractions/` |
| EF configuration, migrations, interceptors | `src/ProdTrack.Infrastructure/Persistence/` |
| Optional Azure / GCP adapters (Sprint 8) | `src/ProdTrack.Infrastructure.Azure/`, `src/ProdTrack.Infrastructure.Gcp/` |
| DTOs, route constants, hub message names | `src/ProdTrack.Contracts/` |
| Endpoints | `src/ProdTrack.Server/Endpoints/<Feature>Endpoints.cs` |
| Back-office Blazor pages | `src/ProdTrack.Server/Components/Pages/<Feature>/` |
| Shared components | `src/ProdTrack.UI.Shared/` |
| Tablet PWA pages | `src/ProdTrack.ShopFloor/Pages/` |
| Tests | mirror the source structure under `tests/` |
| CI/CD workflows | `.github/workflows/` (`ci.yml`, `deploy.yml`, `ops.yml`); Azure Pipelines YAML only in Phase 2 (`docs/04`) |
| Deployment files | `deploy/` (`app_offline.htm`, `appsettings.Production.template.json`) |
| Optional IaC for alternatives | `infra/azure/` (Bicep, Sprint 8); no Terraform by default |
| Helper scripts (e.g. `create-github-issues.sh`, generated) | `scripts/` |

## 6. Communication style

- Be explicit about assumptions; if the docs are ambiguous, choose the simplest option consistent with them, note it, and propose a doc update.
- Keep PR-sized changes; one story per branch `feature/PT-xxx-short-name`; commit messages `PT-xxx: <imperative summary>`; PR title `PT-xxx: ...` with `Closes #<issue>` (each story is a GitHub issue whose title starts with its PT key).
- Never present assumptions as facts about a real company; this is a DMB Websolutions practice app with a fictitious plant scenario and data.
- Workflow changes: keep `docs/11-github-setup.md` (and `docs/08` for ops) in sync. Azure DevOps interview practice: when a story touches CI/CD or planning, mention the related Phase 2 lab in `docs/10-azure-devops-study-guide.md`.
