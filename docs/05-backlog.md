# 05 - Product Backlog

> **Status:** Draft v0.3 (2026-09-26) - practice project. Hosted on the free MonsterASP.NET plan (US$0); GitHub (repo, Projects, Actions) is the delivery tool; Azure DevOps is a Phase 2 migration (PT-073); Azure/GCP hosting are optional alternatives (Sprint 8). Source of truth for scope and sprint assignment.
> **Import files:** GitHub (primary): [`../repo-scaffold/scripts/create-github-issues.sh`](../repo-scaffold/scripts/create-github-issues.sh) (gh CLI) and [`backlog-github-issues.csv`](backlog-github-issues.csv). Azure Boards (Phase 2): [`backlog.csv`](backlog.csv) (Scrum process) and [`backlog-agile-process.csv`](backlog-agile-process.csv) (Agile process). Both are generated from the same content as this document; if you edit one, update the others (see `.cursor/rules/50-docs-maintenance.mdc`).

## 1. How this backlog is organised

- **Hierarchy:** Epic (`E01`...) > Feature (`F01.1`...) > Product Backlog Item / User Story (`PT-001`...). Tasks are created during Sprint Planning as task lists/sub-issues in GitHub, not here.
- **Keys:** every story has a stable key `PT-nnn` at the start of its title. Use it in branch names (`feature/PT-031-start-operation`), commit messages (`PT-031: ...`) and PR titles, and reference the GitHub issue with `#<number>` (e.g. `Closes #31` in the PR description).
- **Estimates:** story points on a modified Fibonacci scale (1, 2, 3, 5, 8, 13). 13 means "split before scheduling".
- **Sprints:** 2 weeks each. Sprint 0 to Sprint 7 form the MVP (see `docs/00-project-plan.md` section 5). Sprint 8 is an optional sprint for the alternative hosting targets (Azure/GCP) and extra Azure DevOps practice (Artifacts). Items without a sprint are Phase 2+ candidates.
- **Assumed velocity:** about 22-27 points per sprint for one developer working part time (about 15-20 hours per week). Re-plan after Sprint 1 using real velocity.
- **Definition of Ready (DoR):** story has a clear user, value, Given/When/Then acceptance criteria, estimate, no unresolved blocking questions, UI sketch if UI-heavy.
- **Definition of Done (DoD):**
  1. Code merged to `main` through a PR with passing CI (build, unit + integration tests, coverage report).
  2. Acceptance criteria automated as tests where reasonable (unit/integration; E2E for main flows).
  3. No new analyzer warnings; formatting passes (`dotnet format --verify-no-changes`).
  4. Runs locally (Local environment) and, once merged, deployed to **prod** on MonsterASP (`<name>.runasp.net`) by the deploy workflow and smoke-checked. There is no hosted dev/staging environment.
  5. Docs updated: API list in `02`, user guide `07` if behaviour visible to users, this backlog status.
  6. Audit logging and authorization applied to any new write endpoint.

## 2. Importing into GitHub (primary) or Azure Boards (Phase 2)

**GitHub:** create the repo and a Projects board (`docs/11-github-setup.md` sections 1-4), then run `DRY_RUN=1 ./scripts/create-github-issues.sh` from the repo root (copied from `repo-scaffold/`), check the output, and run it again with `DRY_RUN=0` (and `PROJECT_NUMBER=<n>` to add issues to the board with a *Points* field). Sprints become **milestones** (Sprint 0-8, Phase 2), epics and features become labels (`epic:E04`, `feature:F04.1`), MVP items get the `mvp` label. `backlog-github-issues.csv` holds the same data for other importers.

**Azure Boards (Phase 2, PT-073):**

1. Create the iteration paths first (`DMB-ProdTrack\Sprint 0` ... `Sprint 8`), see `docs/04-azure-devops-setup.md` section 3. Import fails for items whose Iteration Path does not exist.
2. **Boards > Queries > Import work items** (or **Boards > Work items > Import**), choose the CSV, review the preview, then **Save items**.
3. Which file?
   - Project created with the **Scrum** process (recommended in this plan): use `backlog.csv`. Scrum names the backlog item **Product Backlog Item** and the estimate field **Effort** (there is no *Story Points* field in Scrum).
   - Project created with the **Agile** process: use `backlog-agile-process.csv` (Work Item Type **User Story**, field **Story Points**).
4. Parent/child links are expressed the way Azure DevOps expects for CSV import: separate **Title 1 / Title 2 / Title 3** columns (Epic / Feature / Story), each child row directly under its parent, and an empty **ID** column for new items. Do not sort the file or insert blank rows.
5. Description and Acceptance Criteria are HTML fields, so the CSV uses `<p>` and `<br/>` for line breaks.
6. Sprint membership is also added as a tag (`sprint-3`) and MVP items are tagged `mvp`, which makes queries easy.
7. After import, open the **Epics** backlog level and check the tree. If links are missing, see the troubleshooting note in `docs/04-azure-devops-setup.md` section 11.

Status lives in the GitHub Projects board (or Azure Boards in Phase 2) once imported.

## 3. Sprint summary

| Sprint | Goal | Stories | Points |
|---|---|---|---|
| Sprint 0 | Foundation: GitHub repo + Projects + issues, CI workflow, repo guardrails, solution skeleton, logging, local environment, MonsterASP account/site/DB | PT-001, PT-002, PT-003, PT-004, PT-005, PT-006, PT-055, PT-059 | 26 |
| Sprint 1 | Identity sign-in, roles, core master data, audit capture, deploy workflow to MonsterASP (Web Deploy) | PT-007, PT-009, PT-010, PT-013, PT-015, PT-016, PT-017, PT-047 | 26 |
| Sprint 2 | Routings, sales orders, work orders, release, artwork proofs, ops runbook (HTTPS renewal, DB backup export) | PT-067, PT-014, PT-018, PT-019, PT-020, PT-023, PT-024 | 26 |
| Sprint 3 | Job traveler with QR, shop-floor PWA shell, scanning, user administration, planner list, E2E setup | PT-011, PT-012, PT-021, PT-025, PT-027, PT-028, PT-029 | 26 |
| Sprint 4 | Operation execution (queue/start/pause/complete), scrap, holds, materials and consumption | PT-022, PT-030, PT-031, PT-032, PT-033, PT-034, PT-038, PT-039, PT-040 | 25 |
| Sprint 5 | QC inspections and rework, live supervisor dashboard (WIP, late, throughput, scrap), friendly reconnect / cold-start UX | PT-035, PT-036, PT-037, PT-041, PT-072, PT-042, PT-043 | 25 |
| Sprint 6 | Downtime, OEE-lite, timeline, audit viewer, low-stock alert, traveler reprint, FTP deploy alternative and rollback rehearsal | PT-008, PT-026, PT-044, PT-045, PT-046, PT-048, PT-049 | 19 |
| Sprint 7 | Hardening: E2E release gate, manual regression run, performance and security checks, optional Google sign-in, portability check, docs, v1.0 release | PT-050, PT-051, PT-052, PT-068, PT-070, PT-056, PT-053, PT-054 | 22 |
| **MVP total** | | 61 stories | **195** |
| Sprint 8 (optional) | OPTIONAL after MVP: alternative hosting targets (Azure Bicep, deployTarget=azure|gcp), GitHub Packages NuGet feed | PT-057, PT-058, PT-071 | 12 |
| Phase 2+ | Not scheduled | PT-060, PT-061, PT-062, PT-063, PT-064, PT-065, PT-066, PT-069, PT-073 | 71 |

Sprint 6 is intentionally lighter to leave buffer before the release sprint. All MVP work costs US$0 (free MonsterASP plan, GitHub Free with 2,000 Actions minutes/month on private repos). The custom domain `prodtrack.dmbwebsolutions.com` is deferred (upgrade path in `docs/03-tech-stack-and-cloud.md` section 7). Sprint 8 is optional and needs an Azure (e.g. Azure for Students) or Google Cloud subscription; skip it if time is short.

## 4. Epics and features overview

| Epic | Features | Stories |
|---|---|---|
| **E01** Platform and DevOps Foundation | F01.1 GitHub repository, Projects board and CI<br>F01.2 Solution skeleton and cross-cutting concerns<br>F01.3 Infrastructure as code and continuous delivery | 11 |
| **E02** Identity and Access | F02.1 Authentication and authorization<br>F02.2 User profiles | 4 |
| **E03** Master Data | F03.1 Stations and routings<br>F03.2 Products and specifications<br>F03.3 Reason codes | 5 |
| **E04** Orders and Work Orders | F04.1 Sales orders<br>F04.2 Work orders<br>F04.3 Artwork proofing | 7 |
| **E05** Job Traveler | F05.1 Traveler generation | 2 |
| **E06** Shop Floor Execution | F06.1 PWA shell and scanning<br>F06.2 Operation tracking<br>F06.3 Scrap | 8 |
| **E07** Quality Control | F07.1 Inspections | 3 |
| **E08** Materials and Inventory Basics | F08.1 Materials | 3 |
| **E09** Supervisor Dashboard and Reporting | F09.1 Live dashboard<br>F09.2 Downtime and OEE-lite | 7 |
| **E10** Audit and Compliance | F10.1 Audit trail | 3 |
| **E11** Release Readiness | F11.1 Hardening and release | 8 |
| **E12** Optional Alternative Hosting Targets (Azure, Google Cloud) | F12.1 Alternative targets and packages | 3 |
| **E13** Phase 2 and Later (not scheduled) | F13.1 Phase 2 candidates | 9 |

## 5. Detailed backlog

### E01 - Platform and DevOps Foundation

Solution skeleton, GitHub repo/Projects/Actions CI/CD, MonsterASP hosting setup, logging and error handling so every later feature ships through the same automated path.

#### F01.1 - GitHub repository, Projects board and CI

##### PT-001: Set up GitHub repository, Projects board and backlog issues

