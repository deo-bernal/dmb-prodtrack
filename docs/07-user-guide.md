# 07 - User Guide

> **Status:** Draft v0.3 (2026-09-26) - written before the app exists; update each section when the feature is built (see PT-053).
> **Audience:** plant staff - planners, supervisors, operators, QC inspectors, administrators and viewers.
> **Screenshots:** every `[Screenshot placeholder: ...]` must be replaced with a real screenshot once the screen exists.
> **Note:** DMB ProdTrack is a practice application. Names, customers and numbers in examples are fictitious.

---

## 1. Getting started

### 1.1 What is DMB ProdTrack?

DMB ProdTrack tracks every production job from the customer order to packing. Planners create and release **work orders**, the plant prints a **job traveler** with QR codes, **operators scan** the traveler at each station to record their work, **QC** records inspections, and **supervisors** watch a live dashboard.

### 1.2 Signing in

1. Open the ProdTrack address your administrator gave you (desktop) - for this practice installation `https://dmb-prodtrack.runasp.net` (planned address; there is no separate test system) - or tap the **ProdTrack** icon on the station tablet.
2. Click **Sign in** and enter the email and password of your ProdTrack account (first time: the temporary password from your administrator, then choose a new one). If your administrator enabled it, you can instead click **Sign in with Google**. Admins also enter a 6-digit code from their authenticator app.
3. If nothing happens for a few seconds on the first visit (after about 30 minutes without use), the system is waking up - a "Starting up..." page is shown; wait a moment, it continues by itself.
4. You land on your home page:

| Your role | Home page |
|---|---|
| Planner | Work orders |
| Supervisor | Dashboard |
| Operator | Station queue (tablet) |
| QC | QC queue |
| Admin | Administration |
| Viewer | Dashboard (read-only) |

[Screenshot placeholder: sign-in page]

To sign out, click your name (top right) > **Sign out**. On a shared tablet **always sign out at the end of your shift**.

### 1.3 Screen basics

- **Status colours** always come with an icon and text: Ready (blue, circle), In progress (green, play), Paused (amber, pause), On hold (red, stop), Completed (grey, check), Late (red "LATE" label).
- **Times** are shown in plant time (Philippine time).
- If you see a red banner with a **reference code**, tell your supervisor or IT the code - it helps find the problem quickly.

### 1.4 Installing the tablet app (one-time, done by Admin or Supervisor)

1. On the tablet, open Chrome or Edge and go to `<ProdTrack address>/floor`.
2. Tap **Install app** (or browser menu > **Install** / **Add to Home screen**).
3. Open the new **ProdTrack Floor** icon; it runs full screen.
4. Pair the barcode scanner (USB or Bluetooth) - it works like a keyboard; no setup in ProdTrack is needed.

[Screenshot placeholder: install prompt on tablet]

## 2. Operators (tablet)

### 2.1 Choose your station

1. After sign-in, tap **Select station** or **scan the station label** (QR code on the machine, e.g. `ST:PRINT-01`).
2. The tablet remembers the station until someone changes it.

[Screenshot placeholder: station selection]

### 2.2 Your queue

The queue lists jobs **Ready** or **In progress** at your station, most urgent first (priority, then due date). Late jobs are marked **LATE**. New jobs appear automatically - you don't need to refresh.

[Screenshot placeholder: station queue]

### 2.3 Start a job

1. Take the job traveler. Find the QR code for **your step** (e.g. step 20 Printing).
2. **Scan it.** The job card opens showing product, specs, legend text and quantity. Check that the material and colours match.
3. Tap **Start** (big green button).

If you hear a low tone and see a red message:

| Message | What to do |
|---|---|
| "This step belongs to LAM-01" | You scanned the wrong step or you're at the wrong station. Scan the correct QR |
| "Previous step not completed" | The job isn't ready for you yet. Tell your supervisor if you think it is |
| "Job on hold: <reason>" | Don't work on it. Put it aside and tell your supervisor |
| "Code not recognised" | Rescan slowly; if the traveler is damaged ask the supervisor for a reprint |

