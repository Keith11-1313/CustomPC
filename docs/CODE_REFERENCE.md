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
| StoreTextField.cs / StoreNumberField.cs / StoreDateField.cs | Padded fields around native text, numeric and date controls |
| StoreComboBox.cs / StoreCheckBox.cs / IconAssets.cs | Dropdown rows, checkbox surfaces and Lucide PNG icons |
| PasswordEyeButton.cs | Clickable eye/eye-off button inside password fields |
| StoreDialog.cs | App message and confirmation modal |
| Ui.cs | Prices, grids, receipts, CSV/text export and validation messages |
| AppLog.cs | Unexpected-error diagnostic references |

`AppStore.*.cs` are parts of one `partial` class. `AppStore.Current` creates it lazily. Forms call business methods; they do not write SQL directly.

## The 16 forms

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
| StoreDialog | Shared messages, confirmations and product specifications |

Each form retains `.cs`, `.Designer.cs` and `.resx`. ProductCard, ProductThumbnail and EmptyStateControl are reusable Designer-editable controls. DashboardChart has a database-free placeholder and constructs LiveCharts2 only for runtime data.

## One main window

DashboardForm.OpenScreen creates an authorized page, disposes the previous page, sets TopLevel=false, removes its window border and docks it inside pageHost. The sidebar remains in DashboardForm. Login, editors and receipts use ShowDialog. Back from a hosted page returns to Overview.

Management pages use a bottom actionRow FlowLayoutPanel. Buttons wrap; the table reserves room above it. StoreWindow fits large windows to the working area and supplies scrolling. Runtime checks are separate from actual Designer and Windows DPI checks.

DashboardForm.FitWorkspace keeps the sidebar fixed and positions the hosted page within the visible client area. The main window starts maximized; StoreWindow leaves maximized borders to Windows and clamps restored windows to usable monitor bounds. Hosted pages no longer inherit a fixed 650-pixel scrolling minimum.

BuilderForm.FitBuilder switches to a stacked checkout layout when the page narrows. selectionScroll owns vertical scrolling; checkout stays outside that scroll area. LoadPartChoices rebuilds lists in component order, preserves IDs still offered, and clears incompatible downstream IDs. CPU socket filters motherboards; board memory type filters RAM; AppStore.RequiredPowerSupplyWatts supplies the same wattage rule used at checkout. These filters do not replace CheckBuild or physical fit/BIOS review. They do not rank performance or automatically choose a "best" product.

## From a click to saved data

```text
Button.Click
  â†’ form reads inputs
  â†’ AppStore refreshes records and verifies role
  â†’ business validation
  â†’ Database.WriteRecords inside transaction
  â†’ commit, close dialog or refresh page
```

For an example, read StockAdjustmentForm.SaveClicked â†’ AppStore.AdjustStock â†’ SaveData â†’ Database.WriteRecords. Failed changes roll back and reload the lists. Merely assigning Part.Stock changes memory, not the database.

PartEditForm.UpdateCategoryFields controls category-specific visibility and positions. Common fields stay visible; socket/memory/graphics/recommended-PSU inputs depend on category. SaveClicked separately excludes hidden values, and stock still comes from saved Staff records. The default Designer retains the controls for visual editing; runtime selects the relevant layout.

## Draft and checkout

DashboardForm owns BuildDraft, a dictionary of category â†’ part ID. ProductCard's Add to build selects that category; selecting another CPU replaces the previous CPU. BuilderForm restores the shared draft after navigation. Draft is not a saved order and clears on sign-out.

CheckBuild always validates required categories, active/available parts, graphics output, CPU/board socket, board/RAM memory type and PSU headroom/recommendations. GPU omission requires verified integrated graphics and board video output. Physical fit/BIOS checks remain limited to staff review.