- **Story:** As a **developer**, I want a private GitHub repo deo-bernal/dmb-prodtrack, a GitHub Projects board, and the backlog imported as issues with labels and sprint milestones, so that work is tracked and every change is linked to a story.
- **Points:** 3 | **Sprint:** Sprint 0 | **Tags:** `devops;setup;github`
- **Notes:** Follow docs/11-github-setup.md sections 1-4. Azure DevOps Boards import (backlog.csv) is the Phase 2 path in docs/04.
- **Acceptance criteria:**

```gherkin
Scenario: Repo exists
  Given I have a GitHub account deo-bernal
  When I create the private repo dmb-prodtrack from the repo-scaffold files
  Then main is the default branch, squash merge only, auto-delete head branches on, and the PR template and CODEOWNERS are in place

Scenario: Backlog imported
  Given gh is authenticated and the repo exists
  When I run scripts/create-github-issues.sh (dry run first)
  Then labels, milestones Sprint 0 to Sprint 8 and Phase 2, and one issue per PT story exist, re-running creates no duplicates, and the issues are on the Projects board with a Points field

Scenario: Board views
  Given the Projects board
  When I open it
  Then a Board view by Status (Todo, In progress, In review, Done) and a Table view grouped by milestone show the sprint scope and points
```

##### PT-002: CI workflow: build, test and code coverage on every PR

- **Story:** As a **developer**, I want a GitHub Actions workflow (.github/workflows/ci.yml on ubuntu-latest) that restores, checks formatting, builds, runs unit and integration tests and publishes results and coverage, so that broken code never reaches main.
- **Points:** 5 | **Sprint:** Sprint 0 | **Tags:** `devops;ci;testing;github`
- **Notes:** Ready-to-commit file in repo-scaffold/.github/workflows/ci.yml; details in docs/11 section 6. Coverage gate starts as warning only.
- **Acceptance criteria:**

```gherkin
Scenario: PR validation
  Given a pull request targets main
  When the ci workflow runs
  Then build, unit tests, integration tests (SQL Server service container) and the coverage summary appear on the run and the PR shows a failing check if any step fails

Scenario: Artifact
  Given a push to main passes CI
  When the run finishes
  Then an artifact prodtrack-drop with the win-x86 publish folder and the idempotent migrations.sql is available for the deploy workflow

Scenario: Minutes budget
  Given the repo is private on GitHub Free
  When I check Settings > Billing after a sprint
  Then Actions usage stays well under the 2,000 included minutes (concurrency cancels superseded runs, caching of NuGet packages)
```

#### F01.2 - Solution skeleton and cross-cutting concerns

##### PT-003: Create clean-architecture solution skeleton

- **Story:** As a **developer**, I want the ProdTrack solution with Domain, Application, Infrastructure, Contracts, ApiClient, UI.Shared, Server (API + SignalR + Blazor UI host, InProcess hosting for IIS) and ShopFloor (Blazor WebAssembly PWA) projects, test projects, and a docker-compose.yml for local SQL Server, so that all later stories have a consistent place to live.
- **Points:** 5 | **Sprint:** Sprint 0 | **Tags:** `architecture;setup`
- **Notes:** Structure is defined in docs/02-technical-design.md section 4. Cloud adapter projects (Infrastructure.Azure / .Gcp) are not created in the MVP; add them only if an alternative target is activated.
- **Acceptance criteria:**

```gherkin
Scenario: Solution builds
  Given a fresh clone
  When I run dotnet build and dotnet test
  Then both succeed with zero warnings treated as errors in src projects

Scenario: Health endpoint
  Given the Server is running locally
  When I call GET /health/ready
  Then I get 200 with status Healthy including a database check, and GET /health/live returns 200 without touching the database

Scenario: Dependency rule enforced
  Given the solution
  When an architecture test runs
  Then it fails if Domain or Application reference Infrastructure, Server or any UI project
```

##### PT-004: Structured logging (Serilog rolling file + console, OpenTelemetry-ready)

- **Story:** As a **support engineer**, I want structured logs with a correlation ID for every request written to rolling log files in the app data folder (and to the console locally), so that I can trace a problem on the MonsterASP site without a cloud logging service.
- **Points:** 3 | **Sprint:** Sprint 0 | **Tags:** `observability`
- **Notes:** Application code only uses ILogger, ActivitySource and Meter. Log files can be downloaded via FTP or the control panel for diagnosis.
- **Acceptance criteria:**

```gherkin
Scenario: Correlation
  Given a request enters the Server
  When it is processed
  Then every log line for that request carries the same correlation ID and it is returned in the X-Correlation-Id response header

Scenario: Rolling files within limits
  Given the app runs on MonsterASP
  When it logs for several days
  Then logs are written as compact JSON to App_Data/logs with daily rolling, 10 MB per file and at most 14 files, so disk use stays small

Scenario: Exporter by configuration
  Given Telemetry:Exporter is None, Console or Otlp
  When the app starts
  Then only the configured sinks/exporters are active; OpenTelemetry export is off by default to save memory
```

##### PT-005: Global error handling with ProblemDetails

- **Story:** As an **API consumer**, I want consistent RFC 9457 ProblemDetails error responses, so that clients can show meaningful messages.
- **Points:** 2 | **Sprint:** Sprint 0 | **Tags:** `api;error-handling`
- **Acceptance criteria:**

```gherkin
Scenario: Validation error
  Given a request with invalid data
  When the API validates it
  Then it returns 400 with a ProblemDetails body listing field errors

Scenario: Unhandled error
  Given an unexpected exception occurs
  When the request fails
  Then the API returns 500 ProblemDetails with a traceId and no stack trace outside Development
```

#### F01.3 - Infrastructure as code and continuous delivery

##### PT-006: Local environment and MonsterASP-ready publish configuration

- **Story:** As a **developer**, I want a documented local setup (SQL Server LocalDB/Express or Docker) and a publish configuration for MonsterASP (framework-dependent, win-x86, InProcess, web.config, App_Data folders excluded from deployment), so that the same build runs on my PC and on the free hosting.
- **Points:** 3 | **Sprint:** Sprint 0 | **Tags:** `devops;hosting;monsterasp`
- **Notes:** Self-contained deployment is optional and unverified on MonsterASP; framework-dependent is the documented route.
- **Acceptance criteria:**

```gherkin
Scenario: Local run
  Given a fresh clone on my PC
  When I follow docs/08 section 2
  Then the app runs with SQL Server LocalDB or the Docker SQL Server container and Auth:Mode=Dev

Scenario: Publish output
  Given I run dotnet publish -c Release -r win-x86 --self-contained false
  When the output is produced
  Then it contains web.config with hostingModel inprocess, no appsettings.Production.json secrets, and the App_Data folders are created at runtime, not deployed

Scenario: Lean memory
  Given the app runs locally in Release
  When I check the working set after startup and a few requests
  Then it stays well below the 256 MB limit of the free plan (target < 180 MB), with Server GC settings from docs/02 section 14
```

##### PT-055: MonsterASP account, site and database setup

- **Story:** As a **product owner**, I want a free MonsterASP account with one site (planned dmb-prodtrack.runasp.net), one MSSQL database, HTTPS enabled and remote database access enabled, so that the app has a zero-cost hosted environment.
- **Points:** 2 | **Sprint:** Sprint 0 | **Tags:** `hosting;monsterasp;setup`
- **Notes:** Free plan: learning/testing only, no SLA, no backups, sleeps after 30 minutes idle, EU servers. See docs/03 section 3.
- **Acceptance criteria:**

```gherkin
Scenario: Account and site
  Given I sign up for the free plan without a credit card
  When I create the website
  Then the site responds at https://<name>.runasp.net (planned dmb-prodtrack; alternative name recorded if taken) with a Let's Encrypt certificate

Scenario: Database
  Given the site exists
  When I create the MSSQL database and enable remote access under Databases > Users and remote
  Then I can connect from my PC with SSMS/sqlcmd and the connection string is stored only as the GitHub Actions secret PROD_DB_CONNECTION and in the server-only appsettings.Production.json

Scenario: Credentials stored safely
  Given FTP and Web Deploy credentials are shown in the control panel
  When I finish setup
  Then they are stored as GitHub Actions secrets (MONSTERASP_WEBDEPLOY_PASSWORD, MONSTERASP_FTP_*, PROD_DB_CONNECTION) and in my password manager, nowhere in the repo
```

##### PT-059: Repository guardrails on GitHub Free (secrets, deploy gate, Dependabot)

- **Story:** As a **developer**, I want GitHub Actions secrets and variables, a production environment, a manual deploy gate, Dependabot and documented merge rules that work on a private GitHub Free repo, so that deployments are deliberate and secrets stay safe without paid features.
- **Points:** 3 | **Sprint:** Sprint 0 | **Tags:** `devops;security;github`
- **Notes:** Plan limits verified on GitHub Docs 2026-09-26: required reviewers, environment secrets and protected branches/rulesets are available on private repos only with paid plans. Replaces the previous PT-059 (self-hosted Azure Pipelines agent), now part of the Phase 2 Azure DevOps migration PT-073.
- **Acceptance criteria:**

```gherkin
Scenario: Secrets
  Given the MonsterASP values from PT-055
  When I add them under Settings > Secrets and variables > Actions
  Then they are repository secrets/variables with the names in docs/11 section 5 and are never printed in logs

Scenario: Deploy gate
  Given the repo is private on GitHub Free (no required reviewers, environment secrets or branch protection)
  When code is merged to main
  Then nothing deploys until I run the deploy workflow manually (workflow_dispatch) on main; the run is recorded on the production environment

Scenario: Stronger gates when available
  Given the repo is made public, or GitHub Pro is available
  When I follow docs/11 section 7.2
  Then a ruleset requires PRs and the ci check on main, the production environment requires my approval, and AUTO_DEPLOY=true lets merges deploy after approval

Scenario: Dependencies
  Given Dependabot version updates are configured for nuget and github-actions
  When a new package version exists
  Then a weekly PR is opened
```

