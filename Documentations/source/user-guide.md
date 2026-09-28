title: User Guide
subtitle: Per-role walkthroughs for the back office and the shop-floor app
audience: Plant staff - administrators, planners, supervisors, operators, QC inspectors, viewers
---

# About this guide

DMB ProdTrack tracks production work for a plant that makes safety identification products: pipe markers, valve tags,
safety signs and labels. It follows a job from the customer's sales order to a finished, inspected and packed work order.

The application has two parts:

| Part | Address (local run) | Used by | Device |
|---|---|---|---|
| **Back office** | `https://localhost:5001/` | Admin, Planner, Supervisor, QC, Viewer | Desktop or laptop browser |
| **Shop floor** | `https://localhost:5001/floor/` | Operators, QC inspectors, supervisors | Tablet or phone at a station (installable web app) |

All screenshots in this guide were captured from the running application with the automated Playwright tests, using
demonstration data. Names of customers and people are fictitious.

## Roles at a glance

| Role | What the role can do |
|---|---|
| **Admin** | Everything, including user administration, stations and reason codes |
| **Planner** | Sales orders, work orders (create, edit, release, hold, cancel), artwork upload and approval, products and routings |
| **Supervisor** | Everything read-only, plus hold/resume/cancel work orders, print travelers and run operations on the floor |
| **Operator** | Run operations on the floor (start, pause, complete, log scrap), upload artwork; read everything else |
| **QC** | Run operations and record QC inspections; read everything else |
| **Viewer** | Read-only access to dashboards, orders, products and routings |

A user can hold several roles. Menu entries that a role cannot use are hidden, and the server rejects the action
even if someone tries to call it directly.

# Signing in and your password

## Sign in

Open the back office address in a browser. You are sent to the sign-in page. Enter the email address and password your
administrator gave you and select **Sign in**.

![Sign-in page](images/screens/01-login.png)

After five wrong passwords the account is locked for 15 minutes. An administrator can clear the lock by resetting the
password.

## Change your password

New accounts, and accounts whose password was reset by an administrator, must choose a new password at the first
sign-in. Enter the temporary password once and the new password twice. Passwords need at least 12 characters.
You can change your password at any time with **Change password** in the menu.

![Forced password change](images/screens/02-change-password.png)

To sign out, select **Sign out** at the top right. Sessions last up to 12 hours of activity.

> **First run on a new database.** In the local development setup the app creates the administrator
> `admin@prodtrack.local` with the temporary password `ChangeMe!Dev2026`. Change it at the first sign-in and then
> create personal accounts for everyone else under **Users**.

# Back office screens

## Dashboard

The dashboard is the home page. Tiles show how many work orders are in each status (Draft, Released, InProgress,
OnHold, Completed), how many are **late** (open and past their due date), and today's output: work orders completed,
good units, scrap units and scrap rate. The **WIP by station** table lists, per station, the operations that are ready,
in progress, paused and pending.

The dashboard updates **live**: whenever an operator starts, pauses or completes a step, the numbers refresh within a
second or two. The green **Live** badge shows the live connection is up; **Refresh** reloads on demand.

![Dashboard with live KPIs and WIP by station](images/screens/03-dashboard.png)

## Sales orders

**Sales orders** lists customer orders with number, customer, customer PO, due date, status and line count. Filter by
status or search by customer, PO or number.

![Sales order list](images/screens/04-sales-orders-list.png)

To enter an order (Planner, Admin) select **New sales order**, fill in customer, customer PO and due date, then add one
line per product: product, quantity and, where relevant, legend text (for example the pipe content "FUEL GAS").
The product's default specification is copied to the line. Save to get a number such as `SO-2026-00001`.

![New sales order with lines](images/screens/05-sales-order-new.png)

On the order page, **Create work orders** makes one Draft work order per line. Lines that already have a work order
are skipped, so it is safe to press it again after adding a line. Each line then links to its work order.
**Edit** changes the header and the lines that do not yet have a work order. **Cancel order** is only possible while
no open work orders exist for it.

![Sales order with generated work orders](images/screens/06-sales-order-detail.png)

## Work orders

**Work orders** lists all jobs, newest first, 25 per page. Filter by status, search by number, SKU or customer, and
see late jobs highlighted.

![Work order list](images/screens/07-work-orders-list.png)

**New work order** creates a job directly from a product when there is no sales order (for example stock or rework):
choose the product, quantity, due date, priority (1 = highest), customer and legend.

![New work order](images/screens/08-work-order-new.png)

### Artwork proofs

Products that need artwork approval (signs, printed pipe markers, custom labels) cannot be released until a proof is
approved. On the Draft work order upload a proof (PDF, PNG, JPG or SVG, up to 20 MB). A Planner or Admin then
**Approves** it, or **Rejects** it with a reason; a new upload supersedes the previous version.

