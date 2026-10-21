# Validation evidence

This page separates executed checks from manual checks. Test counts overlap between runs and must not be added into a unique-test total. The application remains under revision.

## Latest relevant run

**15 October 2026, 16:21 â€” 501 checks passed, 0 failed. Build: 0 warnings, 0 errors.** The external harness operated real WinForms controls against an isolated copy of the saved database. No test accounts, stock or orders were written into the live database.

[Machine-readable report](C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703/data-folder-build/bin/DataValidation/debug/results-20261015-162152/report.json)

Screen-fit checks covered usable window areas 1536x816, 1366x728, 1280x680 and 1024x680, plus the maximized workspace, where all seven component selectors were checked inside the visible builder. They checked sidebar position, no forced builder horizontal overflow, visible checkout, nested Back/header separation and reaching Case by scrolling. Initial and scrolled renders were reviewed. An intermediate maximized-window clamp exception was found and corrected; the final run passed. Windows display-scale settings were not changed.

Builder checks cover bold labels, Guest button wording, removal of the redundant sign-in message, CPU-platform changes, cleared incompatible motherboard/RAM selections and PSU filtering. The automated Guest flow selected a complete build, logged in as the isolated Customer, confirmed the order, closed its receipt and verified the saved owner/parts after reloading the copied database.

The UI refresh uses rounded buttons with visible focus, lighter tables, responsive product-card widths, separate builder panels and white chart surfaces with short day labels. Runtime renders cover all 16 forms. Admin, Staff, Customer and Guest screens were exercised; Staff inventory actions and hidden Admin pages were checked. Header containment was tested after resizing. The catalog, login, builder, Staff dashboard, inventory and CPU/GPU/Case editor images were visually inspected. Receipt layout rendering did not exercise a physical printer.

Historical result (superseded by the PNG migration below): the icon cleanup retained 18 original Lucide SVGs in Assets/Icons, with licenses and source hashes. IconAssets uses SVG.NET to render vectors directly. All 18 render checks passed; there are no PNG icon files/resources. The obsolete StoreIcons bitmap loader, colour matrix and conversion script were removed. Eye/eye-off rendering, Tab reachability, accessible action labels, text/caret preservation, masking restoration, checkbox removal and no text/button overlap passed. Masked and revealed login renders plus asset-backed sidebar icons were inspected.

The control pass covers padded text/number/date fields, password reveal/remasking without changing the value, styled checkboxes, selected values in all seven part pickers, an opened native CPU dropdown captured with PrintWindow, modal Confirm/Cancel results, table binding, and visible viewport containment for management tables and resized report filters. Login, picker thumbnails, dropdown popup, modal, product editor, Inventory and Reports renders were inspected. Intermediate runs caught a column-sizing exception and table/action overlap; both were corrected before the final passing run. Actual Windows calendar interaction, file/print dialogs, full keyboard-only traversal, Visual Studio Designer and OS DPI scaling remain manual checks.

Coverage also includes seven category layouts: relevant field visibility/enabled state, hidden-field keyboard exclusion, no overlapping inputs, draft preservation across category changes, saved Case normalization and existing-part stock preservation. CPU, GPU and Case renders were inspected. Actual OS DPI checks remain separate.

Coverage: saved Admin login, local photo decoding, catalog rendering, hosted management navigation, table/button separation at normal and 950Ã—650 page sizes, period/greeting separation, mandatory socket/RAM/PSU checks, incompatible Customer checkout rejection, removal of supplier/rules UI/model fields/tables, database reload, and complete ZIP backup/restore. A sandbox-only temporary-folder permission failure was rerun with the required access; the completed run passed.

Database removals were preceded by consistent SQLite backups. Settings/Rules/Suppliers removals preserved other table counts. Supplier-reference cleanup preserved all other product values. Photographs remained beside custompc.db.

## Current rendered examples

These images are runtime control renders from the latest isolated run, not browser mockups or proof of actual Designer interaction. Stock and accounts in the images are snapshot data, not fixed application defaults.