##### PT-007: Deploy workflow to MonsterASP (Web Deploy from windows-latest) with EF Core migrations

- **Story:** As a **developer**, I want a deploy workflow that takes the CI artifact of a main commit, applies the idempotent migration script to the MonsterASP database and syncs the site with Web Deploy (msdeploy) from a windows-latest runner, the method MonsterASP documents for GitHub Actions, so that every release reaches the hosted site the same repeatable way.
- **Points:** 5 | **Sprint:** Sprint 1 | **Tags:** `devops;cd;monsterasp;github`
- **Notes:** Ready-to-commit file repo-scaffold/.github/workflows/deploy.yml; docs/11 section 6. Windows minutes may count double against the free quota, so the build runs on ubuntu and only the short deploy job on Windows. FTP alternative is PT-008.
- **Acceptance criteria:**

```gherkin
Scenario: Deploy
  Given CI passed on main and I run the deploy workflow (or AUTO_DEPLOY is on and the production approval is given)
  When the deploy-webdeploy job runs
  Then it applies migrations with PROD_DB_CONNECTION, then runs msdeploy with -enableRule:AppOffline, skipping App_Data, logs and appsettings.Production.json

Scenario: Smoke check
  Given deployment finished
  When the workflow calls https://<name>.runasp.net/health/ready with retries (the site may be waking up)
  Then the job fails if it is not Healthy within about 3 minutes

Scenario: No secrets leaked
  Given the workflow runs
  When I read the logs
  Then the Web Deploy password and connection string are masked and never echoed
```

##### PT-008: FTP deploy alternative (ubuntu + lftp + app_offline.htm) and rollback rehearsal

- **Story:** As a **developer**, I want an alternative deploy job that uploads app_offline.htm, mirrors the publish folder with lftp from ubuntu-latest and removes app_offline.htm, plus a rehearsed rollback, so that I have a fallback if Web Deploy fails and can recover quickly from a bad release.
- **Points:** 3 | **Sprint:** Sprint 6 | **Tags:** `devops;cd;monsterasp;github`
- **Acceptance criteria:**

```gherkin
Scenario: FTP deploy
  Given repository variable DEPLOY_METHOD=ftp and the MONSTERASP_FTP_* secrets
  When I run the deploy workflow
  Then the deploy-ftp job runs instead of deploy-webdeploy, the site is offline only during the upload, and App_Data, logs and appsettings.Production.json are untouched

Scenario: Rollback
  Given a bad release is live
  When I run the deploy workflow with the run ID of the previous good CI run
  Then the previous build is live again within 10 minutes; migrations are forward-only (expand/contract)

Scenario: One deploy at a time
  Given two deploy runs are started
  When they overlap
  Then the concurrency group queues the second run
```

##### PT-067: Ops runbook: HTTPS renewal, database backup export and account activity

- **Story:** As a **product owner**, I want a runbook and reminders for the manual 90-day Let's Encrypt renewal, a weekly database backup export, and regular control-panel logins, so that the free site keeps working and I don't lose data (the free plan has no backups and may delete inactive accounts).
- **Points:** 2 | **Sprint:** Sprint 2 | **Tags:** `ops;monsterasp;runbook`
- **Notes:** Replaces the previous PT-067 (custom domain dev-prodtrack), deferred with the custom domain.
- **Acceptance criteria:**

```gherkin
Scenario: Certificate expiry check
  Given the scheduled GitHub Actions workflow ops.yml runs weekly (Monday 09:00 PHT)
  When the site certificate expires in less than 21 days
  Then the run fails with a clear message and I renew it in the control panel following docs/08 section 6

Scenario: Backup export
  Given remote database access is enabled
  When the weekly ops run executes sqlpackage /Action:Export (or I run it manually)
  Then a .bacpac encrypted with BACKUP_PASSPHRASE is stored as a workflow artifact (retention 28 days, within the 500 MB free storage) and I download a copy to my PC monthly

Scenario: Reminder
  Given the certificate was renewed
  When I update the runbook log
  Then a calendar reminder and a GitHub issue (milestone of the sprint due) exist for the next renewal in about 80 days
```

### E02 - Identity and Access

ASP.NET Core Identity sign-in with roles (optional Google sign-in) and authorization policies for all clients; Microsoft Entra ID is a later alternative.

#### F02.1 - Authentication and authorization

##### PT-009: Sign in to the web app with ASP.NET Core Identity

- **Story:** As a **plant employee**, I want to sign in with the account an admin created for me, so that only known users can use the system.
- **Points:** 3 | **Sprint:** Sprint 1 | **Tags:** `security;auth;identity`
- **Notes:** No self-registration. Microsoft Entra ID is PT-069 (Phase 2, needs a tenant); Google sign-in is PT-068.
- **Acceptance criteria:**

```gherkin
Scenario: Sign in
  Given I have an active account
  When I enter my email and password
  Then I am signed in and land on the page for my role

Scenario: Lockout
  Given I enter a wrong password 5 times
  When I try again
  Then the account is locked for 15 minutes and the attempt is logged

Scenario: Bootstrap admin
  Given an empty database and Auth:BootstrapAdmin:Email set
  When the app starts for the first time
  Then that admin account is created with the password from server-only configuration (appsettings.Production.json on the host / user-secrets locally) and must change it at first sign-in

Scenario: Local dev mode
  Given the app runs in Development with Auth:Mode=Dev
  When I open the app
  Then I can pick a test user and role; the app refuses to start with Auth:Mode=Dev in any other environment
```

##### PT-010: Role-based authorization policies

- **Story:** As an **admin**, I want features restricted by role (Admin, Planner, Supervisor, Operator, QC, Viewer), so that people only do what their job requires.
- **Points:** 3 | **Sprint:** Sprint 1 | **Tags:** `security;auth`
- **Acceptance criteria:**

```gherkin
Scenario: Forbidden
  Given I am a Viewer
  When I call POST /api/v1/work-orders
  Then I receive 403

Scenario: UI hides actions
  Given I am an Operator
  When I open the web app navigation
  Then I do not see Admin or Planning menus

Scenario: Role matrix tested
  Given the policy definitions
  When the authorization tests run
  Then each endpoint is verified against the role matrix in docs/02-technical-design.md
```

##### PT-011: Sign in on the shop-floor PWA

- **Story:** As an **operator**, I want to sign in on the tablet PWA with my account, so that my actions are recorded under my name.
- **Points:** 2 | **Sprint:** Sprint 3 | **Tags:** `security;auth;shopfloor`
- **Notes:** Shared-tablet badge login is Phase 2 (PT-060).
- **Acceptance criteria:**

```gherkin
Scenario: PWA sign-in
  Given a tablet with the PWA installed
  When I tap Sign in
  Then I sign in through the Server's Identity pages (same-origin cookie session, BFF style, no tokens in the browser) and my name and role appear in the header

Scenario: Session
  Given I have been signed in during a shift
  When the session approaches expiry
  Then it is renewed by activity (sliding, 12 h max) and when it expires I am sent to sign-in with a clear message without losing an unsaved scan
```

#### F02.2 - User profiles

##### PT-012: User administration and profiles (employee number, badge, roles)

- **Story:** As an **admin**, I want to create users, assign roles, reset passwords, deactivate users and set employee number, badge code and home station, so that access is controlled and operators can be identified by badge scans.
- **Points:** 5 | **Sprint:** Sprint 3 | **Tags:** `admin;users;identity`
- **Notes:** Roles are Identity roles. With Google sign-in (PT-068) a Google account can only be linked to a user created here.
- **Acceptance criteria:**

```gherkin
Scenario: Create user
  Given I am an Admin
  When I create a user with email, name, role Operator and a temporary password
  Then the user exists, must change the password at first sign-in, and the change is audited

Scenario: Deactivate
  Given a user leaves
  When I deactivate the account
  Then the user can no longer sign in and existing sessions end within 30 minutes (security stamp)

Scenario: Edit profile
  Given I am an Admin
  When I set employee number and badge code for a user
  Then the values are saved, badge code is unique and the change is audited
```

### E03 - Master Data

Stations, products and specifications, routings, reason codes and reference standards.

#### F03.1 - Stations and routings

##### PT-013: Manage production stations

- **Story:** As an **admin**, I want to create, edit and deactivate stations (code, name, type, work center), so that work can be routed to real equipment areas.
- **Points:** 3 | **Sprint:** Sprint 1 | **Tags:** `master-data`
- **Notes:** Seed stations: PREPRESS, PRINT-01, LAM-01, ENGRAVE-01, DIECUT-01, QC-01, PACK-01.
- **Acceptance criteria:**

```gherkin
Scenario: Create station
  Given I am an Admin
  When I create station PRINT-01 of type Printing
  Then it appears in the station list and can be used in routings

Scenario: Unique code
  Given station PRINT-01 exists
  When I create another station with code PRINT-01
  Then I see a validation error

Scenario: Deactivate
  Given a station has open operations
  When I deactivate it
  Then I am warned and it is hidden from new routings but open work is unaffected
```

##### PT-014: Manage routing templates

- **Story:** As a **planner**, I want to define an ordered list of stations with standard minutes per unit and setup minutes for each product type, so that work orders get the right steps automatically.
- **Points:** 5 | **Sprint:** Sprint 2 | **Tags:** `master-data;routing`
- **Acceptance criteria:**

```gherkin
Scenario: Define routing
  Given product type Pipe Marker
  When I add steps 10 Prepress, 20 Printing, 30 Laminating, 40 Die-cutting, 50 QC, 60 Packing
  Then the routing is saved as version 1

Scenario: Versioning
  Given routing version 1 is used by released work orders
  When I edit the routing
  Then a new version is created and existing work orders keep version 1
```