![Draft work order waiting for artwork approval](images/screens/09-work-order-draft-artwork.png)

### Release

**Release** copies the current routing of the product type into the work order as numbered operations
(10, 20, 30, ...). The first operation becomes **Ready** at its station; the others are **Pending**. From now on the
work order appears in the station queues on the shop floor.

![Released work order with operations](images/screens/10-work-order-released.png)

Each operation row shows input, good and scrap quantities, who started it and when, and an **Open on floor** link.
The page refreshes live while operators work.

### Print the traveler

**Print traveler** opens a printable page with a QR code for the work order and one QR code per operation, the
specification and blank columns for operator, good, scrap and date. Select **Print** (or save as PDF from the print
dialog). The traveler travels with the job; operators scan its codes at each station.

![Printable traveler with QR codes](images/screens/11-traveler.png)

### Hold, resume and cancel

Supervisors, Planners and Admins can stop a job:

* **Hold** - choose a hold reason code (for example `CUSTOMER-CHANGE`) and add a note. The work order shows a yellow
  banner; floor actions on it are blocked until it is resumed. A failed QC inspection puts the work order on hold
  automatically with reason `QC-FAIL`.
* **Resume** - returns the job to the status it had before the hold.
* **Cancel** - requires a reason and is only possible while no operation has been completed.

![Work order on hold with reason](images/screens/12-work-order-on-hold.png)

### When someone else changed the record

If another user saved the same record after you opened it, ProdTrack does not overwrite their change. You see the
message *"Someone else changed this ... after you opened it"* with a **Reload latest** button. Reload, re-apply your
change and save again.

## Products and routings

**Products** lists the catalogue with SKU, name, type and whether artwork approval is required. Planners and Admins can
add or edit products and their type-specific specification (material, size, colour scheme, signal word and so on).

![Product catalogue](images/screens/15-products.png)

**Routings** shows the current routing (sequence of stations) for each product type, with setup time and standard
minutes per unit.

![Routings per product type](images/screens/13-routings.png)

**Edit** opens the routing editor. Add, remove and reorder steps, choose the station, setup minutes, standard minutes
per unit and whether a step may overlap the previous one. Saving creates a **new version**; work orders that are
already released keep the operations they were released with.

![Routing editor](images/screens/14-routing-editor.png)

## QC checklists

**QC checklists** shows the inspection checklist for each product type: pass/fail items and measured items with
tolerances (for example engraving depth 0.1-0.3 mm). Inspectors complete these checklists on the shop floor.

![QC checklist templates](images/screens/21-qc-checklists.png)

## Administration (Admin)

### Stations

Stations are the work centres on the floor (Prepress, Printing, Laminating, Engraving, Die-cutting, QC, Packing).
Admins can add, rename or deactivate stations. Deactivating does not affect work already in progress.

![Stations](images/screens/16-stations.png)

### Reason codes

Reason codes explain **scrap** (misprint, colour mismatch, cutting error, material defect), **pauses** (break, waiting
for material, end of shift), **downtime** and **holds** (customer change, QC fail). Admins add codes or deactivate old
ones; deactivated codes are no longer offered but stay on historical records.

![Reason codes](images/screens/17-reason-codes.png)

### Users

**Users** lists every account with roles and status.

![User list](images/screens/18-users.png)

**New user** creates an account with email (the sign-in name), display name, optional employee number, badge number and
home station, one or more roles, and a temporary password. The user must change it at the first sign-in. ProdTrack
does not send email - give the temporary password to the person directly.

![Create a user](images/screens/19-user-new.png)

Open a user to edit the profile and roles, **Disable user** (the person can no longer sign in; open sessions end at the
next security check), **Enable** again, or **Reset password** to a new temporary password (this also clears a lockout).

![Edit a user, disable or reset password](images/screens/20-user-edit.png)

# Shop floor app

## Open the app and choose your station

On the station tablet open `/floor/` (for example `https://localhost:5001/floor/`) and sign in with your own account.
In Chrome or Edge you can **Install** the app so it opens full screen from the home screen.

Pick your station. The tablet remembers it.

![Station picker](images/screens/22-floor-stations.png)

## Station queue

The queue lists the operations that can be worked at this station - Ready, In progress and Paused - ordered by
priority and due date. Work orders on hold stay visible with a yellow **On hold** badge, but cannot be worked until they are resumed. The queue refreshes by itself when work arrives from the previous station. Tap an operation to open it,
or scan.

![Station queue](images/screens/23-floor-queue.png)

## Scanning

The scan box at the top of the floor pages accepts:

* **Camera scanning** - select the camera button and hold the traveler QR code in front of the camera. This uses the
  browser's built-in barcode detector (Chrome and Edge on Android, ChromeOS and macOS). The browser asks for camera
  permission the first time; choose **Allow**.