### 2.4 Pause and resume

1. Tap **Pause** and choose a reason (Break, Material wait, Machine issue, Changeover, Other).
2. To continue, open the job (scan again or tap it in the queue) and tap **Resume**.

### 2.5 Log scrap

1. On the open job, tap **Log scrap**.
2. Enter the **quantity** and choose the **reason** (e.g. Misprint, Wrong colour, Lamination bubbles, Engraving error, Cut off-size, Damaged material).
3. Optional: add a note or photo. Tap **Save**.

Log scrap **when it happens**, not at the end of the shift.

### 2.6 Complete a job

1. Tap **Complete**.
2. Enter the **good quantity** (units that pass to the next step). ProdTrack shows the input quantity and scrap already logged; good + scrap cannot exceed the input.
3. Check the **material used** (filled in automatically from the job; change it if you used more or less) and tap **Confirm**.
4. The job moves to the next station. Put the parts and traveler in the outgoing area.

[Screenshot placeholder: complete dialog]

### 2.7 Station down

If your machine stops (fault, no material, maintenance): tap **Station down**, choose the reason. When it runs again tap **Station up**. This helps the plant see lost time.

## 3. Planners (desktop)

### 3.1 Enter a sales order

1. **Orders > New sales order.**
2. Choose the **customer**, enter the **customer PO number**, **order date** and **due date**.
3. Add lines: choose the **product** (e.g. Pipe marker - vinyl), fill the specs (size, colour scheme such as *Flammable - black on yellow*, legend text such as `NATURAL GAS`, flow arrow), and the **quantity**.
4. **Save.** The order gets a number like `SO-2027-00001`.

[Screenshot placeholder: sales order form]

### 3.2 Create and release work orders

1. Open the sales order > **Create work orders**. One work order per line (e.g. `WO-2027-000045`), status **Draft**.
2. If the product needs artwork: open the work order > **Artwork** > **Upload proof** (PDF/PNG/JPG/SVG, max 20 MB).
3. When the customer approves: **Approve** (add a note such as "Approved by customer email 12 Jan"). If they reject: **Reject** with the reason and upload a new version later.
4. Click **Release**. The steps (Prepress, Printing, ...) are created and the first step appears in the station queue.

### 3.3 Print the job traveler

Open the work order > **Print traveler**. Print on A4 or Letter. Keep it with the job. Need another copy? Supervisors and planners can **Reprint** (it is marked "REPRINT 1").

[Screenshot placeholder: traveler PDF]

### 3.4 Find and follow work orders

**Work orders** list: filter by status, customer, due date, station; late jobs marked **LATE**. Open a work order to see its **timeline** (who did what, when).

### 3.5 Master data (Planner/Admin)

- **Products:** catalog items with default specs and material usage per unit (bill of materials).
- **Routings:** the steps and standard minutes per product type. Editing a routing creates a new version; released jobs keep the old version.
- **Materials:** stock list; record **receipts** when deliveries arrive and **adjustments** after counts.

## 4. Supervisors

### 4.1 Live dashboard

[Screenshot placeholder: dashboard]

| Tile | Meaning |
|---|---|
| Station tiles | Jobs Ready / In progress / Paused and units at each station; red **DOWN** when a station is down |
| Late / At risk | Jobs past due, and jobs that probably won't finish in time based on remaining standard work |
| Throughput | Units completed today / this week |
| Scrap | Scrap rate by station and the top scrap reasons |
| OEE-lite | Availability x Performance x Quality per station (simplified; hover for details) |
| Low stock | Materials below reorder point |

### 4.2 Hold, resume, cancel

Open the work order > **Hold** (choose a reason, e.g. Customer change, Material issue, QC failed). Operators cannot start held jobs. **Resume** when solved. **Cancel** is only possible if no step is completed.