#### F03.2 - Products and specifications

##### PT-015: Manage product catalog with specifications

- **Story:** As a **planner**, I want a catalog of products (pipe markers, valve tags, safety signs, labels) with default specs, so that orders are entered quickly and consistently.
- **Points:** 5 | **Sprint:** Sprint 1 | **Tags:** `master-data;product`
- **Acceptance criteria:**

```gherkin
Scenario: Create product
  Given I am a Planner
  When I create a Pipe Marker product with material, size, color scheme, standard and mounting
  Then it is saved with a unique SKU

Scenario: Type-specific fields
  Given product type Valve Tag
  When I open the form
  Then I see tag shape, diameter, material thickness and hole size instead of pipe OD
```

##### PT-016: Seed reference data for ASME A13.1 and ANSI Z535

- **Story:** As a **planner**, I want reference lists for pipe-marker color schemes and safety-sign signal words, so that specs use standard values instead of free text.
- **Points:** 2 | **Sprint:** Sprint 1 | **Tags:** `master-data;reference-data`
- **Notes:** Values must be checked against the current editions of the standards before real use; see glossary in docs/01.
- **Acceptance criteria:**

```gherkin
Scenario: Pipe marker schemes
  Given a new database
  When seed data runs
  Then color schemes such as Flammable (black on yellow) and Fire quenching (white on red) exist and are editable by Admin

Scenario: Signal words
  Given a new database
  When seed data runs
  Then DANGER, WARNING, CAUTION, NOTICE and SAFETY INSTRUCTIONS exist with their header colors
```

#### F03.3 - Reason codes

##### PT-017: Manage scrap, pause and downtime reason codes

- **Story:** As an **admin**, I want configurable reason code lists by category, so that operators pick a reason instead of typing.
- **Points:** 2 | **Sprint:** Sprint 1 | **Tags:** `master-data;quality`
- **Acceptance criteria:**

```gherkin
Scenario: Create reason
  Given I am an Admin
  When I add scrap reason MISPRINT in category Scrap
  Then operators can select it when logging scrap

Scenario: Deactivate
  Given a reason code has history
  When I deactivate it
  Then it is not offered for new entries but history still shows it
```

### E04 - Orders and Work Orders

Turn customer orders into released, routed work orders with approved artwork.

#### F04.1 - Sales orders

##### PT-018: Enter a customer sales order with lines

- **Story:** As a **planner**, I want to enter a sales order with customer, PO number, due date and lines (product, specs, legend text, quantity), so that production knows exactly what to make.
- **Points:** 5 | **Sprint:** Sprint 2 | **Tags:** `orders`
- **Acceptance criteria:**

```gherkin
Scenario: Create order
  Given I am a Planner
  When I enter an order with two lines and save
  Then the order gets number SO-yyyy-nnnnn and status Open

Scenario: Legend required
  Given a pipe marker line
  When I save without legend text
  Then I see a validation error

Scenario: Spec override
  Given a line based on a catalog product
  When I change the color scheme on the line
  Then only that line's spec changes, not the catalog product
```

#### F04.2 - Work orders

##### PT-019: Generate work orders from sales order lines

- **Story:** As a **planner**, I want to create one work order per sales order line, so that each line is tracked through production.
- **Points:** 5 | **Sprint:** Sprint 2 | **Tags:** `work-orders`
- **Acceptance criteria:**

```gherkin
Scenario: Generate
  Given an Open sales order with 3 lines
  When I click Create work orders
  Then 3 work orders in Draft status are created with number WO-yyyy-nnnnnn, copied specs, quantity and due date

Scenario: No duplicates
  Given a line already has a work order
  When I click Create work orders again
  Then no duplicate is created and I am told which lines were skipped
```

##### PT-020: Release a work order to the floor

- **Story:** As a **planner**, I want to release a Draft work order, so that operations are created from the routing and appear in station queues.
- **Points:** 3 | **Sprint:** Sprint 2 | **Tags:** `work-orders;routing`
- **Acceptance criteria:**

```gherkin
Scenario: Release
  Given a Draft work order whose artwork is approved
  When I release it
  Then status becomes Released and one operation per routing step is created with status Pending, the first one Ready

Scenario: Artwork gate
  Given a work order whose product requires artwork and the proof is not approved
  When I try to release it
  Then release is blocked with message Artwork not approved
```

##### PT-021: Planner work order list with filters

- **Story:** As a **planner**, I want to search and filter work orders by status, customer, due date and station, so that I can manage the schedule.
- **Points:** 3 | **Sprint:** Sprint 3 | **Tags:** `work-orders;ui`
- **Acceptance criteria:**

```gherkin
Scenario: Filter
  Given 200 work orders exist
  When I filter Status = Released and Due before Friday
  Then only matching rows show, paged 25 per page, sorted by due date

Scenario: Late flag
  Given a work order is past due and not completed
  When I view the list
  Then the row is marked Late
```

##### PT-022: Hold, resume and cancel a work order

- **Story:** As a **supervisor**, I want to put a work order on hold with a reason, resume it, or cancel it, so that problems stop work until resolved.
- **Points:** 3 | **Sprint:** Sprint 4 | **Tags:** `work-orders`
- **Acceptance criteria:**

```gherkin
Scenario: Hold
  Given a Released or In Progress work order
  When I put it on hold with reason Customer change
  Then status is On Hold and operators cannot start its operations

Scenario: Cancel
  Given a work order with no completed operations
  When I cancel it with a reason
  Then status is Cancelled and it leaves all queues

Scenario: Audit
  Given any of these actions
  When it succeeds
  Then an audit entry records who, when, old and new status and the reason
```

#### F04.3 - Artwork proofing

##### PT-023: Upload artwork proof

- **Story:** As a **prepress operator**, I want to upload a PDF or image proof to a work order, so that the customer-approved design is stored with the job.
- **Points:** 3 | **Sprint:** Sprint 2 | **Tags:** `artwork;storage`
- **Acceptance criteria:**

```gherkin
Scenario: Upload
  Given a Draft work order
  When I upload proof.pdf (max 20 MB)
  Then it is stored through IFileStorage (local disk under App_Data/files on MonsterASP and locally; cloud providers only on the optional alternatives) as version 1 with status Pending approval

Scenario: Wrong type
  Given I choose a .exe file
  When I upload
  Then the upload is rejected; only PDF, PNG, JPG, SVG are allowed
```

##### PT-024: Approve or reject artwork proof

- **Story:** As a **planner**, I want to record approval or rejection (for example after customer sign-off), so that only approved artwork is produced.
- **Points:** 3 | **Sprint:** Sprint 2 | **Tags:** `artwork`
- **Acceptance criteria:**

```gherkin
Scenario: Approve
  Given a Pending proof
  When I approve it with a note
  Then status is Approved with my name and time and the work order can be released

Scenario: Reject
  Given a Pending proof
  When I reject it with a reason
  Then status is Rejected and a new version must be uploaded
```

### E05 - Job Traveler

Printable paperwork that travels with the job and carries scannable QR codes.

#### F05.1 - Traveler generation

##### PT-025: Generate printable job traveler PDF with QR codes

- **Story:** As a **planner**, I want a one-page A4/Letter traveler with work order details, specs, legend, routing steps and QR codes, so that operators scan it instead of typing.
- **Points:** 5 | **Sprint:** Sprint 3 | **Tags:** `traveler;pdf`
- **Notes:** QR payload format is defined in docs/02-technical-design.md section 9.
- **Acceptance criteria:**

```gherkin
Scenario: Generate
  Given a Released work order
  When I click Print traveler
  Then a PDF opens showing header QR (work order), one QR per operation, specs, legend text and due date

Scenario: Scannable
  Given the printed traveler
  When a scanner reads the header QR
  Then the value is WO:WO-yyyy-nnnnnn and per-operation codes are OP:WO-yyyy-nnnnnn:seq
```

##### PT-026: Reprint traveler with audit

- **Story:** As a **supervisor**, I want to reprint a lost or damaged traveler, so that work can continue, and reprints are traceable.
- **Points:** 1 | **Sprint:** Sprint 6 | **Tags:** `traveler;audit`
- **Acceptance criteria:**

```gherkin
Scenario: Reprint
  Given a traveler was printed before
  When I reprint it
  Then the PDF shows REPRINT n and an audit entry is written
```

### E06 - Shop Floor Execution

Tablet PWA for operators to scan, start, pause, complete operations and log scrap.

#### F06.1 - PWA shell and scanning

##### PT-027: Installable shop-floor PWA shell with station selection

- **Story:** As an **operator**, I want an installable tablet app with large touch targets where I pick my station, so that I see only my station's work.
- **Points:** 3 | **Sprint:** Sprint 3 | **Tags:** `shopfloor;pwa`
- **Acceptance criteria:**

```gherkin
Scenario: Install
  Given Chrome or Edge on an Android or Windows tablet
  When I open the ShopFloor URL
  Then I am offered Install app and it launches full screen

Scenario: Station
  Given I am signed in
  When I select or scan station PRINT-01
  Then the choice is remembered on this device
```

##### PT-028: Scan QR/barcode via hardware scanner or camera

- **Story:** As an **operator**, I want to scan traveler codes with a USB/Bluetooth scanner or the tablet camera, so that I never type work order numbers.
- **Points:** 5 | **Sprint:** Sprint 3 | **Tags:** `shopfloor;scanning`
- **Acceptance criteria:**

```gherkin
Scenario: Hardware scanner
  Given the scan box has focus
  When a keyboard-wedge scanner sends OP:WO-2026-000123:20 plus Enter
  Then the operation is looked up and shown within 1 second

Scenario: Camera
  Given I tap Scan with camera
  When I point at a traveler QR
  Then the code is decoded and handled the same way

Scenario: Bad code
  Given I scan an unknown code
  When it is processed
  Then I see a clear red message and hear an error tone
```