Guest checkout temporarily hides the main window and opens login. Customer login continues the same checkout. CreateOrder reloads actual saved parts, checks compatibility/stock again and forces ownership to the signed-in Customer. It saves PcBuild, order-line name/price snapshots and the linked CustomerOrder. Receipt opens; the draft is cleared. Cancelling login retains the preview; Admin/Staff login returns to their dashboard.

## Inventory and order lifecycle

```text
Available = On hand âˆ’ Reserved
Pending Payment â†’ Paid â†’ Processing â†’ Assembly â†’ Ready for Pickup â†’ Completed
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

### Shared appearance

StoreTextField wraps a normal TextBox in a padded, rounded surface. Text, masking, maximum length and multiline receipt behavior are forwarded to that inner control; the TextChanged event is forwarded to the form. StoreNumberField and StoreDateField do the same for numeric entry and the Windows calendar. Their values still go through the existing business validation.

StoreComboBox keeps the Windows popup and keyboard selection, but draws rows with padding and a visible selected state. StoreCheckBox keeps CheckBox behavior and uses a downloaded check icon. IconAssets loads transparent 192×192 Lucide PNGs from embedded Assets/Icons resources. System.Drawing reuses the loaded bitmaps, scales them to the control and tints them for its colour. No SVG package or runtime conversion is needed. Source URLs/hashes and the full license remain in that folder. No icon download happens at runtime. StoreDialog replaces app confirmations/messages; Ui.Confirm returns true only for Confirm. Closing or cancelling it does not approve an action. Windows file and print dialogs remain native.

StoreTextField.ShowPasswordToggle enables PasswordEyeButton. It toggles UseSystemPasswordChar without changing Text, preserves selection, and updates its accessible name between Show password and Hide password. The old login checkbox and its handler were removed. Login, registration, setup, account editing and backup-restore password fields use this control.

StoreWindow calls StoreTheme.Apply after a runtime form loads and keeps direct form headers and their Back buttons within the current width. StoreTheme draws rounded buttons and their hover, pressed, disabled and focus states; it also styles tables. Ui.Bind reapplies table styling after new records are bound. Button styling changes drawing only: the original Click handlers, TabIndex, Enabled state and role checks still control behavior.

ProductCard.Designer.cs owns photo, name, price, stock and action placement. CatalogForm.FitProductCards distributes card widths across the available row. BuilderForm.Designer.cs keeps component selection and checkout in separate white panels. LoginForm.Designer.cs owns the sign-in layout. EmptyStateControl uses the downloaded inbox icon. DashboardChart uses white plot surfaces and readable day labels; chart selection continues to use the original saved dates.

These helpers do not open the database in Designer. Layout controls remain in separate Designer files; actual Visual Studio Designer loading and Windows DPI checks must still be verified independently.

Admin sales use local payment date and confirmed amounts; top parts count quantities from orders paid in the period. Staff incoming counts use local creation date. Queue/inventory are current snapshots. Defaults are Admin 30 days and Staff seven days, with 7/30/90 options. ChartSelection maps a day, part, status, category or stock state to filters on already-authorized lists. Account changes dispose cached plots/selections.

Login compares emails case-insensitively, verifies salted PBKDF2-SHA256 password hashes and rejects archived accounts. Five failed attempts lock an email for five minutes. SaveUser enforces Admin access or Customer-only registration. Privileged methods refresh roles before writes. See [ROLES.md](ROLES.md).

Branding is fixed to CustomPC. No business-name/contact-header settings, supplier module or compatibility toggles remain. The researched product manifest and JSON are documentation; they do not seed products at runtime.

DashboardChart uses LiveCharts2 at runtime. Line creates a LineSeries, Bars chooses RowSeries or ColumnSeries, Donut creates PieSeries, and StackedStock creates three StackedRowSeries for stock states. Prepare disposes the previous chart and chooses the empty state; ShowChart installs the control and its keyboard selection list. Point events use the same ChartSelection objects as Open selected. Charts never calculate sales or decide permissions: AppStore.Dashboards supplies role-checked saved-data totals. The Designer only instantiates the ordinary panel and placeholder.
