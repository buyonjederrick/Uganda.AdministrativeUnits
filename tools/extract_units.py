#!/usr/bin/env python3
"""Extract Uganda's Verified Administrative Units from the official PDF into the
dataset embedded in the Uganda.AdministrativeUnits package.

Usage:
    python tools/extract_units.py <source.pdf> <output.json.gz>

Requires poppler-utils (`pdftotext`). The PDF is a fixed-layout Access report, so each
column (subcounty / parish / village) sits at a constant x position; words are classified
by x coordinate rather than by whitespace. The result is validated against the totals the
document prints about itself (per-district and the national village total); the script
exits non-zero if any of them disagree.
"""
import argparse
import gzip
import json
import re
import subprocess
import sys
import tempfile
from collections import defaultdict
from pathlib import Path

WORD = re.compile(r'<word xMin="([\d.]+)" yMin="([\d.]+)" xMax="[\d.]+" yMax="[\d.]+">(.*?)</word>')
# Code columns are right-aligned, so 3-digit codes start ~6pt further left than 2-digit ones.
COL_RANGE = {"sc": (46.5, 54.5), "pa": (226.0, 234.0), "vi": (370.5, 379.0)}
DAYS = {"Monday,", "Tuesday,", "Wednesday,", "Thursday,", "Friday,", "Saturday,", "Sunday,"}
CORRECTIONS_FILE = Path(__file__).with_name("corrections.json")


def unescape(s):
    for a, b in (("&apos;", "'"), ("&quot;", '"'), ("&lt;", "<"), ("&gt;", ">"), ("&amp;", "&")):
        s = s.replace(a, b)
    return s


def read_pages(bbox_html):
    words, started = [], False
    with open(bbox_html, encoding="utf-8") as f:
        for line in f:
            if "<page " in line:
                if started:
                    yield words
                words, started = [], True
            elif (m := WORD.search(line)):
                words.append((float(m.group(1)), float(m.group(2)), unescape(m.group(3))))
    if started:
        yield words


def group_rows(words, tol=3):
    by_y = defaultdict(list)
    for x, y, t in words:
        by_y[round(y)].append((x, y, t))
    rows, anchor = [], None
    for k in sorted(by_y):
        if anchor is not None and k - anchor <= tol:
            rows[-1].extend(by_y[k])
        else:
            anchor = k
            rows.append(list(by_y[k]))
    return [sorted(r) for r in rows]


def is_code(t):
    return re.fullmatch(r"\d{1,3}", t) is not None


def column_of(x):
    return next((n for n, (lo, hi) in COL_RANGE.items() if lo <= x <= hi), None)


def header_value(words, label):
    toks = [t for _, _, t in sorted(words) if t != label]
    code = next(t for t in toks if is_code(t))
    return code, " ".join(t for t in toks if t != code)


def parse(bbox_html):
    districts, district_totals, grand = {}, {}, None
    d = c = sc = pa = vi = None
    can_append = {"sc": True, "pa": True, "vi": True}
    for page_no, words in enumerate(read_pages(bbox_html), 1):
        head = [w for w in words if w[1] < 110]
        body = [w for w in words if w[1] >= 110]
        code, name = header_value([w for w in head if 70 < w[1] < 90.5], "DISTRICT:")
        nd = districts.setdefault(code, {"code": code, "name": name, "constituencies": {}})
        if nd is not d:
            d, c, sc, pa = nd, None, None, None
        code, name = header_value([w for w in head if 90.5 <= w[1] < 110], "CONSTITUENCY:")
        nc = d["constituencies"].setdefault(code, {"code": code, "name": name, "subcounties": {}})
        if nc is not c:
            c, sc, pa = nc, None, None

        for ws in group_rows(body):
            texts = [t for _, _, t in ws]
            first = texts[0]
            if first.startswith("SUBCOUNTY/TOWN") or first in DAYS or first == "Page":
                continue
            if first == "TOTAL" and "NUMBER" in texts:
                grand = int(texts[-1].replace(",", ""))
                continue
            if first == "TOTAL" and "VILLAGES" in texts:
                continue                                   # per-subcounty subtotal (informational)
            if first == "END":
                district_totals[d["code"]] = int(texts[-1].replace(",", ""))
                continue

            starts = [(column_of(x), i) for i, (x, _, t) in enumerate(ws) if column_of(x) and is_code(t)]
            if not starts:  # a wrapped continuation of a long name
                fx = ws[0][0]
                if 180 < fx < 195:                         # wrapped "TOTAL VILLAGES IN <name>" label
                    continue
                col = "sc" if fx < 150 else "pa" if fx < 330 else "vi"
                target = {"sc": sc, "pa": pa, "vi": vi}[col]
                if target is None:
                    sys.exit(f"page {page_no}: orphan continuation row: {' '.join(texts)}")
                if all(is_code(t) and len(t) == 1 for t in texts):
                    continue                               # stray single-digit artefact in the source
                if can_append[col]:
                    target["name"] += " " + " ".join(texts)
                continue

            for k, (col, i) in enumerate(starts):
                end = starts[k + 1][1] if k + 1 < len(starts) else len(ws)
                code, name = ws[i][2], " ".join(t for _, _, t in ws[i + 1:end])
                if col == "sc":
                    fresh = code not in c["subcounties"]
                    sc = c["subcounties"].setdefault(code, {"code": code, "name": name, "parishes": {}})
                    pa = None
                elif col == "pa":
                    fresh = code not in sc["parishes"]
                    pa = sc["parishes"].setdefault(code, {"code": code, "name": name, "villages": []})
                else:
                    fresh = True
                    vi = {"code": code, "name": name}
                    pa["villages"].append(vi)
                can_append[col] = fresh                    # repeated page-top rows must not re-append wraps
    return districts, district_totals, grand


