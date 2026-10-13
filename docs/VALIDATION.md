# Database and photo validation — 14 October 2026

The requested cleanup replaces the previous AppData database and embedded starter pack. The live data is now **C:\Users\Jerald\Desktop\CustomPC\Database\custompc.db**, with **ProductImages** beside it. The old data was preserved in an external safety snapshot before removal.

The prepared database has exactly one account: name admin, email admin@gmail.com, password admin@gmail.com, role Admin. It contains 42 ordinary products with zero stock and 42 local photographs. Orders, builds, payments, movements, suppliers and login attempts are empty. Three compatibility rules remain. Credentials use the existing salted PBKDF2 hash format.

## Executed current checks

**55 passed, 0 failed** in the isolated data-folder harness. Build: **0 warnings, 0 errors** after a successful NuGet restore. The first restricted restore could not fetch vulnerability metadata; the subsequent forced restore succeeded.

- Default path resolves the solution-root Database folder; explicit test overrides still isolate data.
- Exactly the requested Admin account exists and its requested password authenticates as Admin only.
- All 42 product images decode from ProductImages beside the copied database; filenames contain no external paths.
- No starter product resources are embedded in the compiled application, and the source Assets/StarterCatalog folder is gone.
- No orders, payments, builds, movements or practice stock remain.
- The catalog renders 42 ordinary saved product cards. Admin sees Add product but no Staff stock adjustment or starter-import button.
- Account and product counts survive database reload.
- Complete database/photo ZIP backup remains available.

The harness operates actual WinForms controls against a copy of the prepared database. It does not write test stock/accounts into the live database.

[Current JSON report](C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703/data-folder-build/bin/DataValidation/debug/results-20261014-020102/report.json) · [Current catalog render](C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703/data-folder-build/bin/DataValidation/debug/results-20261014-020102/Catalog.png)

The previous modern UI revision passed 750 regression and 234 modern checks before this cleanup. Those historical runs include the now-removed embedded catalog importer and refer to the older data arrangement; they are not a claim that the old harness was rerun unchanged on this revision.

[Historical regression](C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703/custompc-runtime-validation/run-20261014-014617-355-26148/report.json) · [Historical modern checks](C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703/modern-ui-build/bin/ModernValidation/debug/previews-20261014-014901/report.json)

## Safety backup

The previous database, its images if present, and the removed embedded catalog are at:

`C:\Users\Jerald\.codex\visualizations\2026\10\07\01a11539-e782-7ae3-bb38-3eafc37ec703\before-data-cleanup-20261014-015809`

SQLite online backup preserved the old records before replacing them. The old AppData custompc.db and ProductImages were removed only after the snapshots and new database integrity check. Earlier incomplete preparation attempts left temporary new files; the completed preparation replaced those and verified exactly 42 image references.

## Still separate manual checks

Actual Visual Studio Designer loading, successful native file-picker selection/cancellation, physical printing, external-drive recovery and real Windows 100%/125%/150% DPI switching remain manual checks. Earlier managed scaling and runtime constructor tests do not replace them. The app remains under revision.

## Run the focused validation

```powershell
$qaRoot = 'C:\Users\Jerald\.codex\visualizations\2026\10\07\01a11539-e782-7ae3-bb38-3eafc37ec703'
dotnet build "$qaRoot\data-folder-validation\DataValidation.csproj" --artifacts-path "$qaRoot\data-folder-build" -p:UseAppHost=false
dotnet "$qaRoot\data-folder-build\bin\DataValidation\debug\DataValidation.dll"
```

QA builds, reports and snapshots remain outside source. Database is ignored by Git; copy the whole folder when transferring the prepared data. Deleting it creates an empty store and shows Setup; the app does not regenerate the 42 products or your prepared Admin.
## Form sizing revision — 14 October 2026, 02:38

The main window now opens wider to accommodate the sidebar and 1,100-pixel management pages. Inventory, Orders, Users, Suppliers, Reports and Compatibility use Designer-editable bottom FlowLayoutPanels; action buttons wrap and tables leave room above those panels. The dashboard chart host was widened with the window. Store tools retains the simplified backup layout.

The isolated harness copied the current saved catalog rather than asserting the old zero-stock fixture. No live records or stock were changed. It passed **92 checks, 0 failed**, including table/action separation and visible button containment at normal size and 950×650 page size. Build passed with zero warnings/errors. Inventory, Reports, Compatibility and Store tools runtime renders were visually reviewed. This focused check does not claim every dialog or actual Windows DPI setting has been manually tested.

[Layout report](C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703/data-folder-build/bin/DataValidation/debug/results-20261014-023758/report.json)