| Screen | Preview |
| --- | --- |
| Admin overview | [Open](C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703/data-folder-build/bin/DataValidation/debug/results-20261015-162152/Overview.png) |
| Catalog | [Open](C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703/data-folder-build/bin/DataValidation/debug/results-20261015-162152/Catalog.png) |
| Inventory | [Open](C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703/data-folder-build/bin/DataValidation/debug/results-20261015-162152/Inventory.png) |
| Reports | [Open](C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703/data-folder-build/bin/DataValidation/debug/results-20261015-162152/Reports.png) |
| Activity & Backups | [Open](<C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703/data-folder-build/bin/DataValidation/debug/results-20261015-162152/Store tools.png>) |

## Historical broader tests

The earlier modern UI revision passed 750 regression checks and 234 modern checks. Those tested Guest checkout ownership after reload, role bypasses, reservations/payment/completion, photo import, chart date boundaries, empty/error states and managed scaling. They also included features/data arrangements that have since changed, so they are historical evidence rather than current full-suite certification.

[Earlier regression JSON](C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703/custompc-runtime-validation/run-20261014-014617-355-26148/report.json) Â· [Earlier modern JSON](C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703/modern-ui-build/bin/ModernValidation/debug/previews-20261014-014901/report.json)

## Manual checks still open

| Check | Action |
| --- | --- |
| Visual Studio Designer | Open the 16 forms with View Designer. Runtime/design-mode construction and a successful Designer MSBuild output target do not prove the Designer UI loads. Base-form design guards were added after the reported errors. |
| Native file dialogs | Import a valid photo, test Cancel, delete the original and reopen. Select ZIP backup/restore paths through real Windows dialogs. Programmatic operations were checked separately. |
| Windows DPI | Run at actual 100%, 125%, 150%; check focus, dropdowns, wrapping, scrolling and Save/Back. Earlier managed Scale simulations are separate. |
| Printer | Print a practice receipt and inspect paper size/page breaks. |
| External-drive recovery | Save ZIP to a removable drive, restore a practice database and verify records/photos after reopening. |
| Whole store workflow | Repeat Customer order â†’ Staff payment/assembly/pickup using a separate practice database on the target workstation. |

## Reproduce the focused run on this computer

```powershell
$qaRoot = 'C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703'
dotnet build "$qaRoot/data-folder-validation/DataValidation.csproj" --artifacts-path "$qaRoot/data-folder-build" -p:UseAppHost=false
dotnet "$qaRoot/data-folder-build/bin/DataValidation/debug/DataValidation.dll"
```

The harness expects the prepared Admin credentials and catalog; if saved credentials/catalog change, fixtures must be adapted without resetting live data. Harnesses, reports and safety snapshots are local tooling outside the source project. Keep normal bin/obj build files for Visual Studio.

### 20 October 2026 â€” report caption and backup layout

Changed the report action to Refresh. Activity & Backups now groups saving and restoring into separate Designer-editable cards, with the password inside Restore and a shorter successful account-check message. Existing restore authentication and replacement confirmation remain in place.

Executed a separate compiled WinForms preview against a copied database: 16 checks passed across 1100Ã—688, 850Ã—650 and 760Ã—600 client sizes (backup/restore/password/status/table containment plus report caption). Reviewed the rendered narrow layout and corrected the activity-header row height. Preview files are outside the source project in `backup-ui-check/build/bin/Check/debug/preview` under the external validation workspace. This check did not execute file dialogs, restore operations, Visual Studio Designer or changes to Windows display scaling.

The running application locked the normal build executable; validation used a separate output folder. Close the running app and rebuild to display the changed source.


### 20 October 2026 — PNG icons and Svg package removal

Converted all 20 Lucide UI icons to transparent 192×192 PNGs, preserving the license and original source hashes alongside PNG hashes. Removed the SVG assets and Svg package reference. IconAssets now uses System.Drawing with cached PNGs, alpha-based tinting and high-quality bilinear downscaling; no conversion tool is included in the project. Product photos and the live database were unchanged.

Separate build: zero warnings/errors. Executed 198 checks against a copied database: all 20 PNG resources present, no Svg assembly reference, 180 icon rendering/colour checks (20 icons × 3 sizes × 3 colours), and the existing 16 backup-layout/report-caption checks. Inspected the PNG contact sheet and rendered login screen, including the eye icon. NuGet's resolved package list contains no Svg package. Actual Visual Studio Designer and Windows display-scale changes were not tested.

### 20 October 2026 — inventory filters, empty state and Refresh placement