##### PT-029: Set up Playwright E2E project with first smoke test

- **Story:** As a **developer**, I want a Playwright test project that signs in with a test user and loads the dashboard and shop-floor home, so that UI regressions are caught early.
- **Points:** 3 | **Sprint:** Sprint 3 | **Tags:** `testing;e2e`
- **Acceptance criteria:**

```gherkin
Scenario: Local run
  Given the app runs locally
  When I run dotnet test on ProdTrack.E2E.Tests
  Then the smoke tests pass headless

Scenario: CI workflow
  Given the ci workflow starts the app on ubuntu-latest against a SQL Server service container
  When the e2e job runs
  Then results and Playwright traces are uploaded as artifacts on failure
```

#### F06.2 - Operation tracking

##### PT-030: Station queue

- **Story:** As an **operator**, I want to see Ready and In Progress operations for my station ordered by priority and due date, so that I know what to work on next.
- **Points:** 3 | **Sprint:** Sprint 4 | **Tags:** `shopfloor`
- **Acceptance criteria:**

```gherkin
Scenario: Queue
  Given 3 operations are Ready at PRINT-01
  When I open the queue
  Then they are listed with WO number, product, qty, due date and priority; late jobs are highlighted

Scenario: Live
  Given a planner releases a new work order routed to PRINT-01
  When I am on the queue screen
  Then the new operation appears without refresh
```

##### PT-031: Start an operation by scanning

- **Story:** As an **operator**, I want to start an operation by scanning its traveler code, so that actual start time and operator are recorded.
- **Points:** 3 | **Sprint:** Sprint 4 | **Tags:** `shopfloor;execution`
- **Acceptance criteria:**

```gherkin
Scenario: Start
  Given a Ready operation at my station
  When I scan it and tap Start
  Then status is In Progress with my user ID and server UTC time, and the work order becomes In Progress

Scenario: Wrong station
  Given the operation belongs to LAM-01
  When I scan it at PRINT-01
  Then I am told the correct station and cannot start it

Scenario: Previous step
  Given the previous operation is not complete
  When I try to start
  Then I am blocked unless a Supervisor has allowed overlap for this routing step

Scenario: On hold
  Given the work order is On Hold
  When I scan it
  Then Start is disabled and the hold reason is shown
```

##### PT-032: Pause and resume an operation with reason

- **Story:** As an **operator**, I want to pause an operation with a reason (break, material wait, machine issue) and resume it, so that time tracking is accurate.
- **Points:** 2 | **Sprint:** Sprint 4 | **Tags:** `shopfloor;execution`
- **Acceptance criteria:**

```gherkin
Scenario: Pause
  Given an In Progress operation
  When I tap Pause and choose Material wait
  Then status is Paused and a pause event with reason is stored

Scenario: Resume
  Given a Paused operation
  When I tap Resume
  Then status returns to In Progress and paused time is excluded from run time
```

##### PT-033: Complete an operation with good quantity

- **Story:** As an **operator**, I want to complete an operation by entering good quantity, so that the next station can start.
- **Points:** 3 | **Sprint:** Sprint 4 | **Tags:** `shopfloor;execution`
- **Acceptance criteria:**

```gherkin
Scenario: Complete
  Given an In Progress operation for 100 units
  When I enter good qty 98 and scrap 2 was logged
  Then status is Completed and the next operation becomes Ready with input qty 98

Scenario: Qty check
  Given good plus scrap is more than input qty
  When I complete
  Then I see a validation error

Scenario: Last step
  Given I complete the last operation
  When it saves
  Then the work order becomes Completed
```

#### F06.3 - Scrap

##### PT-034: Log scrap with reason code

- **Story:** As an **operator**, I want to log scrapped units with a reason code and optional note/photo, so that scrap is measured and root causes found.
- **Points:** 3 | **Sprint:** Sprint 4 | **Tags:** `shopfloor;quality;scrap`
- **Acceptance criteria:**

```gherkin
Scenario: Log scrap
  Given an In Progress operation
  When I log 2 units with reason MISPRINT
  Then a scrap record is saved and the dashboard scrap rate updates

Scenario: Reason required
  Given I log scrap
  When no reason is chosen
  Then Save is disabled
```

### E07 - Quality Control

Checklists and inspection results with hold/rework on failure.

#### F07.1 - Inspections

##### PT-035: QC checklist templates per product type

- **Story:** As a **QC inspector**, I want checklist templates (for example legend spelling, color scheme vs spec, dimensions, adhesion, engraving depth), so that inspections are consistent.
- **Points:** 3 | **Sprint:** Sprint 5 | **Tags:** `quality`
- **Acceptance criteria:**

```gherkin
Scenario: Template
  Given I am QC
  When I create a Pipe Marker checklist with 5 items, some requiring a measured value with tolerance
  Then it is used for new QC operations of that product type
```

##### PT-036: Record QC inspection result

- **Story:** As a **QC inspector**, I want to record pass/fail per checklist item, sample size and measurements at the QC station, so that quality evidence is stored with the job.
- **Points:** 5 | **Sprint:** Sprint 5 | **Tags:** `quality;shopfloor`
- **Acceptance criteria:**

```gherkin
Scenario: Pass
  Given a QC operation is In Progress
  When all items pass and I submit
  Then the inspection is Passed, the QC operation completes and packing becomes Ready

Scenario: Out of tolerance
  Given a measured item outside tolerance
  When I enter the value
  Then the item is automatically marked Fail
```

##### PT-037: Failed QC puts work order on hold or creates rework

- **Story:** As a **QC inspector**, I want a failed inspection to either hold the work order or send it back to a chosen station for rework, so that defective product never ships.
- **Points:** 3 | **Sprint:** Sprint 5 | **Tags:** `quality`
- **Acceptance criteria:**

```gherkin
Scenario: Fail with rework
  Given an inspection fails
  When I choose Rework at PRINT-01 for 10 units
  Then a rework operation is added at PRINT-01 and a new QC operation follows it

Scenario: Fail with hold
  Given an inspection fails
  When I choose Hold
  Then the work order is On Hold with reason QC failed and supervisors are notified on the dashboard
```

### E08 - Materials and Inventory Basics

Track stock of key materials and consumption per work order.

#### F08.1 - Materials

##### PT-038: Manage materials and stock levels

- **Story:** As a **planner**, I want a list of materials (vinyl roll, polyester, aluminum blank, stainless blank, laminate, ink) with unit of measure, on-hand qty and reorder point, so that I know what is available.
- **Points:** 3 | **Sprint:** Sprint 4 | **Tags:** `inventory`
- **Acceptance criteria:**

```gherkin
Scenario: Create
  Given I am a Planner
  When I add material VINYL-WHT-24 in square meters with reorder point 50
  Then it appears in the material list with on-hand 0
```

##### PT-039: Receive and adjust stock

- **Story:** As a **planner**, I want to record receipts and adjustments with a reason, so that on-hand quantities stay accurate.
- **Points:** 2 | **Sprint:** Sprint 4 | **Tags:** `inventory`
- **Acceptance criteria:**

```gherkin
Scenario: Receipt
  Given VINYL-WHT-24 has 10 on hand
  When I receive 100
  Then on-hand is 110 and a stock transaction is stored

Scenario: Negative block
  Given on-hand is 5
  When I adjust -10
  Then I see an error unless I am Admin
```

##### PT-040: Record material consumption on operations

- **Story:** As an **operator**, I want materials to be consumed automatically from the bill of materials when I complete an operation, with an option to adjust, so that work order material cost and stock are accurate.
- **Points:** 3 | **Sprint:** Sprint 4 | **Tags:** `inventory;execution`
- **Acceptance criteria:**

```gherkin
Scenario: Backflush
  Given a product BOM says 0.05 m2 vinyl per unit at Printing
  When I complete Printing with 98 good and 2 scrap
  Then 5.0 m2 is consumed (good plus scrap) and on-hand is reduced

Scenario: Adjust
  Given the actual usage differs
  When I edit consumption before confirming
  Then the entered value is used and the difference is flagged
```

### E09 - Supervisor Dashboard and Reporting

Live visibility of WIP, late jobs, throughput, scrap and OEE-lite.

#### F09.1 - Live dashboard

##### PT-041: Live WIP by station

- **Story:** As a **supervisor**, I want a dashboard tile per station showing Ready, In Progress, Paused counts and units, so that I see bottlenecks as they happen.
- **Points:** 5 | **Sprint:** Sprint 5 | **Tags:** `dashboard;signalr`
- **Acceptance criteria:**

```gherkin
Scenario: Live update
  Given the dashboard is open
  When an operator starts an operation
  Then the station tile updates within 2 seconds without page reload

Scenario: Reconnect
  Given the network drops
  When it comes back
  Then the dashboard reconnects and reloads current numbers
```

##### PT-072: Friendly reconnect and cold-start experience

- **Story:** As an **operator**, I want clear, friendly messages and automatic recovery when the connection drops or the site is waking up, so that I am not confused when the free hosting sleeps after 30 minutes idle or the network blips.
- **Points:** 3 | **Sprint:** Sprint 5 | **Tags:** `ux;signalr;blazor;hosting`
- **Notes:** MonsterASP free sleeps after 30 minutes without requests (cannot be changed on free). Do not add keep-alive pings to defeat it (fair use).
- **Acceptance criteria:**

```gherkin
Scenario: Blazor reconnect
  Given a supervisor page is open and the server restarts or the app pool recycles
  When the circuit drops
  Then a branded reconnect overlay shows 'Reconnecting...', retries automatically with backoff, and reloads the page if the circuit cannot be resumed

Scenario: PWA reconnect
  Given a station screen is open
  When the SignalR connection drops
  Then a banner shows 'Offline - retrying', the connection is re-established automatically and the queue is re-fetched

Scenario: Cold start
  Given the site has been idle for more than 30 minutes
  When I open it
  Then a lightweight loading page appears within a few seconds and the app is usable without an error page
```

