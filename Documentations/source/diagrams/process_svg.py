"""Generates images/diagrams/process.svg (end-to-end business flow) - two swim rows, readable at A4 width."""
import pathlib

W, BW, BH = 1000, 138, 58
boxes = {
    # id: (x, y, label lines, fill)
    "po": (20, 60, ["Customer PO"], "#f3f4f6"),
    "so": (185, 60, ["Sales order", "header + lines"], "#dbeafe"),
    "wo": (350, 60, ["Draft work orders", "one per line"], "#dbeafe"),
    "art": (515, 60, ["Artwork proof", "approved", "(if required)"], "#fef3c7"),
    "rel": (680, 60, ["Release", "operations from", "routing"], "#dbeafe"),
    "trv": (845, 60, ["Print traveler", "with QR codes"], "#dbeafe"),
    "ops": (845, 230, ["Scan at station", "start / pause /", "complete"], "#dcfce7"),
    "scr": (845, 360, ["Log scrap", "with reason code"], "#fee2e2"),
    "qc": (620, 230, ["QC inspection", "checklist"], "#fef3c7"),
    "hold": (620, 360, ["Work order on hold", "QC-FAIL", "(supervisor)"], "#fee2e2"),
    "pack": (395, 230, ["Packing", "(last operation)"], "#dcfce7"),
    "done": (170, 230, ["Work order", "completed"], "#bbf7d0"),
}
arrows = [("po", "so", ""), ("so", "wo", ""), ("wo", "art", ""), ("art", "rel", ""),
          ("rel", "trv", ""), ("trv", "ops", "to first station"), ("ops", "qc", "each station"),
          ("qc", "pack", "pass"), ("pack", "done", ""), ("qc", "hold", "fail"), ("ops", "scr", "")]


def centre(b):
    x, y, *_ = boxes[b]
    return x + BW / 2, y + BH / 2


def edge_point(b, towards):
    cx, cy = centre(b)
    tx, ty = centre(towards)
    dx, dy = tx - cx, ty - cy
    if abs(dx) * BH > abs(dy) * BW:
        return cx + (BW / 2) * (1 if dx > 0 else -1), cy
    return cx, cy + (BH / 2) * (1 if dy > 0 else -1)


parts = [f'<svg xmlns="http://www.w3.org/2000/svg" width="{W}" height="450" viewBox="0 0 {W} 450" font-family="DejaVu Sans, Arial" font-size="12">',
         '<defs><marker id="a" markerWidth="10" markerHeight="8" refX="9" refY="4" orient="auto"><path d="M0,0 L10,4 L0,8 z" fill="#374151"/></marker></defs>',
         '<rect x="5" y="20" width="990" height="120" rx="8" fill="none" stroke="#93c5fd" stroke-dasharray="5,4"/>',
         '<text x="15" y="38" font-size="13" font-weight="bold" fill="#1e3a8a">Order, artwork and release - Planner</text>',
         '<rect x="5" y="190" width="990" height="250" rx="8" fill="none" stroke="#86efac" stroke-dasharray="5,4"/>',
         '<text x="15" y="208" font-size="13" font-weight="bold" fill="#166534">Execution and quality - Operator, QC, Supervisor</text>']
for a, b, label in arrows:
    x1, y1 = edge_point(a, b)
    x2, y2 = edge_point(b, a)
    parts.append(f'<line x1="{x1}" y1="{y1}" x2="{x2}" y2="{y2}" stroke="#374151" stroke-width="1.6" marker-end="url(#a)"/>')
    if label:
        parts.append(f'<text x="{(x1 + x2) / 2 + 6}" y="{(y1 + y2) / 2 - 5}" font-size="11" fill="#374151" text-anchor="{"start" if x1 == x2 else "middle"}">{label}</text>')
# resume loop hold -> ops
hx, hy = boxes["hold"][0] + BW, boxes["hold"][1] + BH / 2 + 12
ox, oy = boxes["ops"][0], boxes["ops"][1] + BH - 8
parts.append(f'<path d="M{hx},{hy} L{hx + 50},{hy} L{hx + 50},{oy} L{ox},{oy}" fill="none" stroke="#374151" stroke-width="1.6" stroke-dasharray="4,3" marker-end="url(#a)"/>')
parts.append(f'<text x="{hx + 56}" y="{(hy + oy) / 2 + 20}" font-size="11" fill="#374151">resume</text>')
# rejected loop on artwork
ax, ay = boxes["art"][0] + BW / 2, boxes["art"][1]
parts.append(f'<path d="M{ax - 20},{ay} C{ax - 20},{ay - 28} {ax + 20},{ay - 28} {ax + 20},{ay}" fill="none" stroke="#374151" stroke-width="1.4" marker-end="url(#a)"/>')
parts.append(f'<text x="{ax}" y="{ay - 26}" font-size="11" fill="#374151" text-anchor="middle">rejected: re-upload</text>')
for key, (x, y, lines, fill) in boxes.items():
    parts.append(f'<rect x="{x}" y="{y}" width="{BW}" height="{BH}" rx="6" fill="{fill}" stroke="#6b7280"/>')
    start = y + BH / 2 - (len(lines) - 1) * 7.5 + 4
    for i, line in enumerate(lines):
        weight = "bold" if i == 0 else "normal"
        parts.append(f'<text x="{x + BW / 2}" y="{start + i * 15}" text-anchor="middle" font-weight="{weight}">{line}</text>')
parts.append("</svg>")
out = pathlib.Path(__file__).resolve().parent.parent / "images" / "diagrams" / "process.svg"
out.write_text("\n".join(parts), encoding="utf-8")
print("wrote", out)
