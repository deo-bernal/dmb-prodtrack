# Documentations

Product documentation for DMB ProdTrack as built after the Sprint 2-3 batch (version 1.0, 27 September 2026).

| File | Contents |
|---|---|
| `ProdTrack-User-Guide.pdf` | Per-role walkthroughs, sign-in and password change, every screen and workflow, shop-floor scanning, troubleshooting (real screenshots) |
| `ProdTrack-Business-Document.pdf` | Purpose, production context, stakeholders and roles, business processes, rules, KPIs, scope and future work |
| `ProdTrack-Technical-Document.pdf` | Architecture, stack, data model, API summary, security, audit, concurrency, SignalR, logging/health, testing, CI, local setup, configuration, deployment options |

## Sources

`source/` holds everything needed to rebuild the PDFs:

- `*.md` - document text (a small header block, then Markdown)
- `style.css` - print layout (title page, table of contents with page numbers, running headers/footers)
- `diagrams/*.mmd` - Mermaid diagrams, rendered to `images/diagrams/*.png` with mermaid-cli (`mmdc -s 3 -b white`); `diagrams/process_svg.py` draws `process.svg`
- `images/screens/*.png` - screenshots captured from the running app by the Playwright docs test:
  `$env:PRODTRACK_DOCS_SCREENSHOTS="<folder>"; dotnet test tests/ProdTrack.E2E.Tests --filter "FullyQualifiedName~Capture"`
- `html/*.html` - the generated HTML (open in a browser to preview)
- `build.py` - converts Markdown to HTML and PDF with WeasyPrint:

```bash
python -m venv .venv && .venv/bin/pip install weasyprint markdown   # Windows: .venv\Scripts\pip
.venv/bin/python Documentations/source/build.py
```
