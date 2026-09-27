# MonsterASP.NET free plan: hosting research for DMB ProdTrack

Researched 2026-09-26 (Asia/Manila). Sources: official monsterasp.net pages, help.monsterasp.net docs, forum.monsterasp.net staff answers, plus Cloudflare, Microsoft, and Somee docs. I did not sign up for anything or take any external action.
Legend: **[V]** = verified on an official page (URL given). **[F]** = MonsterASP staff answer on their forum. **[U]** = unverified, inferred, or needs a test after signup.

## Verdict

**Partial fit. For the stated requirements it does NOT fit.**
- The free plan runs the whole tech stack: .NET 10, Blazor Server, SignalR/WebSockets, EF Core + MSSQL, Web Deploy.
- **Deal-breaker 1: no custom domains on the free plan.** `prodtrack.dmbwebsolutions.com` and `dev-prodtrack.dmbwebsolutions.com` can't be bound. The app would live at `<name>.runasp.net` / `<name>.tryasp.net`.
- **Deal-breaker 2: only 1 website and 1 database.** Separate dev and prod environments aren't possible. Opening a second account to get around this is banned by the ToS.
- **Constraint: the ToS allows free plans only for development, testing, learning, and evaluation.** No commercial use, no production workloads, and no production processing of personal data. A practice app is fine.
- The cheapest way to get both custom domains is Premium Single: $1.95/mo for the first year, then $2.50/mo, billed annually. It gives 1 website + 3 subdomains and 2 DBs. That needs payment, which breaks the zero-cost, no-card rule.

## 1. Sign-up, credit card, expiry
- **No credit card is needed for the free plan.** [V] https://www.monsterasp.net/ ("No hidden fees. No credit card required."), https://www.monsterasp.net/Pricing/ ("No card. No catch."), https://www.monsterasp.net/ASP.NET-Freehosting/
- **Expiry and inactivity:** there's no fixed renewal. However, ToS §3.2(e) says free plans "may be deleted, together with all data, after a period of inactivity or where the account is not verified". §10.5 says the 90-day retention doesn't apply to free accounts. [V] https://www.monsterasp.net/Terms/
  - **The inactivity period isn't published. [U]** To be safe, log in to the control panel regularly, keep traffic going, and keep your own DB backups.
- **Free plans get no SLA, no backups, and no support. They can be limited, suspended, or discontinued at any time without notice.** [V] ToS §3.2, §7.3, §9.2 https://www.monsterasp.net/Terms/
- **One account per person.** Multiple accounts to get around free-plan limits are prohibited. [V] ToS §2.4
- **Let's Encrypt on free must be renewed by hand every 90 days** (see §4). That's effectively a periodic login requirement. [V] https://help.monsterasp.net/books/https/page/how-to-activate-https-with-lets-encrypt-certificate

## 2. Free plan limits
Source: https://www.monsterasp.net/Pricing/ [V] unless noted.