### 4.3 Skip a step / allow overlap

When a step isn't needed for a specific job (e.g. no lamination), open the step > **Skip** and give the reason. This is recorded in the audit trail.

### 4.4 Reprint a traveler

Work order > **Reprint traveler**.

## 5. QC inspectors

1. At the QC station, scan the QC step QR on the traveler and tap **Start**.
2. The checklist for the product appears (e.g. legend spelling, colour scheme matches spec, dimensions, adhesion/lamination, engraving depth). Enter the **sample size**.
3. For each item choose **Pass/Fail** or enter the **measured value**; values outside the tolerance turn red and fail automatically.
4. **Submit.**
   - **Passed:** the job moves to Packing.
   - **Failed:** choose **Rework** (select the station and quantity - a rework step and a new QC step are added) or **Hold** (supervisor must decide).

[Screenshot placeholder: QC checklist]

## 6. Administrators

- **Users:** there is no self-registration. In **Administration > Users > New user** enter the person's email and name, choose their **role**, and give them the **temporary password** shown (they must change it at first sign-in). Also set **employee number**, **badge code** and **home station**. Use **Reset password** if someone forgets theirs (there is no "forgot password" email - the system sends no emails, so users ask an administrator) and **Deactivate** when someone leaves (they are signed out within 30 minutes). If Google sign-in is enabled, a person can use **Sign in with Google** only when their Google email matches the email of their ProdTrack account.
- **Stations**, **reason codes**, **reference lists** (pipe-marker colour schemes, safety-sign signal words): Administration menu. Deactivate instead of delete when an item has history.
- **QC templates:** QC and Admin can edit checklists.
- **Audit log:** Administration > Audit - filter by record, user and date to see every change with old and new values.

## 7. Viewers (e.g. customer service)

Use **Work orders** search (by customer PO or SO number) to see status, current station and expected completion. You can't change anything.

## 8. FAQ

| Question | Answer |
|---|---|
| The tablet shows "Offline" | Check Wi-Fi. Wait for the banner to disappear before scanning; actions are not saved while offline in this version |
| I started the wrong job | Pause it and ask a supervisor; they can correct it (the correction is audited) |
| The traveler is lost | Supervisor or planner reprints it |
| Numbers on the dashboard look wrong | Check that jobs were completed and scrap logged at the time; report the reference code if you see errors |
| Why does the first page load slowly in the morning? | On the practice (free) hosting the app sleeps after about 30 minutes without use and wakes on the first visit (a few seconds) |
| The screen shows "Reconnecting..." | The connection to the server was interrupted (Wi-Fi, a new version being installed, or the app waking up). Wait - it reconnects by itself and reloads the page if needed; unsaved input on the current form may need to be re-entered |
| I forgot my password | Ask an administrator to reset it; there is no reset email |
| I see an "Updating - back in a minute" page | A new version is being installed; try again in a minute or two |

## 9. Quick reference card (print for each station)

```text
SCAN step QR  ->  START
PAUSE (reason) / RESUME
LOG SCRAP (qty + reason) when it happens
COMPLETE: good qty -> confirm material
STATION DOWN / UP when the machine stops
Job ON HOLD? Don't work on it - tell your supervisor
Always SIGN OUT at shift end
```

## 10. Change log

| Date | Version | Change |
|---|---|---|
| 2026-09-26 | 0.1 | Initial user guide draft (placeholders) |
| 2026-09-26 | 0.2 | Sign-in with ProdTrack accounts (optional Google), user administration, app addresses |
| 2026-09-26 | 0.3 | Address `https://dmb-prodtrack.runasp.net` (MonsterASP free hosting, no test system; custom domain deferred); wake-up and reconnect notes; FAQ: Reconnecting, forgotten password (admin reset, no email), update page |
| 2026-09-27 | 0.5 | The as-built user guide with real screenshots is `Documentations/ProdTrack-User-Guide.pdf` (this Markdown file remains the planning draft). |