def to_lists(districts):
    return [{
        "code": dd["code"], "name": dd["name"],
        "constituencies": [{
            "code": cc["code"], "name": cc["name"],
            "subcounties": [{
                "code": s["code"], "name": s["name"],
                "parishes": [{"code": p["code"], "name": p["name"], "villages": p["villages"]}
                             for p in s["parishes"].values()],
            } for s in cc["subcounties"].values()],
        } for cc in dd["constituencies"].values()],
    } for _, dd in sorted(districts.items())]


def apply_corrections(data):
    applied = 0
    for corr in json.loads(CORRECTIONS_FILE.read_text(encoding="utf-8")):
        hits = [v for d in data if d["name"] == corr["district"]
                for cc in d["constituencies"] for s in cc["subcounties"]
                for p in s["parishes"] if p["name"] == corr["parish"]
                for v in p["villages"] if v["name"] == corr["village"] and v["code"] == corr["from"]]
        if len(hits) != 1:
            sys.exit(f"correction did not match exactly one record: {corr}")
        hits[0]["code"] = corr["to"]
        applied += 1
    return applied


def validate(data, district_totals, grand):
    errors = []

    def parishes(d):
        return (p for cc in d["constituencies"] for s in cc["subcounties"] for p in s["parishes"])

    def villages(d):
        return sum(len(p["villages"]) for p in parishes(d))

    total = sum(villages(d) for d in data)
    if total != grand:
        errors.append(f"national total {total} != document total {grand}")
    for d in data:
        if villages(d) != district_totals.get(d["code"]):
            errors.append(f"district {d['name']}: {villages(d)} != {district_totals.get(d['code'])}")
        for p in parishes(d):
            codes = [v["code"] for v in p["villages"]]
            if len(codes) != len(set(codes)):
                errors.append(f"duplicate village code in {d['name']} / {p['name']}")
    return total, errors


def main():
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("pdf")
    ap.add_argument("output")
    args = ap.parse_args()
    with tempfile.TemporaryDirectory() as tmp:
        bbox = Path(tmp, "bbox.html")
        subprocess.run(["pdftotext", "-bbox", args.pdf, str(bbox)], check=True)
        districts, district_totals, grand = parse(bbox)
    data = to_lists(districts)
    applied = apply_corrections(data)
    total, errors = validate(data, district_totals, grand)
    if errors:
        sys.exit("VALIDATION FAILED:\n  " + "\n  ".join(errors))
    payload = {
        "dataset": {
            "title": "Uganda's Verified Administrative Units",
            "edition": "July 2022",
            "publishedOn": "2022-07-19",
            "sourceNote": "Extracted from ADMINISTRATIVE_UNITS_IN_UGANDA_JULY_2022.pdf (3,019 pages).",
            "correctionsApplied": applied,
        },
        "districts": data,
    }
    raw = json.dumps(payload, ensure_ascii=False, separators=(",", ":")).encode("utf-8")
    with open(args.output, "wb") as out, gzip.GzipFile(filename="", fileobj=out, mode="wb",
                                                      compresslevel=9, mtime=0) as f:
        f.write(raw)                                       # no name/mtime in header => reproducible bytes
    print(f"districts={len(data)} villages={total} (document: {grand}) corrections={applied}")
    print(f"json={len(raw):,} bytes  gz={Path(args.output).stat().st_size:,} bytes")


if __name__ == "__main__":
    main()