Inventory now uses Designer-editable layout containers for its header, filters, table surface and bottom actions. Refresh sits at the top right beside filters, matching Reports. The table is hidden for an empty result so only the reusable centred empty state is shown. Clear filters also resets Include archived. Dropdown popups match their field width; table header minimum widths now account for measured label text.

Separate build: zero warnings/errors. The focused runner passed 222 checks total, including 24 inventory checks across 1366×728, 1000×650 and 850×600 client sizes: single empty state, centring, Refresh parent/placement, visible action containment, popup width and restoring populated rows. Reviewed empty and populated inventory renders. This run used a copied database and did not validate physical display-scale changes or Visual Studio Designer.

### 21 October 2026 — login branding, password rules and activity clarity

Login uses the supplied Assets/backdrop.png under a dark green tint, at its original pixel dimensions, centred and clipped by the panel without scaling. Brand and hero labels use the bundled Righteous family explicitly with compatible text rendering. Removed the login footer below the account/guest buttons.

New or changed passwords must contain 12–128 characters, uppercase and lowercase letters, a number and a non-whitespace symbol. Setup and account saving share ValidateNewPassword; sign-in still accepts existing passwords. Historical accounts are not reset. Form labels describe the combination requirement.

New audit entries describe order status/customer/total, product price/on-hand/archive state, account role/state, payment amount/order, stock movement quantity/reason and build component count. These are saved-state summaries, not full before/after diffs. Old audit text is preserved because its missing historical values cannot be reconstructed reliably. Activity rows wrap and double-click opens full details.

Separate build: zero warnings/errors. Focused runner: 234 checks passed against a copied database, including six invalid-password cases, one valid combination, saved readable account activity, exclusion of the password from activity, both Righteous font families and removal of the footer. Reviewed the rendered final login screenshot. Actual Visual Studio Designer and physical display-scale settings were not tested.

Login backdrop follow-up: fixed the source photograph's 171-DPI metadata shrinking it inside the bitmap by drawing with explicit pixel source/destination rectangles. The panel now crops the photo at native size, enlarging proportionally only when needed to cover a larger panel. Set the logo PictureBox background to Transparent in Designer and runtime. Separate build passed with zero warnings/errors; 235 focused checks passed, including logo transparency. Visually reviewed the final login render: photo reaches every panel edge and logo has no solid rectangle behind it.

### 21 October 2026 — requested administrator credential and order cleanup

At the owner's request, backed up Database/custompc.db outside the source tree, updated the existing admin@gmail.com account to the supplied new password using salted PBKDF2-SHA256 (600,000 iterations), and cleared 3 orders, 3 builds and 1 payment. All 42 product records and stock values were compared before/after and remained unchanged; all 4 accounts and stock movement history were preserved. Database integrity check returned OK. Cleared the admin login throttle only.

Password validation now rejects a password equal to its account email (ignoring letter case and outer whitespace) in setup, registration/account saves and login. Changing an account email while keeping a password equal to the new email also requires a new password. No automatic password-change dialog was added.

Separate build: zero warnings/errors. The copied-database runner passed its 235 checks plus assertions that the old credential fails without a session, the new credential signs in as Admin, orders/builds/payments are empty, and an otherwise complex password matching its email is rejected. README and Store guide list the updated prepared credentials. Backup: before-account-order-cleanup-20261021-090218.db in the external validation workspace.

### 21 October 2026 — product specifications and chart empty states

Product cards and catalog details now open ProductDetailsForm, with a separate Designer file: product name, price/availability, local photo, scrollable specification rows and Close. Known GPU attributes receive clear labels (graphics memory, colour, cooling, variant, recommended PSU and length); other saved specifications remain intact. No product specifications or stock were changed in the database. EmptyStateControl now centres its icon/message/detail/action as one compact group; empty charts include their title so sales and top-parts panels are distinguishable.

Separate build passed; the existing 235-check focused suite passed. Added rendered previews for the RTX 4070 SUPER specifications dialog and a no-payments chart, and visually inspected both. These additional visual previews are not counted as automated assertions. Visual Studio Designer was not opened.

### 21 October 2026 — action icons and catalog Refresh

