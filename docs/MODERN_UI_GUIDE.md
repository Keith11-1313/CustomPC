# Learn the modern screens

Read [CODE_GUIDE.md](CODE_GUIDE.md) first for the basic C# lessons. This guide connects the new screens to their actual code. The project remains under revision.

## One window, separate forms

Open `DashboardForm.cs` and find `OpenScreen`. This method creates a form such as `CatalogForm`, sets `TopLevel = false`, and places it inside the `pageHost` panel. `Dock = Fill` makes it use the panel's space. The charcoal menu belongs to DashboardForm and stays visible. The previous page is disposed when you navigate away; its timer and images must be disposed too.

CatalogForm, BuilderForm, OrdersForm and the management pages still have separate `.cs`, `.Designer.cs` and `.resx` files. Right-click the `.cs` file and choose **View Designer**. Login, editors and receipts use `ShowDialog` because they need a focused decision before returning to the page.

**Try it:** put a breakpoint inside `DashboardForm.OpenScreen`, run F5 and click Parts catalog. Watch `form.TopLevel` become false. **Checkpoint:** why does the sidebar stay visible? Because it belongs to the main window, outside the hosted page.

## Follow one product into a build

1. `CatalogForm.LoadRows` filters the active records and creates a `ProductCard` for each result.
2. ProductCard shows the name, price, availability and `ProductThumbnail`. Its Add to build event reaches `CatalogForm.AddToBuild`.
3. `DashboardForm.Draft` stores selected part IDs in `BuildDraft`. Choosing another CPU replaces the CPU entry, rather than adding two CPUs.
4. `BuilderForm` receives the same draft, restores its selections and recalculates compatibility and price.
5. Checkout refreshes saved prices and stock again. A Guest sees login first. Customer login continues the same checkout, and `AppStore.CreateOrder` saves a PcBuild and linked order owned by that Customer.

**Try it:** select a CPU, visit the catalog again, then return to the builder. **Checkpoint:** is that unfinished selection already an order in SQLite? No. BuildDraft is in memory. The build and order are saved only after valid authenticated checkout and confirmation. Signing out clears the draft.

## Understand the dashboards

`AppStore.Dashboards.cs` reads saved records and returns small data records. `DailyValue` means one date and one number. `NamedValue` means one name and one number. `CategoryStock` holds the three stock counts for a category. These are chart inputs, not new database tables.

Admin's sales chart groups **confirmed payments by local payment date**. Top parts counts quantities from paid orders in the selected period. Staff sees order counts by local creation date, the current work queue and current stock counts. Staff does not receive sales figures. Inventory and queue are current snapshots regardless of the 7/30/90-day selection.

`DashboardChart` creates ScottPlot only when real runtime data is drawn. Its default constructor shows a database-free Designer preview. Zero-data charts show a useful empty state. Use the chart's selection list and **Open selected**, or double-click a day/bar, to open the filtered page. DashboardForm creates `OrderViewFilter` or `InventoryViewFilter`; filtering narrows an already permission-checked list. Changing account clears chart selections and resets the default period.

**Try it:** as Staff, open the Paid queue selection. **Checkpoint:** should an older Paid order disappear because you chose seven days? No. The queue is current work, not a seven-day history report.

## Product photographs and backups

The Part record stores **only `PhotoFileName`**. `ProductPhotos.Folder` locates ProductImages beside the selected database. Admin's part editor copies JPG/PNG uploads there and normalizes them to PNG, at most 1600 pixels on the longest edge. It does not depend on the original Downloads file afterward. ProductThumbnail reads a detached, smaller display image so the file is not kept locked. Missing or unreadable files show Photo unavailable.

Removing a photo clears its record reference. Older copied files are retained for recovery; complete ZIP backups include the folder. Do not delete them manually while testing recovery. `AppStore.Photos.cs` deletes a newly imported copy if saving its Part fails.

`AppStore.BackupPhotos.cs` writes ZIP backups containing `custompc.db` and ProductImages. Restore validates paths, database links and image bytes before replacement, saves a complete before-restore ZIP, and rolls back if photo replacement fails. If rollback itself fails, recovery files are retained and their paths are shown. Older `.db` backups remain supported, but they do not contain photographs.

## Where the product data lives

The project-root Database folder contains custompc.db and ProductImages. The prepared SQLite database has 42 normal product records and one Admin account; it has no orders, payments or practice stock. Photos have ordinary copied filenames referenced by Part.PhotoFileName. They are not embedded in the application or generated on startup.

Admin edits/adds products through Parts & inventory. Staff records actual receipts. Copy the whole Database folder to keep images with records. StoreDataPath.cs finds the solution folder while developing, or uses Database beside the app if copied elsewhere. CUSTOMPC_DATABASE_PATH remains available for isolated tests. If a database is missing, the normal setup screen creates a new empty store; it does not regenerate the 42 products.

Read [CATALOG_MANIFEST.md](CATALOG_MANIFEST.md) and CATALOG_DATA.json for source documentation. Those documents are not runtime seed code. The old embedded catalog importer and its button were removed.
## Find the file you need

| Change | File |
| --- | --- |
| Palette, button appearance | StoreTheme.cs |
| Product card layout | ProductCard.Designer.cs |
| Catalog layout/search | CatalogForm.Designer.cs / CatalogForm.cs |
| Builder columns and photo previews | BuilderForm.Designer.cs / BuilderForm.cs |
| Shared empty message and action | EmptyStateControl.Designer.cs / EmptyStateControl.cs |
| Chart appearance | DashboardChart.cs |
| Chart calculations and permissions | AppStore.Dashboards.cs |
| Main menu and page hosting | DashboardForm.cs |
| Draft selected component IDs | BuildDraft.cs |
| Uploaded photos | ProductPhotos.cs / AppStore.Photos.cs |
| Complete recovery | AppStore.BackupPhotos.cs |
| Default database location | StoreDataPath.cs |

The `AppStore.*` files are parts of one `partial` class. Read the form's click handler first, then the one store method it calls. You do not need to understand every helper before changing one screen.
Management pages keep action buttons in actionRow, a FlowLayoutPanel in the Designer file. It docks at the bottom and wraps buttons. The table anchors on all sides and reserves space above actionRow. Edit this panel in View Designer instead of placing bottom buttons individually.

Compatibility checks are mandatory application logic in AppStore.Compatibility.cs. There is no Admin rules page, rule editor, Rules table or toggle. Socket, memory, graphics output and PSU checks run during preview and checkout.

Supplier management is removed: no supplier pages, part selector, inventory column, model association or Suppliers table. Existing product sources in the research manifest are documentation, not supplier records.
