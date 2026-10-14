# CustomPC code reference

Use [CODE_GUIDE.md](CODE_GUIDE.md) for lessons. This reference answers where behavior lives and how records move between screens and SQLite.

## File map

| File | Responsibility |
| --- | --- |
| Program.cs | Initializes WinForms, single-instance guard, Setup/Login then Dashboard |
| StoreDataPath.cs | Default Database folder resolution |
| AppStore.cs | Lazy singleton, session, loaded records, transactions |
| AppStore.Accounts.cs | Login, registration, role assignment, password hashing |
| AppStore.Inventory.cs | Parts, reservations, availability, Staff stock adjustments |
| AppStore.Compatibility.cs | Mandatory build checks |
| AppStore.Orders.cs | Owned build/order creation, payments, progress, cancellation/expiry |
| AppStore.Dashboards.cs | Role-checked saved-data chart projections |
| AppStore.Photos.cs / ProductPhotos.cs | Admin photo import, detached image reads, save cleanup |
| AppStore.Maintenance.cs | First Admin setup, activity, throttling, database backup/restore |
| AppStore.BackupPhotos.cs | Complete ZIP backup and photo recovery |
| AppStore.SampleData.cs | Explicit practice-mode fixtures only |
| Database.cs | Schema and JSON row reads/writes |
| Models.cs | Saved record classes |
| StoreWindow.cs | Shared scrolling, styling and design-time guards |
| StoreTheme.cs / AppFonts.cs / BrandLogo.cs | Palette, process-local fonts and supplied logo |
| Ui.cs | Prices, grids, receipts, CSV/text export and validation messages |
| AppLog.cs | Unexpected-error diagnostic references |

`AppStore.*.cs` are parts of one `partial` class. `AppStore.Current` creates it lazily. Forms call business methods; they do not write SQL directly.

## The 15 forms

| Form | Purpose |
| --- | --- |
| SetupForm | First Admin when database is empty |
| LoginForm / RegisterForm | Authentication, Guest access and Customer registration |
| DashboardForm | Main window, sidebar, role dashboard and account switching |
| CatalogForm / BuilderForm | Product cards, draft selections and checkout |
| OrdersForm / ReceiptForm | Order actions and owned/read-authorized receipt |
| InventoryForm / PartEditForm | Inventory list and Admin product metadata/photos |
| StockAdjustmentForm | Staff stock change with reason |
| UsersForm / UserEditForm | Admin account management |
| ReportsForm | Admin reports/CSV export |
| StoreToolsForm | Activity & Backups (internal class name retained) |

Each form retains `.cs`, `.Designer.cs` and `.resx`. ProductCard, ProductThumbnail and EmptyStateControl are reusable Designer-editable controls. DashboardChart has a database-free placeholder and constructs ScottPlot only for runtime data.

## One main window

DashboardForm.OpenScreen creates an authorized page, disposes the previous page, sets TopLevel=false, removes its window border and docks it inside pageHost. The sidebar remains in DashboardForm. Login, editors and receipts use ShowDialog. Back from a hosted page returns to Overview.

Management pages use a bottom actionRow FlowLayoutPanel. Buttons wrap; the table reserves room above it. StoreWindow fits large windows to the working area and supplies scrolling. Runtime checks are separate from actual Designer and Windows DPI checks.

## From a click to saved data

```text
Button.Click
  → form reads inputs
  → AppStore refreshes records and verifies role
  → business validation
  → Database.WriteRecords inside transaction
  → commit, close dialog or refresh page
```

For an example, read StockAdjustmentForm.SaveClicked → AppStore.AdjustStock → SaveData → Database.WriteRecords. Failed changes roll back and reload the lists. Merely assigning Part.Stock changes memory, not the database.

PartEditForm.UpdateCategoryFields controls category-specific visibility and positions. Common fields stay visible; socket/memory/graphics/recommended-PSU inputs depend on category. SaveClicked separately excludes hidden values, and stock still comes from saved Staff records. The default Designer retains the controls for visual editing; runtime selects the relevant layout.

## Draft and checkout