##### PT-042: Late and at-risk jobs list

- **Story:** As a **supervisor**, I want a list of work orders that are late or at risk (remaining standard time exceeds time to due date), so that I can act before customers are let down.
- **Points:** 3 | **Sprint:** Sprint 5 | **Tags:** `dashboard`
- **Notes:** At-risk formula in docs/01 section 9.
- **Acceptance criteria:**

```gherkin
Scenario: Late
  Given a work order due yesterday is not complete
  When I open the dashboard
  Then it is in the Late list with days late

Scenario: At risk
  Given remaining standard minutes exceed working minutes until due
  When I open the dashboard
  Then it is in the At risk list
```

##### PT-043: Throughput and scrap rate

- **Story:** As a **supervisor**, I want today, this week and custom-range throughput (units completed) and scrap rate by station and reason, so that I can track KPIs.
- **Points:** 3 | **Sprint:** Sprint 5 | **Tags:** `dashboard;kpi`
- **Acceptance criteria:**

```gherkin
Scenario: Scrap rate
  Given 100 good and 5 scrap units at PRINT-01 today
  When I view KPIs
  Then scrap rate for PRINT-01 today shows 4.8 percent (5 / 105)

Scenario: Pareto
  Given scrap from several reasons
  When I open the scrap chart
  Then reasons are sorted by quantity descending
```

##### PT-044: Work order timeline and history

- **Story:** As a **supervisor**, I want a timeline of every event on a work order (release, starts, pauses, scrap, QC, completion), so that I can answer where is my order and what happened.
- **Points:** 3 | **Sprint:** Sprint 6 | **Tags:** `work-orders;audit`
- **Acceptance criteria:**

```gherkin
Scenario: Timeline
  Given a work order with activity
  When I open its detail page
  Then events are listed in time order with user, station and Manila local time
```

#### F09.2 - Downtime and OEE-lite

##### PT-045: Log station downtime

- **Story:** As an **operator**, I want to record when my station is down and why, so that availability losses are visible.
- **Points:** 3 | **Sprint:** Sprint 6 | **Tags:** `shopfloor;oee`
- **Acceptance criteria:**

```gherkin
Scenario: Start/stop
  Given my station is running
  When I tap Station down and choose reason Printer fault, later tap Station up
  Then a downtime event with duration is stored and the station tile shows Down in red meanwhile
```

##### PT-046: OEE-lite per station

- **Story:** As a **supervisor**, I want an OEE-lite figure per station and shift (Availability x Performance x Quality), so that I can compare stations and see trend.
- **Points:** 5 | **Sprint:** Sprint 6 | **Tags:** `dashboard;kpi;oee`
- **Notes:** Formula and shift assumption in docs/01 section 9.
- **Acceptance criteria:**

```gherkin
Scenario: Calculate
  Given planned time 480 min, downtime 48 min, standard minutes earned 345.6, good 95 of 100 units
  When I view OEE-lite
  Then Availability 90 percent, Performance 80 percent, Quality 95 percent, OEE-lite 68.4 percent

Scenario: Explain
  Given I hover the OEE value
  When the tooltip opens
  Then it shows the three factors and states that it is a simplified OEE
```

### E10 - Audit and Compliance

Who changed what and when, for traceability.

#### F10.1 - Audit trail

##### PT-047: Capture audit trail automatically

- **Story:** As a **quality manager**, I want every create, update and delete on business entities to be recorded with user, time, old and new values, so that we have full traceability.
- **Points:** 3 | **Sprint:** Sprint 1 | **Tags:** `audit;security`
- **Notes:** Use an EF Core SaveChanges interceptor.
- **Acceptance criteria:**

```gherkin
Scenario: Update logged
  Given a Planner changes a work order due date
  When it is saved
  Then an audit entry holds entity, key, field changes (old to new), user and UTC time

Scenario: Immutable
  Given audit entries exist
  When anyone calls an update or delete on audit data
  Then no endpoint exists and the table grants insert/select only to the app identity
```

##### PT-048: Audit log viewer

- **Story:** As an **admin**, I want to search the audit log by entity, user and date, so that I can investigate issues.
- **Points:** 2 | **Sprint:** Sprint 6 | **Tags:** `audit;admin`
- **Acceptance criteria:**

```gherkin
Scenario: Search
  Given audit entries exist
  When I filter entity WorkOrder and key WO-2026-000123
  Then I see its full change history, paged
```

##### PT-049: Low-stock alert on dashboard

- **Story:** As a **planner**, I want a dashboard alert when on-hand falls below reorder point, so that we reorder in time.
- **Points:** 2 | **Sprint:** Sprint 6 | **Tags:** `inventory;dashboard`
- **Acceptance criteria:**

```gherkin
Scenario: Alert
  Given reorder point 50 and on-hand drops to 45
  When consumption is saved
  Then the Low stock widget lists the material in real time
```

### E11 - Release Readiness

Non-functional hardening, documentation and MVP release.

#### F11.1 - Hardening and release

##### PT-050: E2E regression suite as a release gate

- **Story:** As a **developer**, I want the main flows (order to work order, release, traveler, scan start/complete, QC, dashboard) covered by Playwright and required before prod, so that releases are safe.
- **Points:** 5 | **Sprint:** Sprint 7 | **Tags:** `testing;e2e;devops`
- **Acceptance criteria:**

```gherkin
Scenario: Gate
  Given the e2e job runs in the ci workflow against a throwaway database
  When E2E tests fail
  Then the deploy workflow refuses to deploy that commit (it requires a successful ci run)

Scenario: Post-deploy smoke
  Given Deploy_Prod finished
  When the read-only smoke subset runs against https://<name>.runasp.net
  Then sign-in page and health endpoint pass; no data is written to prod
```

##### PT-051: Performance smoke test

- **Story:** As a **developer**, I want a simple load test simulating 30 tablets and 5 dashboards, so that I know the free hosting is adequate.
- **Points:** 2 | **Sprint:** Sprint 7 | **Tags:** `testing;performance`
- **Notes:** Tool: k6 (free, runs locally). Keep the load small: the free plan has fair-use limits (docs/03).
- **Acceptance criteria:**

```gherkin
Scenario: Targets
  Given the MonsterASP site (warm) and a local Release build
  When the load test runs for 10 minutes
  Then server-side p95 for scan/start/complete is under 500 ms, memory stays under the 256 MB limit, there are no errors, and the network round trip from the Philippines to the EU servers is recorded separately; results are attached to the sprint review
```

##### PT-052: Security review and dependency scanning

- **Story:** As a **developer**, I want dependency vulnerability checks and a basic OWASP review, so that known vulnerabilities are not shipped.
- **Points:** 3 | **Sprint:** Sprint 7 | **Tags:** `security;devops`
- **Acceptance criteria:**

```gherkin
Scenario: Scan
  Given the ci workflow
  When it runs
  Then dotnet list package --vulnerable runs and fails the build on High or Critical findings
```

##### PT-068: Optional Google sign-in linked to existing users

- **Story:** As a **plant employee**, I want to sign in with my Google account, so that I don't need to remember another password.
- **Points:** 3 | **Sprint:** Sprint 7 | **Tags:** `security;auth;google`
- **Notes:** Enabled by Auth:Google:Enabled. Consent screen in Testing status with listed test users (basic scopes only). No billing account is needed for an OAuth client.
- **Acceptance criteria:**

```gherkin
Scenario: New OAuth client
  Given a new, billing-free Google Cloud project used only for this OAuth client (not find-an-agent)
  When I create a Web OAuth client with redirect URIs https://localhost:5001/signin-google and https://<name>.runasp.net/signin-google
  Then client ID and secret are stored in the server-only appsettings.Production.json and local user-secrets, never in the repo

Scenario: Linked user
  Given an Admin created my account with my Gmail address
  When I choose Sign in with Google
  Then the Google login is linked to my account and I get my Identity roles

Scenario: Unknown user
  Given no account exists for my Google email
  When I choose Sign in with Google
  Then access is denied with a message to contact an Admin and no user is created
```

##### PT-070: Manual release regression run (GitHub test-run checklist)

- **Story:** As a **developer**, I want a manual regression checklist for the MVP demo script, executed once for the v1.0 release as a GitHub issue created from a test-run issue template, so that I have a traceable release sign-off without paid tools.
- **Points:** 2 | **Sprint:** Sprint 7 | **Tags:** `testing;release;github`
- **Notes:** Azure Test Plans (paid after a 30-day trial) is covered in the Phase 2 Azure DevOps migration (PT-073, docs/04 section 13.1).
- **Acceptance criteria:**

```gherkin
Scenario: Run executed
  Given the test-run issue template with one checkbox per test case
  When I run the checklist against the MonsterASP site
  Then every case is ticked or linked to a bug issue, and the release issue links the run

Scenario: Traceability
  Given a failed case
  When I create a bug
  Then the bug issue references the PT story and the test-run issue
```

##### PT-056: Portability check: provider contract tests and a Docker run

- **Story:** As a **developer**, I want an IFileStorage/ISecretProvider contract test suite that the Local providers pass (and any future cloud provider must pass), plus a Docker image that runs the app locally, so that moving to an alternative host later is a configuration change, not a code change.
- **Points:** 2 | **Sprint:** Sprint 7 | **Tags:** `portability;testing;docker`
- **Acceptance criteria:**

```gherkin
Scenario: Contract tests
  Given the IFileStorage contract test suite
  When it runs against the Local provider (and Azurite / fake GCS if an alternative is activated)
  Then all providers pass the same tests, including paths with spaces and files up to 20 MB

Scenario: Container
  Given docker compose up
  When I open http://localhost:8080
  Then the app works with Cloud:Provider=Local, proving the build is host-independent
```

##### PT-053: Finalize user guide and release notes for MVP

