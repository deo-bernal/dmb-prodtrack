"""Builds the ProdTrack PDFs from the Markdown sources in this folder.

Usage (any OS with Python 3.11+):
    python -m venv .venv && .venv/bin/pip install weasyprint markdown
    .venv/bin/python build.py            # writes ../*.pdf and ./html/*.html
Diagrams: diagrams/*.mmd are rendered to images/diagrams/*.png with mermaid-cli (mmdc -s 2 -b white).
Screenshots: images/screens/*.png come from the Playwright docs capture
    (PRODTRACK_DOCS_SCREENSHOTS=<dir> dotnet test tests/ProdTrack.E2E.Tests --filter "FullyQualifiedName~Capture").
"""
import pathlib
import re
import sys

import markdown

HERE = pathlib.Path(__file__).resolve().parent
OUT = HERE.parent
DOCS = [
    ("user-guide.md", "ProdTrack-User-Guide.pdf"),
    ("business-document.md", "ProdTrack-Business-Document.pdf"),
    ("technical-document.md", "ProdTrack-Technical-Document.pdf"),
]
VERSION = "1.0"
DATE = "27 September 2026"


def slug(text, seen):
    base = re.sub(r"[^a-z0-9]+", "-", text.lower()).strip("-") or "section"
    s, i = base, 2
    while s in seen:
        s, i = f"{base}-{i}", i + 1
    seen.add(s)
    return s


def render(md_path):
    text = md_path.read_text(encoding="utf-8")
    meta, body = text.split("\n---\n", 1)
    fields = dict(line.split(":", 1) for line in meta.strip().splitlines())
    fields = {k.strip(): v.strip() for k, v in fields.items()}
    html = markdown.markdown(body, extensions=["tables", "fenced_code", "attr_list", "sane_lists"])
    seen, toc = set(), []

    def number_headings(match):
        level, inner = int(match.group(1)), match.group(2)
        plain = re.sub(r"<[^>]+>", "", inner)
        hid = slug(plain, seen)
        if level in (1, 2):
            toc.append((level, hid, plain))
        return f'<h{level} id="{hid}">{inner}</h{level}>'

    html = re.sub(r"<h([1-3])>(.*?)</h\1>", number_headings, html)
    # figures: an image alone in a paragraph becomes a captioned figure
    html = re.sub(r'<p><img alt="([^"]*)" src="([^"]+)" ?/?></p>',
                  r'<figure><img src="\2" alt="\1"/><figcaption>\1</figcaption></figure>', html)
    toc_html = "".join(
        f'<li class="toc{lvl}"><a href="#{hid}">{title}</a></li>' for lvl, hid, title in toc)
    title_page = f"""
<section class="title-page">
  <div class="brand">DMB ProdTrack</div>
  <h0 class="doc-title">{fields['title']}</h0>
  <div class="subtitle">{fields['subtitle']}</div>
  <table class="meta">
    <tr><th>Version</th><td>{VERSION}</td></tr>
    <tr><th>Date</th><td>{DATE}</td></tr>
    <tr><th>Application release</th><td>Sprint 2-3 batch (branch feature/sprint-2-3)</td></tr>
    <tr><th>Audience</th><td>{fields['audience']}</td></tr>
    <tr><th>Author</th><td>Deo Bernal</td></tr>
  </table>
  <p class="disclaimer">Practice project. Not affiliated with, endorsed by, or based on the internal systems of
  DMB Websolutions. Business rules and data are plausible assumptions.</p>
</section>
<section class="toc-page"><h1 class="toc-title">Contents</h1><ul class="toc">{toc_html}</ul></section>
"""
    css = (HERE / "style.css").read_text(encoding="utf-8").replace("__RUNNING__", fields["title"])
    return f"""<!doctype html><html lang="en"><head><meta charset="utf-8"><title>{fields['title']}</title>
<style>{css}</style></head><body>{title_page}<main>{html}</main></body></html>"""


def main():
    from weasyprint import HTML
    (HERE / "html").mkdir(exist_ok=True)
    only = sys.argv[1:]
    for src, pdf in DOCS:
        if only and src not in only:
            continue
        page = render(HERE / src)
        (HERE / "html" / src.replace(".md", ".html")).write_text(page.replace('src="images/', 'src="../images/'), encoding="utf-8")
        HTML(string=page, base_url=str(HERE)).write_pdf(OUT / pdf)
        print("wrote", OUT / pdf)


if __name__ == "__main__":
    main()
