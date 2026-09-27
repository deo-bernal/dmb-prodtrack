# 09 - Coding Standards

> **Status:** Draft v0.3 (2026-09-26) - practice project, written as if for a team (mentoring material).
> These standards are enforced by `.editorconfig`, analyzers, architecture tests and PR review. AI agents follow them via `AGENTS.md` and `.cursor/rules/`.

---

## 1. General

- Target **.NET 10**, C# 14 (`<LangVersion>latest</LangVersion>` not needed; SDK default). `Nullable` **enabled**, `ImplicitUsings` enabled, `TreatWarningsAsErrors` true in `src/`.
- `Directory.Build.props`: common properties, analyzers (`Microsoft.CodeAnalysis.NetAnalyzers` with `AnalysisLevel=latest-recommended`), deterministic builds. `Directory.Packages.props`: central package versions; lock files (`RestorePackagesWithLockFile`).
- Keep methods short (guideline <= 30 lines), classes focused (single responsibility). Prefer clarity over cleverness.
- No commented-out code; no `TODO` without a work item key (`// TODO PT-061: offline queue`).
- English for all code, comments, commits and docs.

## 2. Naming

| Element | Convention | Example |
|---|---|---|
| Namespaces | `ProdTrack.<Layer>.<Feature>` matching folders | `ProdTrack.Application.WorkOrders.Release` |
| Classes, records, enums, methods, properties | PascalCase | `ReleaseWorkOrderHandler`, `WorkOrderStatus.OnHold` |
| Interfaces | `I` + PascalCase | `IFileStorage` |
| Private fields | `_camelCase` | `_timeProvider` |
| Locals, parameters | camelCase | `workOrderId` |
| Constants | PascalCase | `MaxUploadBytes` |
| Async methods | suffix `Async`, accept `CancellationToken ct` last | `SaveAsync(..., CancellationToken ct)` |
| Commands / queries | verb-noun + `Command`/`Query` | `StartOperationCommand`, `GetStationQueueQuery` |
| DTOs (Contracts) | `...Request`, `...Response`, `...Dto` | `CompleteOperationRequest` |
| Tests | `Scenario_Condition_Expected` | `Complete_WhenQtyExceedsInput_ReturnsValidationError` |
| Database | tables singular PascalCase in schemas (`ord.WorkOrder`), columns PascalCase, UTC suffix `AtUtc` | `ReleasedAtUtc` |
| Routes | kebab-case plural nouns, verbs only for actions | `/api/v1/work-orders/{id}/release` |
| Branches | `feature/PT-031-short-name` | |

## 3. C# style

- File-scoped namespaces; one public type per file; file name = type name.
- `var` when the type is obvious from the right-hand side; explicit type otherwise.
- Prefer `record` / `record struct` for DTOs, commands, value objects; `sealed` classes by default.
- Primary constructors for DI in services/handlers; keep domain entities with explicit constructors and private setters.
- Expression-bodied members for one-liners only.
- Pattern matching and `switch` expressions for state logic; exhaustive switches over enums with a default that throws `UnreachableException`.
- Collection expressions (`[]`) and `IReadOnlyList<T>` for exposed collections; never expose mutable `List<T>` from entities.
- Guard clauses with `ArgumentNullException.ThrowIfNull`, `ArgumentOutOfRangeException.ThrowIfNegativeOrZero`.
- No `DateTime.Now/UtcNow`: inject `TimeProvider`. No `Guid.NewGuid()` in domain logic where determinism matters: inject an ID generator or pass IDs in.
- Use `CancellationToken` all the way down; never `.Result` / `.Wait()`; no `async void` except event handlers.
- String comparisons: `StringComparison.Ordinal`/`OrdinalIgnoreCase` explicitly.
- Logging: structured templates, not interpolation: `logger.LogInformation("Operation {OperationId} started by {UserId}", id, userId)`. Consider `LoggerMessage` source-generated methods for hot paths.

## 4. Architecture rules (enforced)

1. `Domain` has no project or package references except BCL.
2. `Application` references `Domain` only (+ abstractions, FluentValidation). No EF Core provider, no ASP.NET Core, no cloud SDKs.
3. `Infrastructure` implements Application ports; no cloud SDKs by default (MonsterASP uses local providers); cloud SDKs only in the optional adapter projects `Infrastructure.Azure` / `Infrastructure.Gcp` (Sprint 8, PT-057/058).
4. `Server` is the composition root; endpoints are thin: bind -> map to command -> call handler -> map `Result` to HTTP.
5. Blazor Server components call Application handlers through a small `IUseCaseDispatcher`; they never use `DbContext` directly.
6. `ShopFloor` talks only to the REST API through `ProdTrack.ApiClient`.
7. Business rules live in domain entities (e.g. `workOrder.Release(routing, now)`), not in endpoints or UI.

## 5. Domain modelling