DashboardForm owns BuildDraft, a dictionary of category → part ID. ProductCard's Add to build selects that category; selecting another CPU replaces the previous CPU. BuilderForm restores the shared draft after navigation. Draft is not a saved order and clears on sign-out.

CheckBuild always validates required categories, active/available parts, graphics output, CPU/board socket, board/RAM memory type and PSU headroom/recommendations. GPU omission requires verified integrated graphics and board video output. Physical fit/BIOS checks remain limited to staff review.

Guest checkout temporarily hides the main window and opens login. Customer login continues the same checkout. CreateOrder reloads actual saved parts, checks compatibility/stock again and forces ownership to the signed-in Customer. It saves PcBuild, order-line name/price snapshots and the linked CustomerOrder. Receipt opens; the draft is cleared. Cancelling login retains the preview; Admin/Staff login returns to their dashboard.

## Inventory and order lifecycle

```text
Available = On hand − Reserved
Pending Payment → Paid → Processing → Assembly → Ready for Pickup → Completed
```

A reservation begins on order creation. Payment does not deduct physical stock. Completion deducts on-hand stock, writes stock movements and ends reservations. Cancellation/expiry releases reservations without deducting on-hand stock. Customer cancellation is own unpaid pending only; Staff owns payment/progress. Expiry is processed while open and on reopening, never while closed.

## Database schema and location

AppStore uses an explicit constructor path, then CUSTOMPC_DATABASE_PATH, then StoreDataPath.DefaultDatabase. Development uses Database beside CustomPC.slnx; a copied app uses Database beside its executable.

| Table | Stored data |
| --- | --- |
| Users | UserAccount |
| Parts | Part, including PhotoFileName |
| Builds | PcBuild and chosen part IDs |
| Orders | CustomerOrder and order lines |
| Payments | Confirmed payment records |
| Movements | InventoryMovement |
| AuditLog | Timestamp, actor, action and details |
| LoginAttempts | Per-email failures and lockout deadline |

The six record tables use Id + JSON Data, not one SQL column per model property. ReadRecords deserializes JSON; WriteRecords compares records and upserts changes. sqlite_sequence is SQLite's internal support for AuditLog IDs. There are no Settings, Rules or Suppliers feature tables. Startup drops legacy Rules/Suppliers tables after startup backup; old extra JSON fields are ignored by current models.

Default diagnostic logs remain in `%LOCALAPPDATA%/CustomPC/Logs`; a database environment override relocates logs beside that path. Program's single-instance key currently still uses the legacy AppData fallback when no override is set. This is a code detail, not the actual AppStore database location.

## Photos and recovery

Part.PhotoFileName is a filename, not an external path. Admin imports JPG/PNG up to 15 MB; the app stores PNG at most 1600 pixels on the longest edge in ProductImages. Display copies are detached from files and reduced for thumbnails. Missing/unreadable photos use a category placeholder. Failed part saves remove a newly imported copy; older unreferenced files are retained for recovery.

ZIP backup contains custompc.db and ProductImages. Restore validates paths, records and image bytes, saves a before-restore backup, then replaces database/photos. Failed photo replacement rolls the database back. If rollback itself fails, recovery files and paths are retained. Legacy .db restore is supported but has no images. New and automatic backup UI uses ZIP.

## Charts, accounts and fixed identity

Admin sales use local payment date and confirmed amounts; top parts count quantities from orders paid in the period. Staff incoming counts use local creation date. Queue/inventory are current snapshots. Defaults are Admin 30 days and Staff seven days, with 7/30/90 options. ChartSelection maps a day, part, status, category or stock state to filters on already-authorized lists. Account changes dispose cached plots/selections.

Login compares emails case-insensitively, verifies salted PBKDF2-SHA256 password hashes and rejects archived accounts. Five failed attempts lock an email for five minutes. SaveUser enforces Admin access or Customer-only registration. Privileged methods refresh roles before writes. See [ROLES.md](ROLES.md).

Branding is fixed to CustomPC. No business-name/contact-header settings, supplier module or compatibility toggles remain. The researched product manifest and JSON are documentation; they do not seed products at runtime.