- **Story:** As a **product owner**, I want the user guide updated with real screenshots and release notes for v1.0, so that plant staff can be trained.
- **Points:** 2 | **Sprint:** Sprint 7 | **Tags:** `docs`
- **Acceptance criteria:**

```gherkin
Scenario: Docs
  Given the MVP is feature complete
  When I review docs/07-user-guide.md
  Then every placeholder screenshot is replaced and each role section matches the app
```

##### PT-054: MVP release to production

- **Story:** As a **product owner**, I want v1.0 deployed to prod with seed master data and a tagged release, so that the MVP is live for demonstration.
- **Points:** 3 | **Sprint:** Sprint 7 | **Tags:** `release;devops`
- **Acceptance criteria:**

```gherkin
Scenario: Release
  Given all MVP stories are Done
  When I run the deploy workflow for the release commit
  Then the app is live, a git tag v1.0.0 and a GitHub Release with notes exist and the runbook smoke checks pass
```

### E12 - Optional Alternative Hosting Targets (Azure, Google Cloud)

Documented, switchable deploy targets for when a paid or student subscription exists, plus package publishing practice. Not needed for the zero-cost MVP.

#### F12.1 - Alternative targets and packages

##### PT-057: Azure alternative: Bicep for Container Apps or App Service

- **Story:** As a **developer**, I want Bicep for an Azure Container Apps (consumption) or App Service B1 environment with an Azure SQL free-offer database and managed identity, so that the app can move to Azure (e.g. Azure for Students) with a custom domain when needed.
- **Points:** 5 | **Sprint:** Sprint 8 | **Tags:** `devops;iac;azure;bicep;optional`
- **Notes:** Blocked until an Azure subscription exists. Needs the Azure adapter project (Infrastructure.Azure).
- **Acceptance criteria:**

```gherkin
Scenario: Deploy
  Given an Azure subscription (for example Azure for Students)
  When I deploy infra/azure/main.bicep with dev.bicepparam
  Then all resources are created without secrets in the templates and the SQL database uses the free offer with auto-pause

Scenario: Teardown
  Given I finish practising
  When I delete the resource group
  Then no billable resources remain
```

##### PT-058: Deploy target parameter: monsterasp | azure | gcp

- **Story:** As a **developer**, I want the deploy workflow input deployTarget with monsterasp as default and jobs for the alternatives, so that I can practise multi-target delivery from one workflow.
- **Points:** 5 | **Sprint:** Sprint 8 | **Tags:** `devops;cd;optional`
- **Notes:** GCP template reuses the v0.2 design (Cloud Run, Terraform) kept in docs/03 appendix.
- **Acceptance criteria:**

```gherkin
Scenario: Default
  Given I run the deploy workflow
  When no input is changed
  Then deployTarget defaults to monsterasp

Scenario: Alternative
  Given I run the workflow with deployTarget=azure (or gcp) and the target exists
  When it runs
  Then only the jobs for that target run (azure/login with OIDC federation, or google-github-actions/auth)
```

##### PT-071: Publish ProdTrack.Contracts as a NuGet package to GitHub Packages

- **Story:** As a **developer**, I want the Contracts package published to GitHub Packages (NuGet) by a workflow on version tags, so that I practise package management and future clients (MAUI) can consume the contracts.
- **Points:** 2 | **Sprint:** Sprint 8 | **Tags:** `devops;packages;optional`
- **Notes:** Azure Artifacts (2 GiB free) is the Phase 2 equivalent in docs/04 section 13.2.
- **Acceptance criteria:**

```gherkin
Scenario: Publish
  Given a tag v1.1.0 on main
  When the publish workflow runs with GITHUB_TOKEN (packages: write)
  Then ProdTrack.Contracts 1.1.0 appears under the repo's Packages and old versions are pruned to stay under the 500 MB free storage
```

### E13 - Phase 2 and Later (not scheduled)

Ideas beyond the MVP; refine before scheduling.

#### F13.1 - Phase 2 candidates

##### PT-060: Shared-tablet kiosk mode with badge scan and PIN

- **Story:** As an **operator**, I want to identify myself on a shared station tablet by scanning my badge and entering a PIN, so that operators don't need a personal password sign-in on every shared tablet.
- **Points:** 8 | **Sprint:** Phase 2+ (unscheduled) | **Tags:** `phase-2;shopfloor;security`
- **Notes:** Needs a security design review.
- **Acceptance criteria:**

```gherkin
Scenario: Badge login
  Given a tablet in kiosk mode signed in as a station device identity
  When I scan my badge and enter my PIN
  Then my actions are recorded under my user for 10 minutes of inactivity
```

##### PT-061: Offline queue for scans when Wi-Fi drops

- **Story:** As an **operator**, I want scans and completions to be queued locally and synced when the connection returns, so that work is not blocked by Wi-Fi gaps.
- **Points:** 8 | **Sprint:** Phase 2+ (unscheduled) | **Tags:** `phase-2;shopfloor;pwa`
- **Acceptance criteria:**

```gherkin
Scenario: Queue
  Given the tablet is offline
  When I complete an operation
  Then it is stored locally, marked Pending sync and sent in order when back online; conflicts are shown to a supervisor
```

##### PT-062: .NET MAUI mobile app for supervisors

- **Story:** As a **supervisor**, I want a native mobile app with dashboard and push notifications, so that I can follow the floor when away from a PC.
- **Points:** 13 | **Sprint:** Phase 2+ (unscheduled) | **Tags:** `phase-2;mobile;maui`
- **Acceptance criteria:**

```gherkin
Scenario: Dashboard
  Given I open the MAUI app
  When I sign in
  Then I see the same KPIs as the web dashboard, reusing the Razor class library via Blazor Hybrid
```

##### PT-063: Print labels and travelers directly to industrial label printers

- **Story:** As a **planner**, I want to send traveler labels to a thermal label printer (for example ZPL), so that we avoid manual printing steps.
- **Points:** 8 | **Sprint:** Phase 2+ (unscheduled) | **Tags:** `phase-2;printing`
- **Notes:** Printer models to be confirmed.
- **Acceptance criteria:**

```gherkin
Scenario: Print
  Given a configured printer
  When I click Print label
  Then the job is sent and its status recorded
```

##### PT-064: Import sales orders from CSV or an ERP API

- **Story:** As a **planner**, I want to import orders instead of typing them, so that data entry errors drop.
- **Points:** 8 | **Sprint:** Phase 2+ (unscheduled) | **Tags:** `phase-2;integration`
- **Notes:** No assumption is made about any real ERP.
- **Acceptance criteria:**

```gherkin
Scenario: Import
  Given a CSV in the documented format
  When I upload it
  Then valid rows become orders and invalid rows are listed with errors
```

##### PT-065: Shift calendar and capacity view

- **Story:** As a **planner**, I want shift calendars per station and a capacity load chart, so that I can plan realistically and OEE uses real planned time.
- **Points:** 8 | **Sprint:** Phase 2+ (unscheduled) | **Tags:** `phase-2;planning`
- **Acceptance criteria:**

```gherkin
Scenario: Load chart
  Given shifts and released work
  When I open capacity
  Then I see load vs capacity per station per day
```

##### PT-066: Email/Teams notifications for holds and late jobs (needs paid hosting or external mail service)

- **Story:** As a **supervisor**, I want notifications when a work order goes on hold or becomes late, so that I react quickly.
- **Points:** 5 | **Sprint:** Phase 2+ (unscheduled) | **Tags:** `phase-2;notifications`
- **Acceptance criteria:**

```gherkin
Scenario: Notify
  Given a work order goes on hold
  When the event is raised
  Then subscribed supervisors get a notification with a link
```

##### PT-069: Microsoft Entra ID sign-in as an alternative auth mode

- **Story:** As an **IT administrator**, I want users to sign in with Microsoft Entra ID and get roles from app roles when Auth:Mode=Entra, so that a Microsoft-centric plant can manage access centrally.
- **Points:** 5 | **Sprint:** Phase 2+ (unscheduled) | **Tags:** `phase-2;security;auth;entra`
- **Notes:** Needs an Entra tenant that allows app registrations; whether Deo's Azure DevOps/Microsoft account directory allows this without a subscription is an open question in docs/00.
- **Acceptance criteria:**

```gherkin
Scenario: Entra mode
  Given an app registration with app roles in an Entra tenant
  When Auth:Mode=Entra and I sign in
  Then my app role maps to the same policies as Identity roles and the rest of the app is unchanged
```

##### PT-073: Migrate or mirror to Azure DevOps (Boards, Repos, Pipelines on a self-hosted agent)

- **Story:** As a **developer**, I want the project mirrored or moved to Azure DevOps: backlog imported from backlog.csv, repo mirrored from GitHub, YAML pipelines running on a self-hosted agent and deploying to MonsterASP, optional Test Plans trial, so that I practise Azure DevOps hands-on for interviews while GitHub stays the day-to-day tool.
- **Points:** 8 | **Sprint:** Phase 2+ (unscheduled) | **Tags:** `phase-2;azure-devops;devops`
- **Notes:** Full plan in docs/04 (Phase 2) and labs in docs/10.
- **Acceptance criteria:**

```gherkin
Scenario: Boards
  Given an Azure DevOps project with the Scrum process and iterations Sprint 0-8
  When I import backlog.csv
  Then epics, features and PBIs appear with parent links and Effort

Scenario: Pipelines
  Given a self-hosted agent in the Default pool (the Microsoft-hosted free grant needs an Azure subscription with billing)
  When I run azure-pipelines.yml
  Then Build_Test and Deploy_Prod (with environment approval) succeed against the same MonsterASP site

Scenario: Single deployer
  Given both GitHub Actions and Azure Pipelines can deploy
  When I switch
  Then only one system holds the deploy secrets at a time, to avoid conflicting deployments
```

## 6. Backlog status tracking

Status lives in the GitHub Projects board once imported. Until then, track status here:

