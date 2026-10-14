# Validation evidence

This page separates executed checks from manual checks. Test counts overlap between runs and must not be added into a unique-test total. The application remains under revision.

## Latest relevant run

**14 October 2026, 12:26 — 449 checks passed, 0 failed. Build: 0 warnings, 0 errors.** The external harness operated real WinForms controls against an isolated copy of the saved database. No test accounts, stock or orders were written into the live database.

[Machine-readable report](C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703/data-folder-build/bin/DataValidation/debug/results-20261014-122620/report.json)

The UI refresh uses rounded buttons with visible focus, lighter tables, responsive product-card widths, separate builder panels and white chart surfaces with short day labels. Runtime renders cover all 16 forms. Admin, Staff, Customer and Guest screens were exercised; Staff inventory actions and hidden Admin pages were checked. Header containment was tested after resizing. The catalog, login, builder, Staff dashboard, inventory and CPU/GPU/Case editor images were visually inspected. Receipt layout rendering did not exercise a physical printer.

The icon cleanup retains 18 original Lucide SVGs in Assets/Icons, with licenses and source hashes. IconAssets uses SVG.NET to render vectors directly. All 18 render checks passed; there are no PNG icon files/resources. The obsolete StoreIcons bitmap loader, colour matrix and conversion script were removed. Eye/eye-off rendering, Tab reachability, accessible action labels, text/caret preservation, masking restoration, checkbox removal and no text/button overlap passed. Masked and revealed login renders plus asset-backed sidebar icons were inspected.

The control pass covers padded text/number/date fields, password reveal/remasking without changing the value, styled checkboxes, selected values in all seven part pickers, an opened native CPU dropdown captured with PrintWindow, modal Confirm/Cancel results, table binding, and visible viewport containment for management tables and resized report filters. Login, picker thumbnails, dropdown popup, modal, product editor, Inventory and Reports renders were inspected. Intermediate runs caught a column-sizing exception and table/action overlap; both were corrected before the final passing run. Actual Windows calendar interaction, file/print dialogs, full keyboard-only traversal, Visual Studio Designer and OS DPI scaling remain manual checks.

Coverage also includes seven category layouts: relevant field visibility/enabled state, hidden-field keyboard exclusion, no overlapping inputs, draft preservation across category changes, saved Case normalization and existing-part stock preservation. CPU, GPU and Case renders were inspected. Actual OS DPI checks remain separate.

Coverage: saved Admin login, local photo decoding, catalog rendering, hosted management navigation, table/button separation at normal and 950×650 page sizes, period/greeting separation, mandatory socket/RAM/PSU checks, incompatible Customer checkout rejection, removal of supplier/rules UI/model fields/tables, database reload, and complete ZIP backup/restore. A sandbox-only temporary-folder permission failure was rerun with the required access; the completed run passed.

Database removals were preceded by consistent SQLite backups. Settings/Rules/Suppliers removals preserved other table counts. Supplier-reference cleanup preserved all other product values. Photographs remained beside custompc.db.

## Current rendered examples

These images are runtime control renders from the latest isolated run, not browser mockups or proof of actual Designer interaction. Stock and accounts in the images are snapshot data, not fixed application defaults.

| Screen | Preview |
| --- | --- |
| Admin overview | [Open](C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703/data-folder-build/bin/DataValidation/debug/results-20261014-122620/Overview.png) |
| Catalog | [Open](C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703/data-folder-build/bin/DataValidation/debug/results-20261014-122620/Catalog.png) |
| Inventory | [Open](C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703/data-folder-build/bin/DataValidation/debug/results-20261014-122620/Inventory.png) |
| Reports | [Open](C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703/data-folder-build/bin/DataValidation/debug/results-20261014-122620/Reports.png) |
| Activity & Backups | [Open](<C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703/data-folder-build/bin/DataValidation/debug/results-20261014-122620/Store tools.png>) |

## Historical broader tests

The earlier modern UI revision passed 750 regression checks and 234 modern checks. Those tested Guest checkout ownership after reload, role bypasses, reservations/payment/completion, photo import, chart date boundaries, empty/error states and managed scaling. They also included features/data arrangements that have since changed, so they are historical evidence rather than current full-suite certification.

[Earlier regression JSON](C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703/custompc-runtime-validation/run-20261014-014617-355-26148/report.json) · [Earlier modern JSON](C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703/modern-ui-build/bin/ModernValidation/debug/previews-20261014-014901/report.json)

## Manual checks still open

| Check | Action |
| --- | --- |
| Visual Studio Designer | Open the 16 forms with View Designer. Runtime/design-mode construction and a successful Designer MSBuild output target do not prove the Designer UI loads. Base-form design guards were added after the reported errors. |
| Native file dialogs | Import a valid photo, test Cancel, delete the original and reopen. Select ZIP backup/restore paths through real Windows dialogs. Programmatic operations were checked separately. |
| Windows DPI | Run at actual 100%, 125%, 150%; check focus, dropdowns, wrapping, scrolling and Save/Back. Earlier managed Scale simulations are separate. |
| Printer | Print a practice receipt and inspect paper size/page breaks. |
| External-drive recovery | Save ZIP to a removable drive, restore a practice database and verify records/photos after reopening. |
| Whole store workflow | Repeat Customer order → Staff payment/assembly/pickup using a separate practice database on the target workstation. |

## Reproduce the focused run on this computer

```powershell
$qaRoot = 'C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703'
dotnet build "$qaRoot/data-folder-validation/DataValidation.csproj" --artifacts-path "$qaRoot/data-folder-build" -p:UseAppHost=false
dotnet "$qaRoot/data-folder-build/bin/DataValidation/debug/DataValidation.dll"
```

The harness expects the prepared Admin credentials and catalog; if saved credentials/catalog change, fixtures must be adapted without resetting live data. Harnesses, reports and safety snapshots are local tooling outside the source project. Keep normal bin/obj build files for Visual Studio.