- Entities protect invariants: private setters, methods named after business actions (`Start`, `Pause`, `Complete`, `Hold`), which return `Result` or raise domain events.
- Value objects for concepts with rules: `WorkOrderNumber`, `Quantity`, `StationCode`, `ScanCode`, `ColorScheme`.
- Domain events are past-tense records: `OperationStarted`, `WorkOrderReleased`, `ScrapLogged`.
- Errors as static typed definitions: `WorkOrderErrors.ArtworkNotApproved` with stable codes (`WorkOrder.ArtworkNotApproved`) used by API clients and tests.

## 6. API standards

- Versioned under `/api/v1`; breaking changes -> `/api/v2`.
- Use `TypedResults` and declare all responses (`Produces<T>`, `ProducesProblem`) so OpenAPI is accurate; add summaries/descriptions.
- Validation errors 400, business rule 422, not found 404, forbidden 403, concurrency 409 - all as ProblemDetails with `code` extension.
- Pagination contract `{ items, page, pageSize, totalCount }`; max page size 100.
- Every write endpoint: authorization policy, validation, audit (automatic via interceptor), idempotency `requestId` for shop-floor actions.
- Never return EF entities; map to Contracts DTOs.

## 7. Data access

- EF Core configuration via `IEntityTypeConfiguration<T>` classes, one per entity, in `Infrastructure/Persistence/Configurations`.
- Queries: `AsNoTracking()` + `Select` projection; avoid `Include` chains for reads; paginate.
- No lazy loading. No raw SQL string concatenation (`FromSql($"...")` interpolation only).
- Migrations: meaningful names (`AddScrapReasonCategory`), reviewed; never edit an applied migration.
- Transactions: one `SaveChangesAsync` per command (unit of work); explicit transactions only when needed.

## 8. Blazor UI

- Components small and focused; page components in `Components/Pages/<Feature>`; shared in `ProdTrack.UI.Shared`.
- Parameters immutable from the child's perspective; use `EventCallback` for outputs.
- Every interactive element gets `data-testid`.
- Accessibility: labels for inputs, keyboard navigable, colour + icon + text for status, `aria-live` for scan feedback.
- Shop-floor UI: min touch target 48 px, base font 18 px, high-contrast theme, one primary action per screen.
- No business logic in components; call handlers/API and render results.
- Dispose subscriptions (`IAsyncDisposable`) for real-time feeds.

## 9. Security coding rules

- Deny by default: fallback authorization policy requires authenticated user; `[AllowAnonymous]` only on health and sign-in endpoints.
- Validate and encode: never render user input as raw HTML (`MarkupString`) ; validate file uploads (type sniffing, size).
- Secrets only via configuration providers; never log them; never commit `appsettings.*.json` with secrets (use user-secrets).
- Use the `ICurrentUser` abstraction; never trust user IDs from request bodies.
- Security-relevant events (sign-in failures and lockouts, forbidden attempts, user/role changes, reprints) are logged.

## 10. Testing rules

See `06-testing-strategy.md`. Minimum per story: unit tests for new domain/application logic and an integration test per new endpoint (happy path + main failure + authorization).

## 11. Git and PR workflow

- One story per branch; small PRs (< ~400 changed lines excluding generated migrations where possible).
- Commit messages: `PT-031: Start operation by scan` (imperative, <= 72 chars subject), body explains why. Reference the GitHub issue (`#42`) in the body or PR.
- PR title `PT-031: <summary>`; description follows `.github/pull_request_template.md`; link the issue with `Closes #42` so it closes on merge and moves on the Projects board; include screenshots for UI changes.
- `main` is changed only through PRs with green `build-test` and `e2e` checks (on the private GitHub Free repo this is a team convention plus the local `.githooks/pre-push` guard, because branch protection is not available - `11` section 7); never push directly to `main` or force-push shared branches.
- Review checklist: correctness vs acceptance criteria, tests, naming, layering, security (authz, input validation), performance (N+1, paging), logging, docs updated.
- Squash merge; delete branch after merge.

## 12. Documentation rules

- Update docs in the same PR as the behaviour change: API table (`02` section 6), user guide (`07`), backlog status (`05` section 6), ADRs for significant decisions.
- XML doc comments on public APIs of `Contracts`, `ApiClient` and Application ports.
- Mermaid for diagrams; keep them in markdown next to the text.

## 13. Mentoring notes (how to review kindly and effectively)

- Comment on code, not people; explain the *why* and link to this document.
- Prefix comments: `nit:` (optional), `suggestion:`, `question:`, `blocking:`.
- Pair on the first story of each new area (e.g. first SignalR feature) and record learnings in the Wiki.

## 14. Change log

| Date | Version | Change |
|---|---|---|
| 2026-09-26 | 0.1 | Initial coding standards |
| 2026-09-26 | 0.2 | Identity-related logging, GCP-first wording |
| 2026-09-26 | 0.3 | No cloud SDKs by default (optional adapters only); GitHub PR/issue conventions (`Closes #n`, PR template, required checks by convention, pre-push hook) |