| Item | Free |
|---|---|
| Websites | 1 (free subdomain only, *.runasp.net / *.tryasp.net [V] https://www.monsterasp.net/ASP.NET-Freehosting/) |
| Disk | 5 GB |
| RAM | 256 MB dedicated (own app pool) |
| CPU | Not published; shared, fair use (ToS §6) [V]/[U] |
| Bandwidth | "Limited". Staff say "there are no fixed limits" but heavy use may be restricted or suspended [F] https://forum.monsterasp.net/d/238-monthly-data-transfer-limit-free-plan |
| Databases | 1 DB, 1 GB. The site copy says "MSSQL or MySQL", so it's 1 total, not 1 of each [V]/[U on exact split] |
| SQL Server version | SQL Server 2025 advertised [V] https://www.monsterasp.net/ |
| Email | None on free. Outgoing SMTP from apps is Premium-only [V] https://help.monsterasp.net/books/e-mails/page/sending-e-mails-from-your-applications-using-our-local-smtp-server |
| SSL | The Pricing table shows "HTTPS (Let's Encrypt) ×" for Free. But the HTTPS help doc and the Freehosting page say free HTTPS is available on free subdomains with **manual renewal every 90 days** (conflict; the doc is more specific) [V] |
| App pool idle | **Sleeps after 30 min with no requests; can't be changed on free.** Premium: 3 h, with "Always Running" on request [F] https://forum.monsterasp.net/d/37-application-pool-timeout-configuration-was-set-to-30-minutes |
| Backups | None on free [V] ToS §7.3 |
| Scheduled tasks | Premium-only [F] https://forum.monsterasp.net/d/92-introducing-a-new-task-scheduler-for-websites-and-databases |
| "DNS, Tasks, Add-ons, Logs" | "Limited" on free [V] Pricing |
| Datacenter | EU (Germany) only [V]. Expect about 200+ ms latency from PH [U] |

## 3. .NET versions / self-contained
- **Supported versions:** .NET 11/10/9/8 are listed for the free plan, with ".NET 10 Latest LTS". [V] https://www.monsterasp.net/ASP.NET-Freehosting/, https://www.monsterasp.net/.NET10-Hosting/
- **Hosting models:** InProcess is supported whether the app is a DLL or an EXE. OutOfProcess works only with a DLL. Running an EXE OutOfProcess disables the app pool, and you need a support ticket to get it back. [V] https://help.monsterasp.net/books/websites/page/aspnet-core-hosting-model-support
- **Self-contained deployment:** the EXE/InProcess support implies it should work, but no doc says so explicitly. [U]
  - Framework-dependent is the documented route.
  - The official GitHub Actions sample publishes with `--runtime win-x86` (32-bit, which suits a small 256 MB pool). [V] https://help.monsterasp.net/books/github/page/how-to-deploy-website-via-github-actions
  - Keep `<AspNetCoreHostingModel>InProcess</AspNetCoreHostingModel>`.

## 4. Custom domain on free
- **Not allowed.** [V] https://www.monsterasp.net/ FAQ ("Free plans only work with our free subdomains"), https://www.monsterasp.net/Pricing/ ("No custom domains"), https://help.monsterasp.net/books/domains/page/dns-settings-for-custom-domains ("You can add your own custom domains only to PREMIUM Websites").
- **DNS on Premium, for reference:**
  - Either delegate NS to ns1/ns2/ns3.machineasp.net, or set records. Apex: A → server IP. Subdomains/www: CNAME → `siteXXXX.siteasp.net`. [V] same doc.
  - For this project you'd use CNAME records only, which leaves the apex and www alone:
    - `prodtrack` CNAME `siteXXXX.siteasp.net`
    - `dev-prodtrack` CNAME `siteYYYY.siteasp.net`
  - Premium Single = 1 website + 3 subdomains [V] Pricing.
  - Let's Encrypt renews automatically on Premium [V] HTTPS doc.
- **Workarounds on free (not recommended):**
  - **Cloudflare Origin Rules can't do it.** Overriding the Host header, which IIS needs so the request matches the runasp.net binding, is Enterprise-only. [V] https://developers.cloudflare.com/rules/origin-rules/ (Availability table)
  - **A Cloudflare Worker reverse proxy** (free tier; can pass WebSockets through) could front `*.runasp.net` under `prodtrack.dmbwebsolutions.com`.
    - Not tested. [U]
    - It needs the dmbwebsolutions.com zone on Cloudflare DNS.
    - It adds latency, and it's arguably a way around a plan restriction (ToS §5.2 bars circumventing "resource limits"). **Grey area.**
  - A plain redirect (`prodtrack.dmbwebsolutions.com` → 301 to runasp.net URL) is possible through any DNS/redirect service. It isn't really a custom domain.

## 5. WebSockets / SignalR / Blazor Server
- **Supported, and WebSockets are on by default.** "Native SignalR support with WebSockets enabled by default. Persistent circuits." [V] https://www.monsterasp.net/Blazor-Hosting/, https://www.monsterasp.net/ (features list: Blazor, SignalR, gRPC, WebSockets)
- **Staff say WebSockets need HTTPS enabled.** [F] https://forum.monsterasp.net/d/87-do-server-support-web-sockets
- **The Blazor page's "auto-renewed" Let's Encrypt claim is for Premium.** On free, renewal is manual. [V] HTTPS doc
- **Caveat:** the 30-min idle sleep and the 256 MB RAM mean cold starts and dropped circuits. Use SignalR/Blazor automatic reconnect. [U: impact to be measured]

## 6. Deployment methods and Azure Pipelines
- **Methods:** Web Deploy (msdeploy), FTP/SFTP, ZIP upload, Git/GitHub Actions, Visual Studio publish profile (.publishSettings). [V] https://help.monsterasp.net/books/deploy, https://www.monsterasp.net/ASP.NET-Freehosting/
  - No REST deploy API is documented. [U]
- **Documented CI:** GitHub Actions on `windows-latest` using `rasmusbuchholdt/simply-web-deploy@2.1.0`.
  - Secrets: WEBSITE_NAME=siteXXXX, SERVER_COMPUTER_NAME=https://siteXXXX.siteasp.net:8172, SERVER_USERNAME=siteXXXX, SERVER_PASSWORD. [V] https://help.monsterasp.net/books/github/page/how-to-deploy-website-via-github-actions
- **Documented msdeploy command line:** [V] https://help.monsterasp.net/books/deploy/page/how-to-deploy-website-content-from-command-line
  ```
  msdeploy.exe -verb:sync -source:contentPath="<publish dir>"
    -dest:contentPath="siteXXXXX",computerName="https://siteXXXXX.siteasp.net:8172/msdeploy.axd?site=siteXXXXX",userName="siteXXXXX",password="***",authtype="Basic",includeAcls="False"
    -allowUntrusted -disableLink:AppPoolExtension -disableLink:ContentExtension -disableLink:CertificateExtension
  ```
- **Azure DevOps:** there's no official MonsterASP doc for it. [V: none found]
  - `AzureRmWebAppDeployment` / `AzureWebApp` won't work, because they need an Azure service connection.
  - **Recommended:** a `PowerShell@2` / script step that calls `msdeploy.exe` with the command above. WebDeploy password goes in a secret variable.
    - The hosted `windows-latest` image (Windows Server 2025 / VS2026) includes the component `Microsoft.VisualStudio.Component.WebDeploy`. [V] https://github.com/actions/runner-images/blob/main/images/windows/Windows2025-Readme.md
    - Its exact msdeploy.exe path isn't confirmed. [U] Probe `C:\Program Files\IIS\Microsoft Web Deploy V3\msdeploy.exe` and `C:\Program Files (x86)\IIS\Microsoft Web Deploy V3\msdeploy.exe`, and fall back to `choco install webdeploy -y`.
  - **Fallback:** the FTP Upload task `FtpUpload@2` to `ftp://siteXXXX.siteasp.net` port 21 (FTP creds are in the control panel [V] https://help.monsterasp.net/books/deploy/page/how-to-deploy-website-content-via-ftpsftp).
    - Upload `app_offline.htm` first, or restart the site, to avoid "550 file in use" locks. Delete it at the end.
    - It's slower and doesn't sync deletes cleanly.
  - `IISWebAppDeploymentOnMachineGroup@0` needs an agent *on* the IIS server, so it's not usable.
- **⚠ No-card caveat for Azure Pipelines (important):**
  - Microsoft's current doc says the **Microsoft-hosted free tier (1 job, 60 min/run, 1,800 min/mo) must be enabled by linking an Azure subscription / setting up billing**. [V] https://learn.microsoft.com/en-us/azure/devops/pipelines/licensing/concurrent-jobs?view=azure-devops
  - An Azure subscription normally needs a card. [U]
  - The **self-hosted free tier (1 parallel job, no time limit) is granted automatically**. [V] same doc.
  - **Zero-card route:** run a self-hosted Azure Pipelines agent on your own Windows PC, with the .NET 10 SDK and Web Deploy installed.
  - **Alternative:** GitHub Actions free minutes with the officially documented workflow.

Sample Azure Pipelines YAML (framework-dependent, self-hosted or hosted Windows agent):
```yaml
trigger: [main]
pool: { name: Default }   # self-hosted Windows agent (or vmImage: windows-latest if hosted grant available)
variables: { buildConfiguration: Release }   # MONSTER_SITE, MONSTER_PWD (secret) in a variable group
steps:
- task: UseDotNet@2
  inputs: { packageType: sdk, version: 10.0.x }
- script: dotnet publish src/ProdTrack.Web/ProdTrack.Web.csproj -c $(buildConfiguration) -r win-x86 --self-contained false -o $(Build.ArtifactStagingDirectory)/publish
- powershell: |
    $md = @("C:\Program Files\IIS\Microsoft Web Deploy V3\msdeploy.exe","C:\Program Files (x86)\IIS\Microsoft Web Deploy V3\msdeploy.exe") | Where-Object { Test-Path $_ } | Select-Object -First 1
    if (-not $md) { throw "msdeploy.exe not found" }
    $site = "$(MONSTER_SITE)"
    & $md -verb:sync "-source:contentPath=$(Build.ArtifactStagingDirectory)\publish" `
      "-dest:contentPath=$site,computerName=https://$site.siteasp.net:8172/msdeploy.axd?site=$site,userName=$site,password=$env:MONSTER_PWD,authtype=Basic,includeAcls=False" `
      -allowUntrusted -enableRule:AppOffline -disableLink:AppPoolExtension -disableLink:ContentExtension -disableLink:CertificateExtension
  env: { MONSTER_PWD: $(MONSTER_PWD) }
```
(`-enableRule:AppOffline` avoids locked DLLs. It's standard msdeploy, not in the MonsterASP doc. [U])

## 7. Remote SQL Server access
- **Remote access exists but is DISABLED by default.** Turn it on per database under Databases → db → "Users and remote". After that, SSMS can connect using the server/login/password shown. [V] https://help.monsterasp.net/books/databases/page/remote-access-for-database, https://help.monsterasp.net/books/databases/page/sql-server-management-studio-ssms
- The docs carry the "freehosting" banner, which implies it's available on free. **Not stated explicitly for free. [U]**
- **SSL/TLS connection doc:** https://help.monsterasp.net/books/databases (the "Secure connection (SSL/TLS) to MSSQL" article) [V listing]
- **EF migrations from the pipeline:** a third-party repo runs `dotnet ef database update --connection "<secret>"` from GitHub Actions against MonsterASP. [V as example] https://github.com/RegManApp/RegMan.Backend/blob/main/.github/workflows/publish.yml
  - No IP allow-listing is mentioned in the docs. [U]
  - An idempotent migration script (`dotnet ef migrations script --idempotent`) run with sqlcmd, or a migration bundle, also works.
- **Web UI:** https://webmssql.monsterasp.net [V link on login page https://admin.monsterasp.net/login]

## 8. Restrictions
- **Commercial / production use:** prohibited on free (ToS §3.2(c)). [V] https://www.monsterasp.net/Terms/
  - Staff: free is for learning; projects that don't fit that purpose may be suspended. [F] forum/238
- **Ads / branding:** none. "We do not place banner ads." [V] https://www.monsterasp.net/ FAQ
- **Background / hosted services:** they run only while the worker is alive, and it sleeps after 30 min idle on free. Always Running is Premium-only (staff mention Hangfire on Premium). [F] https://forum.monsterasp.net/d/104-aspnet-core-background-services-support
- **Scheduled tasks:** Premium-only; minimum interval 30 min, max run 10 min. [F] forum/92
- **Outbound connections:**
  - SMTP from apps: Premium-only [V].
  - General outbound HTTP isn't documented as blocked. [U]
  - Prohibited activities: open proxies, VPN, mining, scanning, etc. [V] ToS §5.2
- **Fair use:** there's a notice, then 2 days to fix or upgrade, then throttling or suspension (ToS §6.3). [V]

## 9. Alternatives (truly free, no card)
| Host | Custom domain on free | .NET 10 | DB | Catches |
|---|---|---|---|---|
| **Somee.com** free [V] https://somee.com/freeaspnethosting.aspx | **Yes, 1 web domain.** Let's Encrypt, manual renewal | Listed "ASP.NET Core 3/5/6/7/8/9/10". The same page's FAQ says "up to .NET 9" (conflict) [U] | 1 MSSQL Express, **30 MB** data + 30 MB log | **Forced ad banner** on every page. 150 MB disk, 5 GB/mo transfer. Site deleted after ~30–45 days without visits and 60 days without a panel login. DB deleted after 30 days without queries. Deploy is FTP only per FAQ (no Web Deploy). WebSocket is ticked only for paid packages in the comparison table [U: table ticks not rendered] |
| **FreeASPHosting.net** | Search snippet: "add a domain for free" [U] | Not confirmed; the site mentions .NET 6/7/8 [U] | MSSQL free [U] | Site blocked by a Cloudflare bot check; couldn't verify. https://freeasphosting.net/ |

**Takeaway:** Somee is the only verified free no-card .NET host that allows a custom domain. But its forced ads, 30 MB DB, uncertain WebSocket support on free, and FTP-only deploy make it a poor fit for Blazor Server + SignalR + Identity. MonsterASP is technically much better, but only on its subdomain.

## Recommendation
1. **To stay zero-cost:** use MonsterASP free with `prodtrack.runasp.net` (or similar) as the only environment. Dev runs locally with SQL Server LocalDB or Docker.
   - Accept that there are no custom domains, the site sleeps after 30 min, and HTTPS needs a manual 90-day renewal.
   - Deploy through a self-hosted Azure Pipelines agent running msdeploy, or through GitHub Actions.
   - Leave all DNS for dmbwebsolutions.com untouched.
2. **If the custom domains are a must:** it's Premium Single ($1.95/mo first year, billed annually, paid by card or other provider), which gives prodtrack + dev-prodtrack via CNAMEs. Or the risky Cloudflare Worker proxy. Both conflict with the zero-cost/no-card or ToS spirit, so they need your decision.