| Key | Status | PR / Notes |
|---|---|---|
| PT-001 | In progress | Repo created and docs/scaffold pushed (feature/sprint-0-1). Projects board and issue import (`scripts/create-github-issues.sh`) are manual steps for Deo |
| PT-002 | Done | `.github/workflows/ci.yml`: `build-test` job (restore, format check, build -warnaserror, unit + integration tests with coverage, vulnerable-package check; publish artifact + idempotent migration script on main) and `e2e` job (Playwright/Chromium); no secrets needed |
| PT-003 | Done | Solution `ProdTrack.slnx` with Domain/Application/Infrastructure/Contracts/ApiClient/UI.Shared/ShopFloor/Server + test projects; architecture tests enforce layering |
| PT-004 | Done | Serilog compact JSON rolling file (`App_Data/logs`), request logging, correlation id, ProblemDetails with traceId, `/health`, `/health/live`, `/health/ready` |
| PT-005 | Done | Global exception handler, ProblemDetails for API, friendly error page for UI, validation errors (FluentValidation) |
| PT-006 | Done (Docker untested) | LocalDB default, `docker-compose.yml` + `.env.example`, local dotnet-ef tool manifest, launch profiles, win-x86 framework-dependent publish verified. Memory check pending |
| PT-055 | Not started | Manual: MonsterASP account/site/DB (Deo) |
| PT-059 | Partly done | `.githooks/pre-push`, CODEOWNERS, PR template, dependabot committed; repo settings/secrets are manual |
| PT-007 | Not started | `deploy.yml` committed from scaffold (manual dispatch); needs MonsterASP secrets, not run |
| PT-009 | Done | ASP.NET Core Identity, admin-created users only, bootstrap admin from config, forced password change, lockout (5 attempts / 15 min), secure `__Host-` cookie, no email |
| PT-010 | Done | Roles Admin/Planner/Supervisor/Operator/QcInspector/Viewer, policies, role-based nav, 401/403 ProblemDetails for API |
| PT-013 | Done | Products (pipe marker, valve tag, safety sign, label) with type-specific fields, CRUD API + Blazor pages |
| PT-015 | Done | Stations CRUD (Admin), seeded default stations |
| PT-016 | Done | Reference data: ASME A13.1 colour schemes, ANSI Z535 signal words, materials (seeded, read API) |
| PT-017 | Done | Reason codes (scrap/hold/downtime/rework) CRUD |
| PT-047 | Done | Audit entries written in the same transaction as the change (`SaveChanges` interceptor-style, ADR-0011); viewer is PT-046 |
| PT-014 | Done | Routings per product type with versioning; routing editor (`/routings/{type}/edit`) saves a new version (add/remove/reorder steps, station, setup and standard minutes, overlap flag); released work orders keep their snapshot |
| PT-018 | Done | Sales orders with lines (CRUD, cancel), API `/api/v1/sales-orders` with ETag/If-Match, Blazor list/new/detail/edit pages |
| PT-019 | Done | Work orders from a product or generated from sales order lines (`POST /sales-orders/{id}/work-orders`, one Draft WO per line, lines that already have a WO are skipped); WO links back to its SO line |
| PT-020 | Done | Release copies routing operations to the work order (requires approved artwork where needed) |
| PT-023 | Done | Artwork upload/download via local file storage abstraction (`IFileStorage`, `App_Data/files`), size/type checks |
| PT-024 | Done | Artwork approve/reject with comment; release blocked until approved |
| PT-041 | Done | Dashboard KPI tiles (WIP, late, completed/good/scrap/scrap rate today) + WIP by station, refreshed live from SignalR `OperationChanged` events (debounced); 30 s polling removed |
| PT-021 | Done | Planner work order list with status/text filters and paging; work order detail refreshes live via SignalR |
| PT-029 | Done | `tests/ProdTrack.E2E.Tests` (Playwright for .NET, Chromium): in-process Kestrel host + SQLite in-memory; sign-in with forced password change, create + release a work order, run an operation on /floor, scan box. Runs locally (`dotnet test`) and in the CI `e2e` job; browsers installed user-locally |
| PT-011 | Partly done | Shop-floor PWA uses the same Identity cookie sign-in (back-office sign-in page); badge/PIN sign-in deferred |
| PT-012 | Done | Admin user administration (`/admin/users`): create, edit (name, employee no., badge, roles), disable/enable, reset password (forces change at next sign-in); API `/api/v1/users` with If-Match on the concurrency stamp |
| PT-022 | Done | Hold (Hold reason code + note), resume (back to the status before the hold), cancel (reason) for work orders; audit-logged, If-Match honoured; held/cancelled WOs block floor execution |
| PT-025 | Done | Printable traveler page (`/work-orders/{id}/traveler`) with QR codes (QRCoder, MIT, SVG) for the work order and each operation; `GET /work-orders/{id}/traveler`. Browser print/Save as PDF instead of server-side PDF |
| PT-026 | Not started | Reprint audit deferred |
| PT-027 | Done | /floor PWA shell with station picker (remembered in localStorage) and station queue |
| PT-028 | Done | Scan box: camera via `BarcodeDetector` where supported, keyboard-wedge scanners and manual entry fallback; `GET /api/v1/scan/{code}` resolves WO or `OP:{wo}:{seq}` codes |
| PT-030 | Done | Station queue (`GET /api/v1/stations/{code}/queue`), Ready/InProgress/Paused operations of released work orders |
| PT-031 | Done | Start operation (from queue or scan), records operator and start time |
| PT-032 | Done | Pause with reason code (Pause category) and resume |
| PT-033 | Done | Complete with good quantity; next step becomes Ready with the good quantity; last step completes the work order |
| PT-034 | Done | Scrap logging with Scrap reason codes and quantity; scrap KPIs on the dashboard |
| PT-035 | Done | QC checklist templates per product type (seeded, read-only page `/qc/templates`); template editor deferred |
| PT-036 | Done | Record QC inspection (pass/fail and measured items with tolerances) at inspection steps; completion requires a passed inspection |
| PT-037 | Partly done | Failed inspection puts the work order on hold with reason QC-FAIL; rework routing deferred |
| PT-072 | Partly done | /floor SignalR client reconnects automatically; cold-start banner deferred |
| — | Done | Optimistic concurrency: rowversion on aggregates, ETag on GET, `If-Match` on PUT/state changes; stale ETag -> 412 `Concurrency.StaleVersion`, lost race -> 409 `Concurrency.Conflict`; edit pages show a "reload latest" prompt |
| — | Done | Local-first database: LocalDB default, migration `Sprint23Execution`, `Database:MigrateOnStartup` applies migrations + seeds on startup in Development |
| PT-038..PT-040, PT-042..PT-046, PT-048, PT-049 | Not started | Materials/inventory, late-jobs list, throughput charts, timeline, downtime, OEE-lite, audit viewer, low-stock alert |

Add rows as work begins. When a story is finished, close the issue (Done on the board) and update this table in the same PR.

## 7. Change log

| Date | Change |
|---|---|
| 2026-09-26 | Initial backlog (v0.1) generated for the practice project. |
| 2026-09-26 | v0.2: GCP primary. PT-004/006/007/008/009-012/055 rewritten for Cloud Run, Terraform, ASP.NET Core Identity; PT-055 moved to Sprint 0; new PT-067 (dev custom domain), PT-068 (Google sign-in), PT-070 (Test Plans), PT-071 (Artifacts, optional), PT-069 (Entra, Phase 2); PT-059 (WIF) moved to Sprint 5; Sprint 8 now the Azure alternative. |
| 2026-09-26 | v0.3: hosting moved to the free MonsterASP.NET plan. Rewritten: PT-003 (no cloud adapter projects), PT-004 (Serilog rolling file), PT-006 (local env + MonsterASP publish config), PT-007 (CD via FTP/lftp + migrations + approval), PT-008 (Web Deploy variant + rollback), PT-009 (bootstrap password from server config), PT-023 (local file storage), PT-029/PT-050 (E2E against the app on the agent), PT-051, PT-056, PT-068. Repurposed IDs: PT-055 (GCP budget -> MonsterASP account/site/DB setup), PT-059 (WIF -> self-hosted agent, moved to Sprint 0), PT-067 (dev custom domain -> ops runbook: HTTPS renewal/backup export). New PT-072 (reconnect/cold-start UX, Sprint 5). E12 is now optional Azure/GCP targets. No IDs deleted; MVP stays 195 points. |
| 2026-09-26 | v0.3 (GitHub primary): PT-001 (GitHub repo, Projects, issue import script), PT-002 (CI workflow), PT-007 (deploy workflow, Web Deploy from windows-latest), PT-008 (FTP alternative from ubuntu + rollback), PT-029/PT-050/PT-054 (Actions), PT-059 repurposed again (self-hosted agent -> repository guardrails on GitHub Free), PT-067 (scheduled ops workflow, encrypted bacpac artifact), PT-070 (Azure Test Plans -> GitHub test-run checklist), PT-071 (Azure Artifacts -> GitHub Packages); new Phase 2 PT-073 (migrate/mirror to Azure DevOps). New outputs: backlog-github-issues.csv and repo-scaffold/scripts/create-github-issues.sh. |
| 2026-09-27 | Status table filled for the Sprint 0-1 implementation (branch `feature/sprint-0-1`). |
| 2026-09-27 | Status updated for the Sprint 2-3 batch (branch `feature/sprint-2-3`, PR #2): sales orders, user admin, hold/cancel, concurrency, routing editor, traveler + QR, floor scanning and execution, QC, scrap, live dashboard, Playwright E2E. |
| 2026-09-28 | Rebrand: project renamed to DMB ProdTrack by DMB Websolutions; planned host `dmb-prodtrack.runasp.net`; GitHub repo renamed to `deo-bernal/dmb-prodtrack` and history rewritten to drop the previous name. |