* **Hardware scanners** - USB or Bluetooth scanners that type the code and press Enter work in the scan box.
* **Manual entry** - type the work order number (for example `WO-2026-000004`) or an operation code
  (`OP:WO-2026-000004:10`) and press Enter.

A work order code opens its current operation; an operation code opens that step. A short tone confirms a good scan.
Unknown codes show a clear message:

![Unknown code](images/screens/27-floor-scan-not-found.png)

## Run an operation

The operation page shows the work order, product, station, input quantity, good and scrap so far, and the due date.

![Operation ready to start](images/screens/24-floor-operation-ready.png)

1. **Start** - clocks the step on and records you as the operator.
2. **Pause** - choose a reason (break, waiting for material, end of shift) and select **Pause**. **Resume** continues.
3. **Log scrap** - enter the quantity and a scrap reason, optionally a note, and select **Log scrap**. Scrap can be
   logged several times while the step is in progress.
4. **Complete step** - enter the good quantity and select **Complete step**. Good plus scrap cannot exceed the input.
   The next step becomes Ready at its station with the good quantity as its input. Completing the last step completes
   the work order.

![Operation in progress with scrap logged](images/screens/25-floor-operation-in-progress.png)

## QC inspection

At an inspection station (QC-01) the operation page shows the checklist for the product type. Mark each pass/fail item
and enter measured values; values outside the tolerance fail automatically. Enter the sample size and notes and select
**Record inspection** (QC or Admin role).

* **Pass** completes the step and moves the job on to packing.
* **Fail** puts the work order on hold with reason `QC-FAIL`. A supervisor reviews it and resumes it when the problem
  is fixed.

![QC checklist on the floor](images/screens/26-floor-qc-checklist.png)

# Typical workflows by role

## Planner

1. Enter the customer's **sales order** with its lines.
2. Select **Create work orders** on the sales order.
3. For each work order that needs artwork, upload the proof and approve it (or wait for approval).
4. **Release** the work order and **Print traveler**; hand the traveler to the first station.
5. Watch progress on the **Dashboard** and the work order page.

## Supervisor

1. Watch the **Dashboard**: late jobs, WIP by station, today's scrap rate.
2. **Hold** a work order when there is a customer change or quality issue; **Resume** once it is cleared.
3. Review work orders on hold after a failed inspection (`QC-FAIL`).
4. Help at stations: supervisors can also run operations on the floor.

## Operator

1. Open `/floor/`, pick your station, and work the queue top to bottom.
2. Scan the traveler (or tap the operation), **Start**, log any **scrap** with a reason, then **Complete step** with
   the good quantity.
3. **Pause** with a reason for breaks or when waiting for material.

## QC inspector

1. At the QC station scan the traveler and **Start** the inspection step.
2. Fill in the checklist and **Record inspection**. Fail holds the job for the supervisor.

## Administrator

1. Create user accounts and assign roles; reset passwords and disable leavers.
2. Maintain stations and reason codes.
3. Change your own bootstrap password at the first sign-in.

## Viewer

Use the dashboard, sales order and work order lists and detail pages for information. Actions are hidden.

# Troubleshooting

| Problem | What to do |
|---|---|
| "Invalid sign-in attempt" | Check the email and password (passwords are case-sensitive). After 5 failures the account is locked for 15 minutes; ask an Admin to reset the password. |
| Asked to change password every time | Complete the change form; the new password must be at least 12 characters and different from the temporary one. |
| A menu item or button is missing | Your role does not allow that action. Ask an Admin to add the role. |
| "Someone else changed this ... after you opened it" | Another user saved the record first. Select **Reload latest**, re-apply your change and save again. |
| Cannot release a work order | Approve the artwork proof first (products that require artwork), and check the work order is still Draft. |
| Cannot start or complete an operation | The work order may be on hold or cancelled, or the step is not Ready yet (a previous step is still open). At inspection stations a passed inspection is needed before completion. |
| Complete is rejected | Good quantity plus scrap cannot exceed the input quantity. |
| Camera button does not appear or shows no picture | The browser does not support the barcode detector, or camera permission was denied. Allow the camera in the site settings, use a hardware scanner, or type the code. The camera only works on `https://` or `localhost`. |
| "No work order or operation found" after scanning | The traveler may be from another environment or the code was mistyped. Check the number printed under the title. |
| Dashboard shows no **Live** badge | The live connection dropped (for example after the laptop slept). It reconnects automatically; select **Refresh** or reload the page. |
| "An error has occurred" banner | Reload the page. If it keeps happening, note the time and tell the administrator; the server log (`App_Data/logs`) has the details and a trace id. |
| Shop-floor app shows an old version | Close and reopen the app, or reload with Ctrl+F5 on a desktop browser. |