Added PNG icons for receipt, edit product/account, movement history and CSV export actions. Pencil was downloaded from Lucide and converted outside the source project; no Svg package was added to CustomPC. Catalog Refresh now sits at the upper right of the filter row and the old bottom refresh row no longer reserves space. Separate build: zero warnings/errors; 245 focused checks passed, including all 21 PNGs and the catalog Refresh placement. Visually reviewed catalog and inventory renders. Corrected a malformed separator in product-card availability text found during review.

### 21 October 2026 — structured product specification table

Replaced the specifications bullet list with a two-column, read-only table with subtle row separators and wrapped values. Product photo and price/stock sit to the left; stock is shown once. ProductSpecifications translates the existing saved text into category-aware attribute names, separates CPU cores/threads when explicit, and retains unrecognised text as Additional detail. Custom `Label: value` entries are supported. No catalog data or stock was changed and no new hardware facts were inferred.

Separate build passed with zero warnings/errors. Focused runner passed 288 checks, including preservation of the recorded details for all 42 products and CPU cores/threads mapping. Visually reviewed CPU dialog at 720×500 and GPU at 900×650; longer specifications remain accessible by scrolling. Visual Studio Designer and actual OS display-scale changes were not tested.

### 21 October 2026 — Admin overview summary and stock attention

Added four Admin cards: confirmed payment amount, distinct paid orders in the selected local-date period, current low-stock product count and current out-of-stock count. Added Stock attention with a clear no-alert state and filtered inventory links. Admin's charts share one row, have persistent headings and retain chart selection/empty-state actions. Removed Admin's duplicate bottom navigation buttons. Staff retains operational charts and its existing navigation. Fixed malformed inventory/stock chart title separators.

Separate build passed with zero warnings/errors; focused runner passed 292 checks including summary-card and stock-attention visibility at 1366×800 and 1000×680. Visually reviewed the smaller Admin overview render. These checks used a database copy; no live sales, stock or accounts were modified. Actual Visual Studio Designer, physical OS scaling and a complete Staff interaction regression were not performed in this change.

### 21 October 2026 — Staff overview and remaining operational icons

Staff now uses the compact three-chart layout with four count-only cards: incoming orders for the selected period, current work queue, ready-for-pickup orders and current stock alerts. Added filtered stock-attention links and removed duplicate bottom navigation buttons. Sales amounts/Admin report access remain unavailable. Corrected malformed category-selection separators and angled stock-chart labels for narrower panels.

Added PNG-backed icons to Adjust stock, Record adjustment, Confirm store payment and Next order stage. The new Lucide arrow was converted outside the project; CustomPC still has no Svg package.

Separate build: zero warnings/errors. Focused suite: 303 checks passed, including all 22 PNGs, Staff cards containing no sales amounts, and Staff report access remaining disabled. Visually reviewed Staff overview at 1100×750, stock-adjustment dialog and empty Orders screen with enabled/disabled icon styles. All QA accounts/data stayed in the copied database. Full payment/stage processing and physical Windows display-scale settings were not retested in this UI change.

### 21 October 2026 — LiveCharts2 replaces ScottPlot

Replaced the ScottPlot.WinForms reference with LiveChartsCore.SkiaSharpView.WinForms 2.0.4. NuGet's resolved dependency list and the compiled assembly reference check confirm ScottPlot is absent. DashboardChart still presents a database-free Designer placeholder and constructs runtime controls only when data exists. Existing backend date calculations, permissions, chart-selection targets and empty-state actions were retained.

The chart layer now uses a green line with light area fill, rounded bars, a donut with an external legend and horizontal stacked stock bars. Axis lines/grids are subtle; date labels are spaced and full product names remain available through tooltips and the labelled selection control. Clicking series points opens the corresponding target; the selection dropdown and Open selected button remain available for keyboard users. Clearing role/account data disposes the runtime chart and clears all targets.

Separate build: zero warnings/errors. Focused runner: 317 checks passed, including all six runtime chart constructions, six labelled navigation actions, disposal/clearing, absence of ScottPlot references and previous Admin/Staff restrictions. Inspected synthetic six-chart render plus real copied-data Admin and Staff renders. Synthetic sales/queue values exist only in the external QA tool, never in the store database. Direct physical mouse-hit testing, Visual Studio Designer and OS scaling changes remain manual checks.
