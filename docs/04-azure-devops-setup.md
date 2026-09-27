# 04 - Azure DevOps Setup (Phase 2: migrate or mirror from GitHub)

> **Status:** Draft v0.3 (2026-09-26) - practice project. **Phase 2 document.**
> **Current tooling is GitHub** (repo `deo-bernal/dmb-prodtrack`, Projects, Actions - see [`11-github-setup.md`](11-github-setup.md)). This document is the **Phase 2 plan (story PT-073)** to migrate or mirror the project to Azure DevOps for hands-on practice and interviews. Story references below (PT-007, PT-008, PT-067 ...) mean "the Azure DevOps equivalent of that GitHub story". Only one system should hold the MonsterASP deploy secrets at a time.
> **Goal:** step-by-step setup of Azure DevOps (the full ALM tool and a key learning goal) for DMB ProdTrack: Scrum Boards, Git repo with branch policies, variable groups and secure files, environments with approvals and checks, multi-stage YAML pipelines on a **self-hosted agent** that deploy to the **free MonsterASP.NET site** (default `deployTarget: monsterasp`; Azure/GCP only as optional alternatives), plus Test Plans, Artifacts, Wiki and dashboards. Interview refresher: `10-azure-devops-study-guide.md`.
> **Names used below:** organization `https://dev.azure.com/<your-org>`, project `DMB-ProdTrack`, repo `prodtrack`. Replace `<...>` placeholders. Portal menu names can shift slightly between Azure DevOps updates.

---

## 0. Prerequisites checklist

- [ ] Azure DevOps organization (Deo already has one). No Azure subscription is needed or linked.
- [ ] MonsterASP free account, site, database, HTTPS and remote DB access set up (`08` section 3, PT-055). Planned URL `https://dmb-prodtrack.runasp.net`.
- [ ] A machine for the **self-hosted agent** (section 1.2, PT-073): the assistant's Ubuntu box (default) or Deo's Windows PC.
- [ ] Git, .NET 10 SDK, and SQL Server LocalDB/Express or Docker Desktop on the dev PC (`08` section 2).
- [ ] Azure CLI with the `azure-devops` extension (`az extension add --name azure-devops`) - optional, for the CLI alternatives shown. (Azure DevOps CLI commands do not need an Azure subscription.)
- [ ] Not needed by default: `gcloud`, Terraform, Bicep, an Azure subscription (only for the optional alternatives in `03` Appendix A).

## 1. Organization and project

1. **Organization settings > Billing:** no Azure subscription is linked, so everything stays on free amounts (5 Basic users, 1 self-hosted parallel job, 2 GiB Artifacts). Paid features (Test Plans after its trial, Microsoft-hosted jobs) would require a linked subscription.
2. **Organization settings > Pipelines > Parallel jobs:** *Self-hosted* shows **1 free job, unlimited minutes** - this is what ProdTrack uses. *Microsoft-hosted* free grant now has to be enabled by linking an Azure subscription / billing (Microsoft Learn "Configure and pay for parallel jobs"), so it is **not used**. All YAML uses `pool: { name: Default }`.
3. **New project:** name `DMB-ProdTrack`, visibility **Private**, version control **Git**, work item process **Scrum**.
   - Why Scrum: it matches the JD ("Scrum") and gives Product Backlog Items, Effort, Sprint burndown and the Impediment work item type. Note that Scrum calls the estimate field **Effort** (not *Story Points*, which is the Agile process field).
4. **Project settings > Overview:** keep Boards, Repos, Pipelines, Test Plans (optional 30-day trial) and Artifacts enabled.
5. **Organization settings > Pipelines > Settings:** new organizations have *Disable creation of classic build pipelines* and *... classic release pipelines* **on** by default - keep them on; this project is YAML-only. Also keep *Limit job authorization scope to current project* and *Protect access to repositories in YAML pipelines* on.

CLI alternative:

```bash
az devops configure --defaults organization=https://dev.azure.com/<your-org>
az devops project create --name "DMB-ProdTrack" --process Scrum --source-control git --visibility private
az devops configure --defaults project="DMB-ProdTrack"
```

### 1.2 Self-hosted agent (PT-073)

One self-hosted agent in the **Default** pool runs every pipeline. Choose one location (open question in `00`):

| | **Linux agent on the assistant's Ubuntu box (default)** | Windows agent on Deo's PC |
|---|---|---|
| Deploy method | FTP with `lftp` (`deployMethod=ftp`) | Web Deploy `msdeploy` (`deployMethod=msdeploy`) or FTP via WinSCP |
| Tools to install | .NET 10 SDK, `lftp`, `sqlcmd` (go-sqlcmd or mssql-tools18), `sqlpackage`, `openssl`, `curl`, optional Docker + Playwright browser deps | .NET 10 SDK, Web Deploy 4 (msdeploy), PowerShell `SqlServer` module (`Invoke-Sqlcmd`), `sqlpackage`, optional Docker Desktop |
| Availability | While the box runs | While the PC is on |

**Register the agent (Linux):**

```bash
# 1. PAT: User settings > Personal access tokens > New: scope "Agent Pools (Read & manage)", expiry 1-7 days.
# 2. Dedicated non-admin user
sudo useradd -m -s /bin/bash azagent
sudo -iu azagent
mkdir -p ~/agent && cd ~/agent
# 3. Download URL: Organization settings > Agent pools > Default > New agent > Linux > copy the link
curl -fsSLo agent.tar.gz "<agent-download-url-for-linux-x64>"
tar zxf agent.tar.gz
./config.sh --unattended --url https://dev.azure.com/<your-org> --auth pat --token "<PAT>" \
  --pool Default --agent box-linux-01 --work _work --acceptTeeEula
exit
cd /home/azagent/agent && sudo ./svc.sh install azagent && sudo ./svc.sh start
# 4. Tools (Ubuntu)
sudo apt-get update && sudo apt-get install -y lftp curl openssl jq
#    .NET 10 SDK: Microsoft package feed or dotnet-install.sh (see Microsoft Learn "Install .NET on Ubuntu")
#    sqlcmd: go-sqlcmd package from packages.microsoft.com; sqlpackage: dotnet tool install -g microsoft.sqlpackage
# 5. Capabilities: add to /home/azagent/agent/.env (one per line), then restart the service:
#    lftp=/usr/bin/lftp
#    sqlcmd=/usr/bin/sqlcmd
```

