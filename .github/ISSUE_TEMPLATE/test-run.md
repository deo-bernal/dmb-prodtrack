---
name: Release test run
about: Manual regression checklist for a release (PT-070)
title: "Test run: vX.Y.Z"
labels: [testing, release]
---

Environment: https://msi-prodtrack.runasp.net - build/commit: `...` - date (PHT): ...

Tick each case or link the bug issue that failed it.

- [ ] E1 Sign in and land on the role home page
- [ ] E2 Create sales order and work orders; upload and approve artwork; release; print traveler
- [ ] E3 Operator scans OP code, starts, logs scrap, completes
- [ ] E4 Dashboard updates live (WIP, scrap rate)
- [ ] E5 QC pass; packing completes; work order Completed
- [ ] E6 OEE-lite, timeline and audit log visible
- [ ] E7 Cold start after > 30 min idle shows the loading page; reconnect overlay after restart
- [ ] E8 Admin creates a user and resets a password (no email)

Result: pass / fail - notes:
