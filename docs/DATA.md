# Data provenance and validation

## Source

*Uganda's Verified Administrative Units, July 2022*: a 3,019-page A4 report (generated from Microsoft Access on 19 July 2022) listing, per district and constituency, every subcounty/town, parish and village with its numeric code.

## Extraction

The report has a fixed layout, so each column sits at a constant x position. `tools/extract_units.py` reads word coordinates (`pdftotext -bbox`) and classifies each code by column rather than by whitespace. It also handles:

- right-aligned codes (3-digit codes start ~6 pt further left than 2-digit ones);
- long names that wrap onto a second line (appended to the unit in the same column);
- rows that are repeated at the top of a continuation page (merged, not duplicated).

## Validation

The extractor exits non-zero unless all of these hold. They compare the result with figures the document prints about itself:

| Check | Result |
|---|---|
| National village total vs. the document's closing total | 71,230 = 71,230 |
| Villages per district vs. each "END OF DISTRICT" line | 146 / 146 match |
| Duplicate village codes within a parish | none (after the correction below) |
| Per-subcounty "TOTAL VILLAGES IN" lines vs. extracted counts | 2,190 of 2,198 matched with zero mismatches; the other 8 have label text that differs slightly from the unit name |

The test suite additionally asserts the counts at every level, that all `FullCode`s are unique, and that every parent/child link is consistent.

## Corrections to the source

Corrections are explicit, minimal and listed in `tools/corrections.json`. There is currently one:

| Unit | Source | Package | Reason |
|---|---|---|---|
| MANAFWA › BUNAMONE WARD › BUTTA | code `01` (a stray `0` is printed on the next line) | `02` | Two villages in the parish both had code `01`. Codes otherwise ascend (01, 03, 05, 07), so the second `01` is read as a typo for `02`. This is an inference; remove the entry from `corrections.json` and regenerate to get the source value back. |

## Known source quirks (kept verbatim)

- Code sequences have gaps (e.g. 01, 03, 05); that is normal in the register.
- Names are used as printed, including odd punctuation (`KATOVU GAVU[T/C]`, `ACHOLI_BUR TOWN COUNCIL`) and any spelling errors.
- A handful of names contain accented capitals (`PAYÍLA`) or lower-case letters (`DHOs`).
- The document's closing line labels the national total "ALBERTINE"; this appears to be a label error in the source and does not affect the figures.
- The report is a point-in-time snapshot. Units created or renamed since July 2022 are not reflected.