**Register the agent (Windows, PowerShell as admin):** download the Windows x64 agent zip from the same page, extract to `C:\agents\a1`, run `.\config.cmd --unattended --url https://dev.azure.com/<your-org> --auth pat --token <PAT> --pool Default --agent pc-win-01 --runAsService --windowsLogonAccount "NT AUTHORITY\NETWORK SERVICE"`, then install the .NET 10 SDK, **Web Deploy** (msdeploy lands under `C:\Program Files\IIS\Microsoft Web Deploy V3\`) and `Install-Module SqlServer -Scope AllUsers`.

**Hygiene:** revoke the registration PAT after setup (the agent uses its own OAuth token afterwards); keep the project private and never build forks on this agent; run as a non-admin account; enable workspace clean (`workspace: clean: all`). Adding the agent user to the `docker` group grants root-equivalent access on that machine - do it only if you want Testcontainers integration tests and E2E on the agent; otherwise set `agentHasDocker=false` and those steps are skipped. Keep the agent updated (Agent pools > Default > Update all agents) - the Node 6/10/16 task handlers are removed from the agent in 2026 (`10` section 1).

## 2. Boards configuration

1. **Project settings > Team configuration > General:**
   - Backlog navigation levels: Epics, Features, Backlog items (all on).
   - Working days: Mon-Sat if you practise on Saturdays; else Mon-Fri.
   - Bugs: *Bugs are managed with requirements* (bugs appear on the backlog with PBIs).
2. **Board columns** (Boards > Board > settings gear > Columns) for Backlog items:

   | Column | State mapping | WIP limit | Definition of done (column) |
   |---|---|---|---|
   | New | New | - | - |
   | Ready | Approved | 5 | Meets Definition of Ready (`05` section 1) |
   | In Progress | Committed | 2 | Branch created, tasks listed |
   | In Review | Committed | 2 | PR open, CI green |
   | Done | Done | - | DoD met, deployed to prod (MonsterASP) after approval |

   Enable **split columns** (Doing/Done) for In Progress if you like.
3. **Card styles:** red card rule for tag `blocked`; tag colours for `security`, `devops`, `phase-2`.
4. **Swimlanes:** add `Expedite` for production bugs.
5. **Dashboards:** create "ProdTrack Sprint" dashboard with Sprint Burndown, Velocity, Cumulative Flow, Build History (pipeline), Deployment status, Test results trend and a query tile "MVP not done" (details in section 13.4).
6. **Markdown in work items:** large text fields (Description, Acceptance Criteria) can be switched to the Markdown editor per field (GA 2025); once a field is saved as Markdown it stays Markdown. The CSV import uses HTML, which is fine.

## 3. Area and iteration paths

1. **Project settings > Boards > Project configuration > Areas:** under `DMB-ProdTrack` add `Platform`, `MasterData`, `Orders`, `ShopFloor`, `Quality`, `Inventory`, `Dashboard`, `Security`, `Docs`. (The CSV imports everything at the root area; move items to sub-areas while refining if you want.)
2. **Iterations:** create under `DMB-ProdTrack`:

   | Iteration | Start | End |
   |---|---|---|
   | Sprint 0 | 2026-10-05 | 2026-10-16 |
   | Sprint 1 | 2026-10-19 | 2026-10-30 |
   | Sprint 2 | 2026-11-02 | 2026-11-13 |
   | Sprint 3 | 2026-11-16 | 2026-11-27 |
   | Sprint 4 | 2026-11-30 | 2026-12-11 |
   | Sprint 5 | 2027-01-04 | 2027-01-15 |
   | Sprint 6 | 2027-01-18 | 2027-01-29 |
   | Sprint 7 | 2027-02-01 | 2027-02-12 |
   | Sprint 8 (optional Azure alternative) | 2027-02-15 | 2027-02-26 |

   Name the last one exactly `Sprint 8` (the CSV uses `DMB-ProdTrack\Sprint 8`).
3. **Team configuration > Iterations:** select all sprints for the default team, so they show under Boards > Sprints. **Capacity** per sprint: set your hours/day (e.g. 2-3 h) to see realistic capacity bars.

CLI alternative (repeat per sprint):

```bash
az boards iteration project create --name "Sprint 0" --path "\\DMB-ProdTrack\\Iteration" \
  --start-date 2026-10-05 --finish-date 2026-10-16
az boards iteration team add --id <iteration-id> --team "DMB-ProdTrack Team"
```

## 4. Import the backlog

1. **Boards > Queries > Import work items** (or *Boards > Work items > Import work items*), choose `docs/backlog.csv` (Scrum). For an Agile-process project use `docs/backlog-agile-process.csv`.
2. Review the preview (items appear bold = unsaved), then **Save items**.
3. Verify: **Boards > Backlogs >** switch level to **Epics**, expand the tree (Epic > Feature > PBI). **Sprints** view should show PBIs per sprint.
4. Create a shared query "MVP not done": `Tags Contains mvp AND State <> Done`.

## 5. Repository

1. **Repos > Files:** the default repo is named after the project; rename it to `prodtrack` (*Project settings > Repositories*), or create a new one and delete the empty default.
2. Phase 2: either connect pipelines to the GitHub repo (section 12, option A) or fill this repo with the one-way mirror (option B). Do not develop in two places.

3. Add `.gitignore` (`dotnet new gitignore`), `.gitattributes` (`* text=auto`), `.editorconfig`, `global.json` in Sprint 0 (PT-003).
4. **Pull request template:** create `.azuredevops/pull_request_template.md`:

   ```markdown
   ## What / why
   PT-xxx: <summary>. AB#<work item id>

   ## How tested
   - [ ] Unit tests  - [ ] Integration tests  - [ ] E2E (if UI flow)  - [ ] Manually on Local (and on prod after deploy)

   ## Checklist
   - [ ] Acceptance criteria met (Given/When/Then)
   - [ ] Authorization + audit applied to new write paths
   - [ ] No secrets, no cloud SDK types outside Infrastructure.*
   - [ ] Docs updated (02 API list / 07 user guide / 05 status / ADR)
   - [ ] Migration added and reviewed (if schema changed)
   ```

5. **Branching model:** trunk-based with short-lived branches: `feature/PT-031-start-operation`, `fix/PT-031-null-station`, `chore/...`. Squash merge to `main`. Release tags `v1.0.0` (SemVer).

## 6. Branch policies and PR rules

**Repos > Branches > main > ... > Branch policies:**

| Policy | Setting | Why |
|---|---|---|
| Require a minimum number of reviewers | 1; **Allow requestors to approve their own changes = On** (solo practice); "Reset code reviewer votes when there are new changes" On | Real teams: 1-2 reviewers, no self-approval |
| Check for linked work items | **Required** | Traceability Boards <-> code |
| Check for comment resolution | Required | No unresolved review threads |
| Limit merge types | **Squash merge only** | Linear history |
| Build validation | Pipeline `prodtrack-ci` (azure-pipelines.yml), trigger automatic, **Required**, expiry 12 h | Nothing merges without green CI |
| Automatically included reviewers | (team) optional; for solo, skip | |

Also: **Repos > Settings:** disable *forks* if not needed; in *Security* deny "Force push" and "Bypass policies" for Contributors on `main`. Protect release tags `v*` (tag policies are managed via *Security* for refs).

Commit message convention: `PT-031: Start operation by scan (AB#123)` - `AB#` is optional in Azure Repos since you link work items in the PR, but useful if the repo is mirrored to GitHub.

## 7. Connections and credentials

- **No cloud service connections are needed** for the default target. MonsterASP has no API or OIDC federation; the pipeline authenticates with the **FTP** (Linux agent) or **Web Deploy** (Windows agent) credentials shown in the MonsterASP control panel, and reaches the database with a SQL login (remote access enabled in the panel). All are stored as **secret variables** (section 8) and mapped explicitly into the steps that need them.
- These are long-lived passwords: least exposure (only the Deploy_Prod job and `prodtrack-ops` can read the variable group), rotate on exposure or every 6 months (`08` section 11), never echo them, and pass them through environment variables or here-documents rather than command-line arguments where possible.
- **GitHub** (section 12): option A uses the *Azure Pipelines* GitHub App (GitHub service connection created by the wizard, scoped to `deo-bernal/dmb-prodtrack`); option B needs no Azure DevOps-side credential (GitHub pushes with `AZDO_MIRROR_PAT`).
- **Alternative targets only** (`03` Appendix A): Azure would use an Azure Resource Manager service connection with workload identity federation (needs a subscription); Google Cloud would use Workload Identity Federation with the pipeline OIDC token or a service-account key. Not configured in the MVP.

## 8. Variable groups and secrets

**Pipelines > Library > + Variable group:**

| Group | Variable | Secret? | Example / note | Used by |
|---|---|---|---|---|
| `prodtrack-common` | `buildConfiguration` | no | `Release` | all |
| | `dotnetRid` | no | `win-x86` (framework-dependent publish for MonsterASP) | Build_Test |
| | `agentHasDocker` | no | `true` on the Linux box with Docker, else `false` (skips Testcontainers/E2E) | Build_Test |
| `prodtrack-monsterasp` | `APP_URL` | no | `https://dmb-prodtrack.runasp.net` | Deploy_Prod, ops |
| | `MONSTERASP_FTP_HOST` | **yes** | `siteXXXX.siteasp.net` (FTP; `sftp://siteXXXX.siteasp.net` if you use SFTP) | Deploy_Prod (ftp), ops |
| | `MONSTERASP_FTP_USER` | **yes** | from control panel | Deploy_Prod (ftp), ops |
| | `MONSTERASP_FTP_PASSWORD` | **yes** | from control panel | Deploy_Prod (ftp), ops |
| | `MONSTERASP_FTP_ROOT` | no | site root folder after login, e.g. `/wwwroot` **(verify after first FTP login)** | Deploy_Prod (ftp), ops |
| | `PROD_DB_CONNECTION` | **yes** | `Server=<db-server>;Database=<db>;User Id=<login>;Password=<pwd>;Encrypt=True;TrustServerCertificate=True` (TLS options per MonsterASP "Secure connection to MSSQL" article, verify) | Deploy_Prod, ops |
| | `MONSTERASP_SITE` | no | `siteXXXX` | Deploy_Prod (msdeploy) |
| | `MONSTERASP_WEBDEPLOY_PASSWORD` | **yes** | Web Deploy password from the panel | Deploy_Prod (msdeploy) |

- Secret variables are **not** exposed to scripts automatically: map them with `env:` (as in the templates below). They are masked in logs.
- On `prodtrack-monsterasp`: *Pipeline permissions* -> only `prodtrack-ci` and `prodtrack-ops`; add an **Approvals and checks** entry (e.g. Branch control `refs/heads/main`) for practice.
- **Variable groups vs Key Vault (interview topic):** a variable group can hold its own secret variables or be **linked to an Azure Key Vault** (needs a subscription and service connection). Here the secrets live in the variable group; application secrets on the host live in the server-only `appsettings.Production.json` (`08` section 3), which never passes through the pipeline.

## 9. Environments, approvals and checks

**Pipelines > Environments > New environment** (resource: None):

| Environment | Approvals and checks |
|---|---|
| `prodtrack-prod` | **Approvals:** Deo (self-approval allowed for solo practice; real teams: a different approver), timeout 72 h, instructions "CI green, release notes ready, bacpac taken if the migration is risky". **Branch control:** only `refs/heads/main` and `refs/tags/v*`. **Exclusive lock** (no parallel deployments). Optional **Business hours** check (Asia/Manila) |

Checks can also be put on **variable groups, secure files, agent pools and service connections** (protected resources) - practise one on `prodtrack-monsterasp`. Deployment history per environment gives traceability work items -> commits -> deployments. There is no dev environment: every approved merge goes to the single MonsterASP site.

## 10. Pipelines

Create pipelines from YAML: **Pipelines > New pipeline > Azure Repos Git > prodtrack > Existing Azure Pipelines YAML file**:

| Pipeline name | File | Trigger |
|---|---|---|
| `prodtrack-ci` | `/azure-pipelines.yml` | PRs to main (via build validation policy) and merges to main (CI + CD with approval) |
| `prodtrack-ops` | `/pipelines/prodtrack-ops.yml` | Weekly schedule (certificate expiry, DB size, bacpac + files backup) |

```mermaid
flowchart LR
    PR[PR or merge to main] --> BT[Stage Build_Test<br/>self-hosted agent<br/>restore, format, build, unit tests<br/>integration + E2E if Docker<br/>publish win-x86, migrations.sql]
    BT -->|main only| AP{Environment<br/>prodtrack-prod<br/>approval}
    AP --> DP[Stage Deploy_Prod<br/>app_offline.htm, sqlcmd migrations<br/>lftp mirror or msdeploy<br/>remove app_offline, smoke with retries]
    DP --> SITE[dmb-prodtrack.runasp.net]
```

### 10.1 Main pipeline `azure-pipelines.yml`

```yaml
# azure-pipelines.yml - DMB ProdTrack CI/CD (self-hosted agent, MonsterASP free plan)
# Build + test on every PR (build validation policy); on main: approval, then deploy to prod.
name: $(Date:yyyyMMdd)$(Rev:.r)

trigger:
  branches: { include: [ main ] }
  paths: { exclude: [ docs/*, '*.md' ] }
# For Azure Repos Git the YAML 'pr:' trigger is ignored - PR builds come from the
# build validation branch policy (section 6). Kept for the optional GitHub mirror.
pr:
  branches: { include: [ main ] }

parameters:
  - name: deployTarget
    displayName: Deploy target
    type: string
    default: monsterasp
    values: [ monsterasp, azure, gcp ]   # azure/gcp: optional alternatives (03 Appendix A)
  - name: deployMethod
    displayName: MonsterASP deploy method
    type: string
    default: ftp
    values: [ ftp, msdeploy ]            # ftp = Linux agent + lftp; msdeploy = Windows agent
  - name: runIntegrationTests
    type: boolean
    default: true
  - name: runE2E
    type: boolean
    default: true

variables:
  - group: prodtrack-common

stages:
  - stage: Build_Test
    displayName: Build and test
    jobs:
      - template: pipelines/templates/build.yml
        parameters:
          runIntegrationTests: ${{ parameters.runIntegrationTests }}
          runE2E: ${{ parameters.runE2E }}

  - ${{ if eq(parameters.deployTarget, 'monsterasp') }}:
    - stage: Deploy_Prod
      displayName: Deploy to MonsterASP (prod)
      dependsOn: Build_Test
      condition: and(succeeded(), eq(variables['Build.SourceBranch'], 'refs/heads/main'), ne(variables['Build.Reason'], 'PullRequest'))
      variables:
        - group: prodtrack-monsterasp
      jobs:
        - ${{ if eq(parameters.deployMethod, 'ftp') }}:
          - template: pipelines/templates/deploy-monsterasp-ftp.yml
        - ${{ else }}:
          - template: pipelines/templates/deploy-monsterasp-msdeploy.yml

  - ${{ else }}:
    # Optional alternatives: the template exists only after PT-058 activates the target.
    - template: pipelines/templates/deploy-${{ parameters.deployTarget }}.yml
```

### 10.2 Template `pipelines/templates/build.yml`

```yaml
parameters:
  - name: runIntegrationTests
    type: boolean
    default: true
  - name: runE2E
    type: boolean
    default: true

jobs:
  - job: Build
    displayName: Build, test, publish
    pool: { name: Default }
    timeoutInMinutes: 60
    workspace: { clean: all }
    steps:
      - checkout: self
        fetchDepth: 0

      - task: UseDotNet@2
        displayName: .NET SDK from global.json
        inputs: { packageType: sdk, useGlobalJson: true }

      - script: dotnet tool restore
        displayName: Restore local tools (dotnet-ef, reportgenerator)

      - script: dotnet restore ProdTrack.slnx --locked-mode
        displayName: Restore

      - script: dotnet format ProdTrack.slnx --verify-no-changes --no-restore
        displayName: Format check

      - script: dotnet build ProdTrack.slnx -c $(buildConfiguration) --no-restore -warnaserror
        displayName: Build

      - script: >
          dotnet test ProdTrack.slnx -c $(buildConfiguration) --no-build
          --filter "Category!=Integration&Category!=E2E"
          --logger trx --collect:"XPlat Code Coverage"
          --results-directory $(Agent.TempDirectory)/tests
        displayName: Unit tests

      - ${{ if parameters.runIntegrationTests }}:
        - script: >
            dotnet test ProdTrack.slnx -c $(buildConfiguration) --no-build
            --filter "Category=Integration"
            --logger trx --collect:"XPlat Code Coverage"
            --results-directory $(Agent.TempDirectory)/tests
          displayName: Integration tests (Testcontainers SQL Server)
          condition: and(succeeded(), eq(variables['agentHasDocker'], 'true'))

      - task: PublishTestResults@2
        condition: succeededOrFailed()
        inputs:
          testResultsFormat: VSTest
          testResultsFiles: '$(Agent.TempDirectory)/tests/**/*.trx'
          failTaskOnFailedTests: true

      - task: PublishCodeCoverageResults@2
        condition: succeededOrFailed()
        inputs:
          summaryFileLocation: '$(Agent.TempDirectory)/tests/**/coverage.cobertura.xml'

      # Framework-dependent, 32-bit Windows build for the MonsterASP IIS site (works when built on Linux too).
      - script: >
          dotnet publish src/ProdTrack.Server/ProdTrack.Server.csproj -c $(buildConfiguration)
          -r $(dotnetRid) --self-contained false
          -o $(Build.ArtifactStagingDirectory)/app
        displayName: Publish app ($(dotnetRid), framework-dependent)

      - script: >
          dotnet ef migrations script --idempotent
          --project src/ProdTrack.Infrastructure --startup-project src/ProdTrack.Server
          --configuration $(buildConfiguration)
          --output $(Build.ArtifactStagingDirectory)/db/migrations.sql
        displayName: Generate idempotent migration script

      - task: CopyFiles@2
        displayName: Copy deploy helpers (app_offline.htm)
        inputs:
          SourceFolder: deploy
          Contents: app_offline.htm
          TargetFolder: $(Build.ArtifactStagingDirectory)/deploy

      - publish: $(Build.ArtifactStagingDirectory)
        artifact: drop
        displayName: Publish pipeline artifact

  - ${{ if parameters.runE2E }}:
    - job: E2E
      displayName: Playwright E2E against the app on the agent
      dependsOn: Build
      condition: and(succeeded(), eq(variables['agentHasDocker'], 'true'))
      pool:
        name: Default
        demands: [ Agent.OS -equals Linux ]
      timeoutInMinutes: 30
      workspace: { clean: all }
      steps:
        - checkout: self
        - task: UseDotNet@2
          inputs: { packageType: sdk, useGlobalJson: true }
        - bash: |
            set -euo pipefail
            export SA_PWD="E2e-$(Build.BuildId)-$RANDOM!aA"
            docker run -d --name prodtrack-e2e-sql -e ACCEPT_EULA=Y -e "MSSQL_SA_PASSWORD=$SA_PWD" \
              -p 14333:1433 mcr.microsoft.com/mssql/server:2022-latest
            export ConnectionStrings__ProdTrack="Server=localhost,14333;Database=ProdTrackE2E;User Id=sa;Password=$SA_PWD;TrustServerCertificate=True"
            export ASPNETCORE_ENVIRONMENT=E2E Database__MigrateOnStartup=true Auth__Mode=Test
            dotnet build tests/ProdTrack.E2E.Tests -c Release
            pwsh tests/ProdTrack.E2E.Tests/bin/Release/net10.0/playwright.ps1 install chromium || true
            sleep 20   # SQL Server container start
            nohup dotnet run --project src/ProdTrack.Server -c Release --urls http://localhost:5080 > server.log 2>&1 &
            for i in $(seq 1 30); do curl -fs http://localhost:5080/health/ready && break; sleep 3; done
            E2E__BaseUrl=http://localhost:5080 dotnet test tests/ProdTrack.E2E.Tests -c Release --no-build \
              --logger trx --results-directory $(Agent.TempDirectory)/e2e
          displayName: Start SQL + app, run E2E
        - bash: docker rm -f prodtrack-e2e-sql || true
          displayName: Remove SQL container
          condition: always()
        - task: PublishTestResults@2
          condition: succeededOrFailed()
          inputs:
            testResultsFormat: VSTest
            testResultsFiles: '$(Agent.TempDirectory)/e2e/**/*.trx'
            testRunTitle: E2E
```

Notes: Playwright browsers (and PowerShell for `playwright.ps1`) are installed once on the agent (`sudo apt-get install -y powershell` from the Microsoft feed, then `playwright.ps1 install --with-deps chromium`). `Auth:Mode=Test` is the header-based test scheme from `02` section 7.4 and is refused in Production.

### 10.3 Template `pipelines/templates/deploy-monsterasp-ftp.yml` (default: Linux agent + lftp)

```yaml
jobs:
  - deployment: DeployFtp
    displayName: FTP deploy to MonsterASP
    environment: prodtrack-prod
    pool:
      name: Default
      demands: [ Agent.OS -equals Linux, lftp, sqlcmd ]
    strategy:
      runOnce:
        deploy:
          steps:
            - download: current
              artifact: drop

            - bash: |
                set -euo pipefail
                lftp <<EOF
                set cmd:fail-exit yes
                set net:max-retries 3
                set net:timeout 30
                set ftp:ssl-allow yes
                open -u "$FTP_USER","$FTP_PASSWORD" "$FTP_HOST"
                cd "$FTP_ROOT"
                put "$(Pipeline.Workspace)/drop/deploy/app_offline.htm" -o app_offline.htm
                EOF
                sleep 10   # let IIS unload the app and release file locks
              displayName: Take site offline (app_offline.htm)
              env:
                FTP_HOST: $(MONSTERASP_FTP_HOST)
                FTP_USER: $(MONSTERASP_FTP_USER)
                FTP_PASSWORD: $(MONSTERASP_FTP_PASSWORD)
                FTP_ROOT: $(MONSTERASP_FTP_ROOT)

            - bash: |
                set -euo pipefail
                kv() { printf '%s' "$PROD_DB_CONNECTION" | tr ';' '\n' | grep -i -E "^[[:space:]]*($1)[[:space:]]*=" | head -1 | cut -d= -f2- ; }
                export SQLCMDPASSWORD="$(kv 'Password|Pwd')"
                sqlcmd -S "$(kv 'Server|Data Source')" -d "$(kv 'Database|Initial Catalog')" \
                  -U "$(kv 'User Id|UID|User')" -C -b \
                  -i "$(Pipeline.Workspace)/drop/db/migrations.sql"
              displayName: Apply idempotent EF Core migrations (sqlcmd)
              env:
                PROD_DB_CONNECTION: $(PROD_DB_CONNECTION)

            - bash: |
                set -euo pipefail
                lftp <<EOF
                set cmd:fail-exit yes
                set net:max-retries 3
                set net:timeout 30
                set ftp:ssl-allow yes
                open -u "$FTP_USER","$FTP_PASSWORD" "$FTP_HOST"
                cd "$FTP_ROOT"
                mirror --reverse --delete --verbose --parallel=4 \
                  --exclude-glob App_Data/ --exclude-glob logs/ \
                  --exclude-glob app_offline.htm --exclude-glob appsettings.Production.json \
                  "$(Pipeline.Workspace)/drop/app" .
                EOF
              displayName: Upload app (lftp mirror, keeps App_Data and server config)
              env:
                FTP_HOST: $(MONSTERASP_FTP_HOST)
                FTP_USER: $(MONSTERASP_FTP_USER)
                FTP_PASSWORD: $(MONSTERASP_FTP_PASSWORD)
                FTP_ROOT: $(MONSTERASP_FTP_ROOT)

            - bash: |
                set -euo pipefail
                lftp <<EOF
                set net:max-retries 3
                set ftp:ssl-allow yes
                open -u "$FTP_USER","$FTP_PASSWORD" "$FTP_HOST"
                cd "$FTP_ROOT"
                rm -f app_offline.htm
                EOF
              displayName: Bring site online (remove app_offline.htm)
              condition: always()   # never leave the site offline, even if a step failed
              env:
                FTP_HOST: $(MONSTERASP_FTP_HOST)
                FTP_USER: $(MONSTERASP_FTP_USER)
                FTP_PASSWORD: $(MONSTERASP_FTP_PASSWORD)
                FTP_ROOT: $(MONSTERASP_FTP_ROOT)

            - bash: |
                for i in $(seq 1 18); do
                  code=$(curl -s -o /dev/null -w '%{http_code}' --max-time 30 "$(APP_URL)/health/ready" || true)
                  if [ "$code" = "200" ]; then echo "Healthy after attempt $i"; exit 0; fi
                  echo "Attempt $i: HTTP $code - waiting (cold start after deploy is normal)"; sleep 10
                done
                echo "##vso[task.logissue type=error]Smoke test failed: $(APP_URL)/health/ready"; exit 1
              displayName: Smoke test with retries
```

Notes:
- `ftp:ssl-allow yes` uses explicit FTPS when the server offers it; if certificate validation fails, check the MonsterASP FTP/SFTP article before relaxing `ssl:verify-certificate` **(verify)**. For SFTP set `MONSTERASP_FTP_HOST=sftp://siteXXXX.siteasp.net`.
- `sqlcmd -C` trusts the server certificate (go-sqlcmd and mssql-tools18); the password travels in `SQLCMDPASSWORD`, not on the command line. Alternative to sqlcmd: check out the repo in this job and run `dotnet ef database update --project src/ProdTrack.Infrastructure --startup-project src/ProdTrack.Server --connection "$PROD_DB_CONNECTION"`.
- `mirror --delete` removes server files that are no longer in the build, except the excluded paths.

### 10.4 Template `pipelines/templates/deploy-monsterasp-msdeploy.yml` (Windows agent + Web Deploy, PT-008)

```yaml
jobs:
  - deployment: DeployWebDeploy
    displayName: Web Deploy to MonsterASP
    environment: prodtrack-prod
    pool:
      name: Default
      demands: [ Agent.OS -equals Windows_NT ]
    strategy:
      runOnce:
        deploy:
          steps:
            - download: current
              artifact: drop

            - powershell: |
                $ErrorActionPreference = 'Stop'
                Import-Module SqlServer
                Invoke-Sqlcmd -ConnectionString $env:PROD_DB_CONNECTION `
                  -InputFile "$(Pipeline.Workspace)\drop\db\migrations.sql" -QueryTimeout 600
              displayName: Apply idempotent EF Core migrations (Invoke-Sqlcmd)
              env:
                PROD_DB_CONNECTION: $(PROD_DB_CONNECTION)

            - powershell: |
                $ErrorActionPreference = 'Stop'
                $md = @("C:\Program Files\IIS\Microsoft Web Deploy V3\msdeploy.exe",
                        "C:\Program Files (x86)\IIS\Microsoft Web Deploy V3\msdeploy.exe") |
                      Where-Object { Test-Path $_ } | Select-Object -First 1
                if (-not $md) { throw "msdeploy.exe not found - install Web Deploy on the agent" }
                $site = "$(MONSTERASP_SITE)"
                & $md -verb:sync `
                  "-source:contentPath=$(Pipeline.Workspace)\drop\app" `
                  "-dest:contentPath=$site,computerName=https://$site.siteasp.net:8172/msdeploy.axd?site=$site,userName=$site,password=$env:WD_PASSWORD,authtype=Basic,includeAcls=False" `
                  -allowUntrusted -enableRule:AppOffline `
                  "-skip:objectName=dirPath,absolutePath=\\App_Data$" `
                  "-skip:objectName=dirPath,absolutePath=\\logs$" `
                  "-skip:objectName=filePath,absolutePath=\\appsettings\.Production\.json$" `
                  -disableLink:AppPoolExtension -disableLink:ContentExtension -disableLink:CertificateExtension
                if ($LASTEXITCODE -ne 0) { throw "msdeploy failed with exit code $LASTEXITCODE" }
              displayName: Web Deploy sync (msdeploy)
              env:
                WD_PASSWORD: $(MONSTERASP_WEBDEPLOY_PASSWORD)

            - powershell: |
                for ($i = 1; $i -le 18; $i++) {
                  try { $r = Invoke-WebRequest "$(APP_URL)/health/ready" -UseBasicParsing -TimeoutSec 30
                        if ($r.StatusCode -eq 200) { "Healthy after attempt $i"; exit 0 } } catch { }
                  "Attempt ${i}: not healthy yet (cold start is normal)"; Start-Sleep 10
                }
                Write-Host "##vso[task.logissue type=error]Smoke test failed"; exit 1
              displayName: Smoke test with retries
```

The msdeploy command line follows MonsterASP's documented example (help "How to deploy website content from command line"). `-enableRule:AppOffline` and the `-skip` rules are standard Web Deploy features but are **not** in MonsterASP's docs - verify on the first run (fallback: upload `app_offline.htm` by FTP first as in 10.3). Migrations run before the sync here because AppOffline is applied by msdeploy itself; keep migrations expand/contract so the running old code tolerates the new schema for those seconds.

### 10.5 E2E and quality gates

E2E runs in the `Build_Test` stage against the app started on the agent with a throwaway SQL Server container (10.2), so a failing E2E blocks `Deploy_Prod` (PT-050). After deployment, only the read-only smoke subset may run against the live site (`E2E__BaseUrl=$(APP_URL)`, category `Smoke`) - never tests that write data to prod.

### 10.6 Ops pipeline `pipelines/prodtrack-ops.yml` (PT-067)

```yaml
# Weekly checks for the free MonsterASP site: certificate expiry, DB size, backups.
trigger: none
pr: none
schedules:
  - cron: "0 1 * * 1"          # Monday 01:00 UTC = 09:00 PHT
    displayName: Weekly ops checks
    branches: { include: [ main ] }
    always: true

pool:
  name: Default
  demands: [ Agent.OS -equals Linux, lftp, sqlcmd ]

variables:
  - group: prodtrack-monsterasp
  - name: backupDir
    value: /home/azagent/prodtrack-backups

steps:
  - checkout: none

  - bash: |
      set -euo pipefail
      host=$(echo "$(APP_URL)" | sed -E 's#https?://([^/]+).*#\1#')
      end=$(echo | openssl s_client -connect "$host:443" -servername "$host" 2>/dev/null | openssl x509 -noout -enddate | cut -d= -f2)
      days=$(( ( $(date -d "$end" +%s) - $(date +%s) ) / 86400 ))
      echo "Certificate for $host expires $end ($days days)"
      if [ "$days" -lt 21 ]; then
        echo "##vso[task.logissue type=error]HTTPS certificate expires in $days days - renew it in the MonsterASP panel (08 section 6)"; exit 1
      fi
    displayName: Check HTTPS certificate expiry

  - bash: |
      set -euo pipefail
      kv() { printf '%s' "$PROD_DB_CONNECTION" | tr ';' '\n' | grep -i -E "^[[:space:]]*($1)[[:space:]]*=" | head -1 | cut -d= -f2- ; }
      export SQLCMDPASSWORD="$(kv 'Password|Pwd')"
      mb=$(sqlcmd -S "$(kv 'Server|Data Source')" -d "$(kv 'Database|Initial Catalog')" -U "$(kv 'User Id|UID|User')" -C -h -1 -W \
        -Q "SET NOCOUNT ON; SELECT CAST(SUM(size) * 8 / 1024.0 AS int) FROM sys.database_files WHERE type = 0")
      echo "Data file size: ${mb} MB of 1024 MB"
      if [ "$mb" -gt 700 ]; then echo "##vso[task.logissue type=warning]Database above 700 MB - purge/archive (08 section 7.4)"; fi
    displayName: Check database size
    env:
      PROD_DB_CONNECTION: $(PROD_DB_CONNECTION)

  - bash: |
      set -euo pipefail
      mkdir -p "$(backupDir)"
      f="$(backupDir)/prodtrack-$(date +%Y%m%d).bacpac"
      sqlpackage /Action:Export /SourceConnectionString:"$PROD_DB_CONNECTION" /TargetFile:"$f" /p:VerifyExtraction=true
      ls -1t "$(backupDir)"/*.bacpac | tail -n +5 | xargs -r rm --
    displayName: Export database (.bacpac, keep 4)
    env:
      PROD_DB_CONNECTION: $(PROD_DB_CONNECTION)

  - bash: |
      set -euo pipefail
      d="$(backupDir)/files-$(date +%Y%m%d)"
      lftp <<EOF
      set ftp:ssl-allow yes
      open -u "$FTP_USER","$FTP_PASSWORD" "$FTP_HOST"
      cd "$FTP_ROOT"
      mirror App_Data/files "$d"
      EOF
      ls -1dt "$(backupDir)"/files-* | tail -n +5 | xargs -r rm -rf --
    displayName: Download uploaded files (keep 4)
    env:
      FTP_HOST: $(MONSTERASP_FTP_HOST)
      FTP_USER: $(MONSTERASP_FTP_USER)
      FTP_PASSWORD: $(MONSTERASP_FTP_PASSWORD)
      FTP_ROOT: $(MONSTERASP_FTP_ROOT)
```

Backups stay on the agent machine; copy them monthly to Deo's PC. The weekly run also wakes the site once, which is fine; do not schedule it more often just to keep the site awake.

### 10.7 Optional alternative targets (Sprint 8)

`deployTarget=azure|gcp` includes `pipelines/templates/deploy-azure.yml` / `deploy-gcp.yml`, which are written only when PT-058 is done and a subscription exists (Azure: ARM service connection + Container Apps or App Service; GCP: WIF or SA key + Cloud Run). See `03` Appendix A. They are not part of the MVP and the default run never compiles them.

## 11. Troubleshooting

| Symptom | Fix |
|---|---|
| CSV import: items imported but no parent links | Each child row must be directly under its parent, the title in the next Title column (Title 1 Epic, Title 2 Feature, Title 3 PBI), ID column empty; don't sort. Hierarchy via a *Parent* column is not supported in CSV import |
| CSV import: "Iteration path does not exist" | Create Sprint 0-8 first with exact names |
| CSV import: field "Story Points" not found | Project uses Scrum: use `backlog.csv` (Effort) |
| "No hosted parallelism has been purchased or granted" | A YAML file uses `vmImage:` - switch to `pool: { name: Default }` (self-hosted) |
| Run waits "for an agent" forever | Agent offline (box/PC off, service stopped) or demands not met (`lftp`, `sqlcmd`, OS) - check Agent pools > Default > Capabilities |
| lftp `550` / "file in use" | `app_offline.htm` not uploaded or the app not yet unloaded; increase the sleep; check `MONSTERASP_FTP_ROOT` |
| lftp login/TLS error | Wrong host/user; FTPS certificate - see note in 10.3; try SFTP |
| sqlcmd login timeout | Remote access not enabled for the DB (Databases > Users and remote), wrong server name, or remote access not available on free (verify) - then use `Database:MigrateOnStartup` fallback (`08` section 3) |
| msdeploy `ERROR_USER_UNAUTHORIZED` / 401 | Web Deploy user/password or `siteXXXX` wrong; port 8172 blocked on the PC's network |
| Smoke test fails but site works a minute later | Cold start after deploy/sleep - retries cover about 3 minutes; check `App_Data/logs` if it persists |
| Blazor/SignalR "Reconnecting" in the browser | Site slept or recycled (normal on free); certificate expired (WebSockets need HTTPS) |
| Tasks warn about Node 6/10/16 handlers | Update the task major version and the agent; end-of-life Node handlers are being removed from the agent (see `10` section 1) |

## 12. Repo direction: GitHub is the source of truth (Phase 2 mirror)

GitHub (`deo-bernal/dmb-prodtrack`) stays the **source of truth**. Two Phase 2 options:

| Option | How | When |
|---|---|---|
| **A. Azure Pipelines on the GitHub repo (no mirror)** | New pipeline > **GitHub** > install the *Azure Pipelines* GitHub App for this repo only; `azure-pipelines.yml` lives in the GitHub repo; PR builds report back as GitHub checks | Simplest; Boards can link commits/PRs with `AB#<id>` via the Azure Boards GitHub App |
| **B. One-way mirror GitHub -> Azure Repos** | A GitHub Actions job pushes `main` and tags to Azure Repos with a PAT (scope *Code: Read & write*, stored as GitHub secret `AZDO_MIRROR_PAT`); pipelines and branch policies run on Azure Repos | To practise Azure Repos branch policies and build validation hands-on |

Mirror job for option B (add to a GitHub workflow only in Phase 2):

```yaml
  mirror-to-azure-repos:
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    steps:
      - uses: actions/checkout@v4
        with: { fetch-depth: 0 }
      - name: Push main and tags to Azure Repos
        env:
          AZDO_PAT: ${{ secrets.AZDO_MIRROR_PAT }}
        run: |
          set -euo pipefail
          git push "https://pat:${AZDO_PAT}@dev.azure.com/<your-org>/DMB-ProdTrack/_git/prodtrack" HEAD:refs/heads/main --force
          git push "https://pat:${AZDO_PAT}@dev.azure.com/<your-org>/DMB-ProdTrack/_git/prodtrack" --tags
```

`--force` is intentional on the replica only; never force-push GitHub `main`. Changes are made on GitHub; Azure Repos is read-only for humans in option B. Deploy from **one** system: when Azure Pipelines deploys, disable the GitHub `deploy` workflow and move the MonsterASP secrets.

## 13. Other Azure DevOps features to practise

### 13.1 Test Plans (Phase 2, PT-073; MVP uses the GitHub test-run issue PT-070)

- Needs the **Basic + Test Plans** access level (or Visual Studio Enterprise/Test Professional). **Price: US$52 per user per month; a 30-day free trial** can be started from Organization settings > Billing (or the Test Plans hub prompt) - verify the current flow. Start it at the beginning of Sprint 7, cancel/downgrade after the v1.0 release.
- Practise: a test plan "v1.0 Release" with requirement-based suites (one per epic, linked to PBIs), manual test cases for the MVP demo script (`00` section 8) with shared steps and parameters, running tests with the web runner, creating bugs from failed steps, the Progress report and a test-results widget on the dashboard.
- Without the licence, Basic users can still do **exploratory testing** with the Test & Feedback browser extension and see automated test results in Pipelines (Tests tab).

### 13.2 Artifacts (Phase 2, PT-073; the MVP option is GitHub Packages PT-071)

- **2 GiB free per organization.** Create a project-scoped feed `prodtrack` with the **nuget.org upstream** (so all restores go through the feed and are cached) and publish `ProdTrack.Contracts` from the pipeline (`NuGetAuthenticate@1` + `dotnet nuget push`). Add a retention policy (keep the last 10 versions) to stay well under 2 GiB. Feed **views** (`@local`, `@prerelease`, `@release`) are a common interview question.

### 13.3 Wiki

- Publish `docs/` as a **code wiki** (Overview > Wiki > Publish code as wiki > repo `prodtrack`, branch `main`, folder `/docs`), so the Markdown here is browsable and mermaid diagrams render (the Azure DevOps wiki supports `::: mermaid` blocks; standard fenced ```mermaid rendering may differ - check). Keep sprint notes (daily scrum, reviews, retros) in a separate **project wiki**.

### 13.4 Dashboards and queries

- Dashboard widgets: Sprint Burndown, Velocity, Cumulative Flow Diagram, Lead/Cycle time, Build History, Deployment status (environments), Test Results Trend, Query Tile ("MVP not done"), Markdown widget with the sprint goal. Analytics views/Power BI are optional.

### 13.5 Advanced Security (paid, mention only)

- **GitHub Advanced Security for Azure DevOps** is now sold as **GitHub Secret Protection** (US$19) and **GitHub Code Security** (US$30) per active committer per month (standalone since 2025): secret scanning with push protection, dependency scanning and CodeQL code scanning. Not used here (needs Azure billing); the free substitutes are `dotnet list package --vulnerable` and the security review in PT-052. Know what it does for interviews.

### 13.6 Governance settings worth knowing

- Classic build/release pipeline creation disabled by default for new organizations (YAML-first).
- Protected resources + checks, pipeline permissions, "Limit job authorization scope", and repository settings such as *Commit author email validation* and branch/tag security.
- Personal access tokens: keep short expiries and least scope; Microsoft is moving integrations from Azure DevOps OAuth apps (no new registrations since April 2025) to **Microsoft Entra** OAuth.

## 14. Change log

| Date | Version | Change |
|---|---|---|
| 2026-09-26 | 0.1 | Initial setup guide (Azure primary, GCP alternative, GitHub mirror) |
| 2026-09-26 | 0.2 | GCP primary: `targetCloud` default `gcp`, GCP auth (SA key secure file -> pipeline-token WIF), `gcp-auth.yml`, variable groups without Key Vault, Cloud SQL start step, Terraform infra pipeline with approval; Azure stage as alternative; sections on Test Plans, Artifacts, Wiki, dashboards, Advanced Security, governance |
| 2026-09-26 | 0.3 | **MonsterASP free plan + self-hosted agent:** prerequisites without cloud CLIs; section 1.2 self-hosted agent (Linux box default, Windows PC alternative); no cloud service connections (FTP/Web Deploy/SQL credentials as secret variables); variable groups `prodtrack-common`/`prodtrack-monsterasp`; single environment `prodtrack-prod` with approval; new YAML: `build.yml` (win-x86 publish, idempotent migration script, E2E on agent), `deploy-monsterasp-ftp.yml` (lftp + app_offline.htm + sqlcmd, default), `deploy-monsterasp-msdeploy.yml` (Windows), `prodtrack-ops.yml` (cert expiry, DB size, bacpac); `deployTarget` monsterasp/azure/gcp replaces `targetCloud`; GCP/WIF, Terraform and Cloud Run content removed |
| 2026-09-26 | 0.3b | **Marked Phase 2** (GitHub is the primary tooling, doc 11): banner, story references map to PT-073, section 12 rewritten (GitHub stays source of truth: Azure Pipelines GitHub App or one-way mirror to Azure Repos), mirror-to-GitHub pipeline removed |
