## Story

Closes #<issue> (PT-nnn: title)

## What changed

-

## Checklist

- [ ] Acceptance criteria of the story met (Given/When/Then)
- [ ] Tests: unit / integration / E2E (if UI flow) added or updated; `ci` is green
- [ ] Manually checked locally (and on prod after deploy, if user-visible)
- [ ] No secrets, no cloud SDK references in the default build, no `dmbwebsolutions.com` DNS or `find-an-agent` changes
- [ ] Migrations are expand/contract (old code still works on the new schema)
- [ ] Docs updated (`docs/02` API list, `docs/07` user guide, `docs/05` status) if behaviour changed
- [ ] Memory/DB impact considered for the free host (256 MB RAM, 1 GB DB)
