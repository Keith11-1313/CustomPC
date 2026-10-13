# CustomPC code reference

Start with [the guided beginner lessons](CODE_GUIDE.md) if you are new to C# or WinForms. This reference explains the saved source in more detail and supports project-defense preparation. Look up the section related to the feature you are studying; you do not need to read every table first.

All source paths below are relative to the repository folder. The application source is inside `CustomPC/`.

## 1. Start with these five files

1. `CustomPC/Program.cs`: where the application starts.
2. `CustomPC/LoginForm.cs`: a short example of reading inputs and handling a button.
3. `CustomPC/Models.cs`: the records the application works with.
4. `CustomPC/AppStore.Accounts.cs`: the rules behind login and registration.
5. `CustomPC/Database.cs`: how records reach the SQLite file.

After that, read `BuilderForm.cs` with `AppStore.Compatibility.cs`, then `OrdersForm.cs` with `AppStore.Orders.cs`. Use **F12** on a method name in Visual Studio to jump to its definition. Use **Shift+F12** to find places that call it. Use **Ctrl+Shift+F** to search the solution.

## 2. How the files fit together

The main path is:

```text
User clicks a control
    -> Form event handler reads the inputs
    -> AppStore checks the rules and changes records
    -> Database saves those records in SQLite
    -> Form reloads the grid or closes the dialog
```

| Part of the code | What it does | Example |
| --- | --- | --- |
| Forms | Display controls, read input, open dialogs, and show messages | `StockAdjustmentForm.SaveClicked` reads quantity and reason |
| Models | Describe one record and its fields | `Part` has `Name`, `Price`, and `Stock` |
| AppStore | Holds loaded records, the current session, and business rules | `AdjustStock` prevents stock from falling below reservations |
| Database | Opens SQLite and reads/writes records | `WriteRecords<Part>` saves parts |
| Ui | Reuses display and file-export code across forms | `Money`, `ShowError`, `Bind`, `Export`, `Receipt` |
| StoreWindow | Base form shared by all screens; applies private fonts, logo icon and small-screen scrolling | `OnControlAdded`, `OnFontChanged`, `FitToWorkingArea` |
| Branding | Reads the supplied image and bundled font files | `BrandLogo.cs` reads `Assets/logo.png`; `AppFonts.cs` loads Righteous and Roboto |

`AppStore` is a **partial class**. The files named `AppStore.*.cs` are sections of one class, grouped by subject. They are not separate databases or separate services.

| File | Subject | Methods to look for |
| --- | --- | --- |
| `AppStore.cs` | Shared state, initialization, database loading/saving, access checks | `Current`, constructor, `Refresh`, `LoadData`, `SaveData`, `RequireAdmin`, `RequireStaff` |
| `AppStore.Accounts.cs` | Accounts, session, password handling | `Login`, `Logout`, `SaveUser`, `HashPassword`, `VerifyPassword` |
| `AppStore.Inventory.cs` | Parts, suppliers, stock | `Reserved`, `Available`, `StockState`, `SavePart`, `ValidatePart`, `AdjustStock`, `SaveSupplier` |
| `AppStore.Compatibility.cs` | Build checks and configurable rules | `CheckBuild`, `RuleIsEnabled`, `SaveRule` |
| `AppStore.Orders.cs` | Order lifecycle, payment, cancellation | `CreateOrder`, `VisibleOrders`, `ExpireOrders`, `ConfirmPayment`, `AdvanceOrder`, `CancelOrder` |
| `AppStore.SampleData.cs` | Initial demonstration records | `CreateSampleData`, `AddSamplePart`, `AddSampleRule` |

## 3. C# expressions you will see

You can return to this table when an unfamiliar expression interrupts your reading.

| Expression | Meaning in this project |
| --- | --- |
| `namespace CustomPC;` | Groups the project's classes under the same name |
| `public partial class LoginForm : StoreWindow` | Login inherits our small shared window class, which inherits `Form`; its class definition is split across files |
| `public sealed partial class AppStore` | Split class; `sealed` means other classes cannot inherit from it |
| `private` / `public` | Used only inside the class / available to other classes |
| `static` | Belongs to the class itself; call `AppStore.Current` without creating a form-specific store |
| `readonly` | The field cannot be assigned a different object after initialization; the object's contents can still change |
| `new Part()` | Creates a new `Part` object in memory |
| `Name { get; set; }` | A property that code can read and assign |
| `Session { get; private set; }` | Other classes can read the session; only `AppStore` can assign it |
| `List<Part>` | A collection containing `Part` objects; `Add`, `Remove`, and `Count` work on that collection |
| `Part?` / `string?` | The value may be `null`, meaning no object/value is present |
| `decimal? confirmedTotal = null` | Optional amount: it may be omitted, or supplied to check the price the user confirmed |
| `store.Session?.Id` | Read `Id` only when `Session` exists; otherwise produce `null` |
| `Session!.Id` | Tell the compiler a session exists here; `!` does not perform a runtime check |
| `null!` in Designer files | The field starts without a control; `InitializeComponent` assigns the control |
| `foreach (Part part in Parts)` | Visit each saved part, one at a time |
| `continue` / `break` | Skip to the next loop item / stop the loop |
| `return` | Finish the method; optionally send a value back to its caller |
| `==`, `!=`, `&&`, `||` | Equal, not equal, both conditions true, either condition true |
| `condition ? first : second` | Choose one value based on a condition |
| `is Part selectedPart` | Check that an object is a `Part`, then give it the local name `selectedPart` |
| `as UserAccount` | Try treating an object as a `UserAccount`; return `null` if it is another type |
| `(int)quantity.Value` | Convert the numeric control's decimal value into an integer |
| `$"Welcome, {name}"` | Insert a variable's value into text |
| `using System.Data;` | Import a namespace so you can write `DataTable` directly |
| `using (SqliteConnection connection = ...)` | Dispose of the connection when the block ends |
| `using PartEditForm partForm = ...;` | Shorter disposal syntax; dispose when the current scope ends |
| `try` / `catch` | Attempt an operation, then handle an exception if it fails |
| `throw new InvalidOperationException(...)` | Stop the operation with an explanation of why it is invalid |
| `throw;` inside `catch` | Pass the existing exception back to the caller after undoing changes |
| `ReadRecords<T>` | A generic method; `T` becomes the type supplied by the caller, such as `Part` |
| `where T : DatabaseRecord` | Restrict `T` to record classes that inherit the shared `Id` |

`decimal` stores prices and totals. `int` stores quantities and wattage. `bool` stores yes/no values such as `Archived`. `DateTime` stores order and payment times. Saved order times use UTC; forms convert them to local time for display.

## 4. What happens when the app starts

`Program.Main` is the entry point:

```csharp
ApplicationConfiguration.Initialize();

using (LoginForm loginForm = new LoginForm())
{
    if (loginForm.ShowDialog() != DialogResult.OK)
    {
        return;
    }
}

Application.Run(new DashboardForm());
```

The first line sets up WinForms defaults. The `using` block creates and displays login before a dashboard exists. Sign-in and guest access return OK; any other result ends `Main`. After an OK result, `Application.Run` creates the dashboard and starts its message loop, which waits for button clicks, keyboard input, window events, and timer ticks. Closing the main dashboard ends the app.

Follow these steps in `Program.cs`, `LoginForm.cs`, `DashboardForm.cs`, and `AppStore.cs`:

1. `Program.Main` applies WinForms defaults, prevents a second app instance for the same database, and opens the shared store. If no accounts exist, it opens `SetupForm`; cancelling setup ends startup. Successful setup creates the chosen Admin, then login opens.
2. `ShowDialog()` shows only login and waits for successful sign-in, guest access, or cancellation.
3. The first use of `AppStore.Current` creates one shared `AppStore` object. Later calls reuse it for the lifetime of this process.
4. The store constructor chooses the database path, creates missing tables, and loads the saved records into lists.
5. If there are no users and `CUSTOMPC_DEMO_MODE=1` is explicitly set, `CreateSampleData` writes demonstration records. Normal empty stores require `CompleteSetup`, which creates the chosen administrator and three rules without fake stock. Existing records are preserved.
6. `ExpireOrders` cancels overdue unpaid orders.
7. After login or guest access returns `DialogResult.OK`, `Program.Main` creates `DashboardForm`. Cancelling login returns from Main without opening a dashboard.
8. The dashboard constructor creates its controls and connects button, `Shown`, and timer events.
9. `DashboardForm_Shown` calls `UpdateDashboard` to refresh data, show the identity, and enable the appropriate menu buttons, then starts the expiry timer.

Dashboard button handlers call `OpenScreen` with a screen name. That method refreshes saved data, uses a `switch` to construct the requested form, and opens it inside `try/catch`. Keeping construction inside that block means an access or loading error raised by a child form can be displayed instead of escaping the button handler.

The database path is chosen in this order: a path explicitly passed to the `AppStore` constructor, the `CUSTOMPC_DATABASE_PATH` environment variable, then the solution-root `Database\custompc.db`. Normally the last choice is used. See StoreDataPath.cs for how the solution folder is found.

### Session and window lifetime

`Session` is the signed-in `UserAccount` held in memory. `null` represents guest access. `Logout` clears it. The app does not remember a signed-in session after closing.

`Refresh` reloads the database. `LoadData` finds the signed-in account again by its `Id`, so an archived account loses its session and a changed role takes effect in the loaded account. Lists are replaced during refresh; do not assume an old reference is always the latest saved record.

Page forms are hosted by `DashboardForm.OpenScreen`: TopLevel is false and the page fills pageHost. Login, editors and receipts use dialogs. `ShowDialog(this)` opens a modal child window. The owner waits until that dialog closes. A `using` statement disposes of the child afterward. `DialogResult.OK` indicates that the dialog completed its action; it is not an order status.

`DashboardForm.SignIn` handles **Sign out** after startup. It stops the timer, hides the dashboard, clears the current session, and opens login without a dashboard owner. An OK result updates and shows the dashboard, activates it, and restarts the timer. Cancelling login after signing out closes the dashboard. Cancelling Guest Sign in returns to the Guest workspace.

The 17 forms other than Dashboard and Login have a navigation button. **Esc** activates the same button through the form's `CancelButton` property. Back closes a hosted page or focused dialog and returns to its previous screen: catalog to dashboard, an inventory editor to inventory, registration to login, or a receipt to the screen that opened it. The first-time setup screen uses Exit instead, because cancelling setup ends startup. These actions do not save unfinished editor inputs. Records already saved by an earlier action remain saved; closing a receipt after confirming an order does not cancel that order.

Each form connects `back.Click += BackButton_Click;` and sets `CancelButton = back;`. `BackButton_Click` sets `DialogResult.Cancel` and calls `Close()`. This uses the ordinary button declared in that form's Designer file, so you can select and move Back through View Designer. Login is the starting screen, and the dashboard is the main menu; neither has a previous screen to return to. Use Sign out on the dashboard to return to login.

## 5. Designer files and event handlers

Each of the 19 forms has these files:

| File | Edit it for |
| --- | --- |
| `FormName.cs` | Button behavior, reading inputs, validation messages, loading rows |
| `FormName.Designer.cs` | Control declarations and `InitializeComponent`; normally maintained by Visual Studio's visual Designer |
| `FormName.resx` | Resources associated with that form |

Both `.cs` files declare parts of the **same form class**. The `email` field declared in `LoginForm.Designer.cs` is therefore usable in `LoginForm.cs`.

`InitializeComponent` creates controls, assigns their names and text, sets sizes and positions, and adds them to the window. These controls are saved in Designer files. Open a form with **View Designer** or **Shift+F7** to change its layout. Runtime code fills lists, loads grids, changes labels, and enables buttons after the controls exist.

`Ui.IsDesign` checks whether the form is being created by the Designer. Constructors return early in that case so opening a design does not sign in, open a database, or start the normal application workflow. A compiled application and a form opening successfully in Visual Studio Designer are separate checks.

This line in `StockAdjustmentForm.cs` connects a control event to a method:

```csharp
save.Click += SaveClicked;
```

Read it as: **when the save button is clicked, run `SaveClicked`**. `sender` is the control that raised the event. `EventArgs e` contains event information. These handlers do not need to use either argument when reading the form's own controls.

The central action inside that handler is:

```csharp
int stockChange = (int)quantity.Value;
AppStore.Current.AdjustStock(partId, stockChange, reason.Text);
DialogResult = DialogResult.OK;
```

1. Read the numeric input.
2. Call the business method with the selected part and reason.
3. If it succeeds, close the dialog with an OK result.

The surrounding `try/catch` shows a message through `Ui.ShowError` if the method fails. `InventoryForm.AdjustStockClicked` then calls `LoadRows` to rebuild its grid from saved data. This is the usual pattern used by the editor forms.

## 6. Follow a login from button to session

Open `LoginForm.cs`, then `AppStore.Accounts.cs`.

1. `LoginButton_Click` passes `email.Text` and `password.Text` to `AppStore.Current.Login`.
2. `Login` rejects empty input, then calls `Refresh`, so it checks saved accounts rather than relying only on an old list.
3. It loops through `Users`, compares the trimmed email without case sensitivity, and ignores archived accounts.
4. It verifies the entered password against the stored `PasswordHash`.
5. On success, it assigns the account to `Session`.
6. The login handler sets `DialogResult.OK`. At startup, `Program.Main` then creates the dashboard. When switching accounts, `DashboardForm.SignIn` restores the existing dashboard with an updated menu.
7. On failure, the exception reaches the form's `catch` block and its message appears in a warning dialog.

Demo credentials require explicit `CUSTOMPC_DEMO_MODE=1` in an empty practice database. The three legacy `@custompc.local` addresses initially use `CustomPC123!`; an edited or archived account keeps its saved state. Normal setup creates only a personally chosen Admin. Live order creation checks legacy default passwords and blocks orders until an Admin changes or archives those accounts. Login no longer displays public credentials. After five failed attempts for the same normalized email, a five-minute block is saved in `LoginAttempts`; success clears it.

The **Show password** checkbox calls `ShowPassword_CheckedChanged`. It changes masking so you can inspect what was entered; it does not change the account's password. Passwords must match exactly, including letter case, punctuation, and any spaces. The app trims the email's outer spaces but deliberately does not trim the password. If retrying a rejected login, clear the input, type the password carefully, and inspect it with Show password before changing saved account data.

### Registration and account editing

`RegisterForm.SaveButton_Click` checks matching passwords, creates a `UserAccount`, and calls `SaveUser(newCustomer, password.Text, true)`. The last argument requests public registration. The store forces the role to `Customer`, checks for a duplicate email, validates required input, hashes the password, and saves the record.

`UserEditForm.SaveClicked` calls `SaveUser` without the registration argument. That path requires an admin. On an existing account, leaving the password blank preserves its saved hash. Entering a new password replaces it. The signed-in admin cannot archive or demote their own account.

### Why the saved value is a hash

The database stores `PasswordHash`, not the plain password. New hashes use PBKDF2/SHA-256, 600,000 rounds and a random salt, stored as `PBKDF2-SHA256:600000:Base64Salt:Base64Hash`. Verification also accepts the earlier two-field format with 210,000 rounds, preserving existing accounts until their password changes. Normal new/reset passwords require 12–128 characters; the legacy public demo password is refused. Explicit practice mode retains the earlier eight-character minimum for lesson fixtures.

A salt is random data used during hashing. It makes two users with the same password have different saved values. It is not the password and does not need to be hidden. Verification uses the saved salt to hash the entered password again, then compares the result with the saved hash. Base64 is an encoding for bytes, not encryption.

## 7. Records and the links between them

`Models.cs` contains the record classes. `DatabaseRecord` supplies a generated string `Id`. Classes such as `Part` inherit that property.

| Model | Meaning | Links to other records |
| --- | --- | --- |
| `UserAccount` | Name, email, password hash, role, archive flag | Customer, staff, or admin identity |
| `Part` | Component category, specifications, price, physical stock | `SupplierId` identifies a supplier |
| `Supplier` | Distributor details and archive flag | Parts point to its `Id` |
| `CompatibilityRule` | Name, kind, and enabled flag | Kinds are `Socket`, `Memory`, `Power` |
| `PcBuild` | Customer and chosen component IDs | `CustomerId`, `PartIds` |
| `CustomerOrder` | Number, customer, build, deadlines, status, order lines | `CustomerId`, `BuildId` |
| `OrderLine` | Part ID, copied name, quantity, copied price | Embedded inside an order; not its own SQLite table |
| `Payment` | Full order amount, recording staff, paid time | `OrderId`, `StaffId` |
| `InventoryMovement` | Positive or negative stock change and reason | `PartId`, `UserId` |

IDs connect records without depending on names. For example, two parts can have similar names but different IDs. These links are stored inside JSON; the current SQLite schema does not define SQL foreign keys. The application checks references in its own methods.

`OrderLine` copies the part's name and price at order creation. Changing a catalog price later does not change an existing order's receipt. `CustomerOrder.Total` loops through lines and adds `Price * Quantity`.

Archive flags retain a saved record while hiding it from normal active choices. Archiving is different from removing a database row.

## 8. Build selection and compatibility

In `BuilderForm.cs`:

1. `LoadPartChoices` refreshes the store and loads one ComboBox for each category in `AppStore.Categories`.
2. It includes only active parts with available stock.
3. There is no customer selector. The order owner is taken directly from the signed-in session. Staff and Admin cannot open the builder or place orders. Guest access provides a preview but cannot submit an order.
4. `GetSelectedParts` reads the selected `Part` object from each ComboBox.
5. `PartSelectionChanged` calls `UpdateBuildSummary` whenever a choice changes.
6. `UpdateBuildSummary` adds prices, calls `CheckBuild` and displays errors. A valid selection enables Guest checkout/login or Customer ordering.
7. For Guest, `SubmitButton_Click` calls `SignInFromBuilder`; login temporarily hides the builder/dashboard. Cancelling preserves the preview. Customer login refreshes the selected parts/prices and continues the same handler, with no second checkout-button click. It asks about the current total, collects IDs, calls `CreateOrder` with the signed-in Customer ID and confirmed total, then opens the receipt. Both the build and linked order are saved to that account. Invalid refreshed selections return for correction. Staff/Admin return to their dashboard without an order.

If saved prices have changed since the user chose the parts, `CreateOrder` rejects the old confirmed total. `RefreshSelectedParts` reloads current part objects, keeps selections that remain available, recalculates the total, and lets the user review and confirm again. The store also rechecks availability at submission.

`CheckBuild` in `AppStore.Compatibility.cs` returns a `List<string>` of errors. An empty list means its configured checks passed. The checks are:

- Exactly one CPU, motherboard, RAM, storage, power supply and case. A dedicated GPU is optional when the CPU has verified integrated graphics and the motherboard has a recorded video output. At most one dedicated GPU is allowed.
- Every selected part is active and available.
- If the socket rule is enabled, CPU and motherboard sockets must match and be specified.
- If the memory rule is enabled, RAM and motherboard memory types must match and be specified.
- If the power rule is enabled, the PSU capacity must cover the sum of the other components' stored wattages plus 100 W.

Rule names are labels. Rule behavior comes from the hardcoded `Kind` checks in `CheckBuild`. Editing a name does not create a new algorithm. `RuleIsEnabled` controls whether a supported check runs.

These checks cover saved socket, memory, and wattage fields. They do not automatically verify case clearance, BIOS support, physical dimensions, connectors, or every real-world compatibility issue. Staff must verify those details before assembly.

Graphics output is an additional required check, independent of configurable socket/memory/power rules. `Part.HasIntegratedGraphics` applies to CPUs; `Part.HasVideoOutput` applies to motherboards. Admin records them in the component editor. Older JSON records load both as false. The GPU ComboBox contains a string for No dedicated GPU alongside Part objects; `GetSelectedParts` includes only objects that are Parts, so that placeholder adds no price, reservation or receipt line. For example, Intel desktop F models require a discrete GPU according to [Intel's documentation](https://www.intel.com/content/www/us/en/support/articles/000058487/processors.html); do not mark them as having integrated graphics.

## 9. Stock: on hand, reserved, available

The most important formula is in `AppStore.Inventory.cs`:

```csharp
public int Available(Part part)
{
    return part.Stock - Reserved(part.Id);
}
```

- **On hand** is `Part.Stock`: the physical count currently recorded.
- **Reserved** is calculated by `Reserved` from active order lines.
- **Available** is the number another order can still reserve.

Example with one component:

| Event | On hand | Reserved | Available |
| --- | ---: | ---: | ---: |
| Before an order | 10 | 0 | 10 |
| New pending order uses 1 | 10 | 1 | 9 |
| Staff confirms payment | 10 | 1 | 9 |
| Order is processing, assembly, or ready for pickup | 10 | 1 | 9 |
| Customer picks up and order completes | 9 | 0 | 9 |

Cancelling an unpaid order releases its reservation: on hand remains 10 and available returns to 10. The code does not subtract stock when placing the order and again at pickup.

`SavePart` requires Admin and validates catalog edits. A new part starts at zero stock; editing an existing part must preserve its current stock. `AdjustStock` requires Staff, a nonzero quantity, and a reason, and prevents stock from falling below the quantity already reserved. `RecordStockMovement` saves the quantity change, reason, user, and timestamp.

The `MaximumStock` constant limits the saved physical count to 10,000,000 units. Stock adjustment calculates the result using `long` before converting to `int`, so adding a very large number cannot wrap an integer into an invalid quantity.

`StockState` uses **available** quantity. Zero means out of stock. A positive quantity at or below `LowStock` means low stock. The inventory screen displays all three counts so you can see why a part is unavailable despite physical units still being present.

## 10. Follow an order to completion

`CreateOrder` reloads saved data inside a transaction and checks the selection again. The store performs this second validation because a UI preview may have become outdated before the user confirms.

It verifies the session and target customer, finds the selected parts, runs `CheckBuild`, and compares current prices with the optional confirmed total. It then creates a `PcBuild`, creates a `CustomerOrder`, and copies the selected parts into `OrderLine` objects. The deadline is creation time plus 24 hours. Adding the pending order makes `Reserved` count its lines.

| Status | How it is reached | Holds a reservation? |
| --- | --- | --- |
| `Pending Payment` | `CreateOrder` | Yes |
| `Paid` | Staff calls `ConfirmPayment` before the deadline | Yes |
| `Processing` | `AdvanceOrder` from Paid | Yes |
| `Assembly` | `AdvanceOrder` from Processing | Yes |
| `Ready for Pickup` | `AdvanceOrder` from Assembly | Yes |
| `Completed` | `AdvanceOrder` records pickup and deducts physical stock | No |
| `Cancelled` | `CancelOrder` or expiry of unpaid order | No |

`ConfirmPayment` records the full amount in a `Payment` and changes status to `Paid` in the same transaction. This records payment at the physical store; it does not contact an online payment service.

`AdvanceOrder` uses a `switch` to allow one stage at a time. At completion, `DeductCompletedOrderStock` reduces `Part.Stock` and records negative inventory movements. A completed order cannot be completed again through that method.

`CancelOrder` allows only the order's own Customer to cancel an unpaid pending order. Staff and Admin are denied manual cancellation. Paid and completed orders cannot be cancelled through this method. `VisibleOrders` returns all orders for Staff/Admin and only the signed-in Customer's own orders for a Customer.

In `OrdersForm.cs`, `OrderSelectionChanged` calls `UpdateOrderActions`. The signed-in role decides which buttons are visible. The selected row and its saved status decide which visible buttons are enabled: Staff can pay an unexpired pending order or advance a paid/in-progress order; Customer can cancel their own unexpired pending order. Receipt requires a selected visible order. Completed/cancelled orders cannot enable payment, progress or cancellation. `LoadRows` preserves the selected order after an action if it still matches the filters, so Staff can continue to the next stage. AppStore still rechecks permission and status inside the transaction.

### Deadline and timer

`ExpireOrders` finds unpaid orders whose deadline has passed. `CancelExpiredOrders` changes their status to Cancelled. It does not delete the order or subtract physical stock.

The dashboard and orders screen have 30-second WinForms timers that refresh records. Startup, refresh, order creation, and payment checks also participate in deadline handling. A WinForms timer runs while the app's UI message loop is running; it is not a server job. If the application is closed, the saved deadline remains, and overdue unpaid orders are cancelled when the application next loads/refreshes them. Cancellation is therefore not guaranteed to happen at the exact deadline while the app is closed.

## 11. SQLite loading and transactions

SQLite is a database in a local file. `Database.OpenConnection` builds a connection string and opens that file. There is no SQL Server process or internet request in this path.

`CreateTables` preserves eight business tables: `Users`, `Parts`, `Suppliers`, `Rules`, `Builds`, `Orders`, `Payments`, and `Movements`. Each has this shape:

```sql
CREATE TABLE IF NOT EXISTS Parts (
    Id TEXT PRIMARY KEY,
    Data TEXT NOT NULL
);
```

`Data` is JSON text containing the record's properties. Conceptually, one part row looks like this:

```text
Id   = a generated unique ID
Data = {"Id":"same ID","Name":"Ryzen 5 5600","Category":"CPU",...}
```

The `...` above abbreviates the other `Part` properties; it is not text the application writes. `JsonSerializer.Serialize(record)` converts an object into JSON. `Deserialize<Part>(savedJson)` reconstructs a `Part` object. `ReadRecords<UserAccount>(connection, "Users")` does the same job for users through the generic type parameter.

This means `Name` and `Stock` are JSON fields, not ordinary SQL columns. To inspect parts in a SQLite editor, a read-only query is:

```sql
SELECT Id,
       json_extract(Data, '$.Name') AS Name,
       json_extract(Data, '$.Stock') AS OnHand
FROM Parts;
```

### The save pattern

Business methods follow a common sequence:

1. Open a connection and begin a transaction.
2. Reload current saved records.
3. Check permissions and validate the requested operation.
4. Change the in-memory records.
5. Call `SaveData` to persist changed records, add the activity entry and commit.
6. If an exception occurs, `UndoFailedChange` rolls back the transaction and reloads saved records, then the exception returns to the form.

A transaction makes related writes succeed together. For example, confirming payment must save both a payment record and the Paid order status. It should not save just one of them after an error.

`Database.WriteRecords` reads saved JSON, compares it with the current records and skips identical rows. New or changed records use `INSERT ... ON CONFLICT(Id) DO UPDATE`; removed IDs are deleted individually. `SaveData` applies this to the eight business lists and appends a successful activity entry in the same transaction. It still loads and compares lists, so this is a local store application rather than a database server for multiple computers. `CallerMemberName` supplies the business method's name to `SaveData` for the audit action without every caller repeating it.

Three supporting tables hold settings, successful activity and temporary failed-login blocks: `Settings`, `AuditLog` and `LoginAttempts`. `PRAGMA user_version=1` identifies the schema; newer schemas are refused. `AppStore.Maintenance.cs` handles first setup, store identity and recovery. Existing files get a daily startup backup before schema initialization. Admin backup uses SQLite's online snapshot API and validates the output. Restore validates business records, requires the current Admin password and preserves a safety backup before replacing data. Full operating steps are in [STORE_GUIDE.md](STORE_GUIDE.md).

Values use SQL parameters (`$id` and `$data`) rather than being placed directly into SQL command text. Table names come from fixed application calls. Manage records through the app; direct SQLite edits bypass account, stock and order rules. Back up before direct edits. Deleting the database removes saved records and triggers first-store setup on the next normal launch.

## 12. Every form and what to read

| Form source | Screen and main responsibilities | Methods to follow |
| --- | --- | --- |
| `DashboardForm.cs` | Main navigation, role menus, metrics, account switching, expiry refresh | `SignIn`, `UpdateDashboard`, `OpenScreen`, `ExpiryTimer_Tick` |
| `SetupForm.cs` | First administrator for CustomPC | `SaveButton_Click` → `CompleteSetup` |
| `StoreToolsForm.cs` | Store details, backup/restore, Admin activity history | `SaveButton_Click`, `BackupButton_Click`, `RestoreButton_Click`, `LoadHistory` |
| `LoginForm.cs` | Login, show password, register button, guest access | `LoginButton_Click`, `ShowPassword_CheckedChanged`, `RegisterButton_Click`, `GuestButton_Click` |
| `RegisterForm.cs` | Create a customer account | `SaveButton_Click` |
| `CatalogForm.cs` | Search/filter active components and inspect details | `LoadRows`, `PartsGrid_CellDoubleClick`, `FilterChanged` |
| `BuilderForm.cs` | Choose required components and optional GPU, calculate total, create own order | `LoadPartChoices`, `GetSelectedParts`, `UpdateBuildSummary`, `SubmitButton_Click`, `RefreshSelectedParts` |
| `OrdersForm.cs` | Search/filter orders, view receipt, pay, advance, cancel | `LoadRows`, `PayButton_Click`, `AdvanceButton_Click`, `CancelButton_Click` |
| `ReceiptForm.cs` | Display, save a text receipt, print it | Constructor with ID, `SaveButton_Click`, `PrintReceipt`, `PrintDocument_PrintPage` |
| `InventoryForm.cs` | View on-hand/reserved/available stock; open editors/history | `LoadRows`, `EditClicked`, `AdjustStockClicked`, `HistoryClicked` |
| `PartEditForm.cs` | Add/edit catalog details and archive flag | Constructor with ID, `SaveClicked` -> `SavePart` |
| `StockAdjustmentForm.cs` | Record a stock change with a reason | `SaveClicked` -> `AdjustStock` |
| `SuppliersForm.cs` | Search suppliers and list their components | `LoadRows`, `EditClicked`, `SuppliedPartsClicked` |
| `SupplierEditForm.cs` | Add/edit supplier contact and archive flag | Constructor with ID, `SaveClicked` -> `SaveSupplier` |
| `UsersForm.cs` | Search active/archived user accounts | `LoadRows`, `AddClicked`, `EditClicked` |
| `UserEditForm.cs` | Add/edit account, role, password, archive flag | Constructor with ID, `SaveClicked` -> `SaveUser` |
| `CompatibilityForm.cs` | View and select the supported rules | `LoadRows`, `EditClicked` |
| `RuleEditForm.cs` | Edit name/kind/enabled flag | Constructor with ID, `SaveClicked` -> `SaveRule` |
| `ReportsForm.cs` | Admin-only inventory, orders, movements and sales reports; CSV export | `LoadReport`, report-specific loaders, `IsWithinDateRange`, `ExportClicked` |

Most list screens build a `DataTable` with columns meant for display, loop over models, add rows, and call `Ui.Bind`. The table is a screen representation, not another SQLite database. The hidden `Id` column lets a selected display row identify the correct saved record. `Ui.RequireSelection` reports an error when no row is selected.

Most editors have two constructors: a parameterless one for adding/designing, and one that accepts an existing record ID for editing. The editing constructor uses `: this()` to run the normal setup first, then fills controls with the saved record. On save, keeping the old `Id` updates that record through the store instead of creating an unrelated one.

`PartEditForm` keeps the current supplier in its choices even if that supplier has been archived. Saving an unrelated part edit preserves that historical association; a newly assigned supplier must be active. Its `PartEditForm_Shown` handler fits the editor to a short display, and scrolling makes fields lower in the form reachable.

### Roles

| Action | Guest | Customer | Staff | Admin |
| --- | --- | --- | --- | --- |
| Browse catalog | Yes | Yes | Yes | Yes |
| Preview builder | Yes | Yes | No | No |
| Place an order | No | Own account | No | No |
| View receipts/orders | No | Own orders | All orders | Read-only |
| Cancel pending unpaid order | No | Own order | No | No |
| Confirm store payment and advance order | No | No | Yes | No |
| View inventory | No | No | Yes | Yes |
| Adjust stock | No | No | Yes | No |
| Add/edit part catalog details | No | No | No | Yes |
| Manage suppliers, users, compatibility rules | No | No | No | Yes |
| Inventory/orders/movements reports | No | No | No | Yes |
| Sales report | No | No | No | Yes |

`IsStaff` means exactly Staff. `IsAdmin` and `IsCustomer` also match only their own roles. Shared read access is explicit through `CanViewInventory` and `CanViewAllOrders`. The dashboard's enabled buttons communicate access, and business methods such as `RequireStaff`, `RequireCustomer` and `RequireAdmin` enforce changes again after loading the saved account. Admin does not inherit Staff/Customer actions. See [ROLES.md](ROLES.md) for assignment steps and the strict split approved for this revision.

Privileged list screens call their `ValidateAccess` helper after refreshing saved data. If an account has lost access while its screen is open, the helper clears the displayed grid, disables actions, and raises a readable message. Orders refreshes action availability from the current role, selected order and status. Report export reloads the report and checks access before saving, including the admin requirement for Sales.

### Receipts and exports

`Ui.Receipt` creates the receipt text from order-line snapshots and payment records. `ReceiptForm.RefreshReceipt` checks that the order is in `VisibleOrders` before displaying, saving or printing it. Losing access clears the receipt and disables saving/printing. Save writes a `.txt` file. Print uses `PrintDocument` and a Windows print dialog; `PrintDocument_PrintPage` continues on another page when the receipt reaches the bottom margin.

`Ui.Export` writes the visible report grid columns into CSV. It doubles quote characters inside values, encloses values in quotes, and prefixes values starting with spreadsheet formula markers. Reports filter UTC timestamps after converting to local dates. Inventory reports describe current inventory, so their date controls are disabled. Orders reports use creation dates; sales reports use payment dates; movement reports use movement dates. An order's value and a confirmed payment's sales amount are different measurements.

## 13. Find a bug by following one operation

When something fails, first write down the exact action, account role, message, and selected record. Then follow the handler rather than reading unrelated files.

| Problem | First place to inspect | Next place |
| --- | --- | --- |
| App starts in the wrong window | `Program.Main`: login and result check | `LoginForm` result, then `Application.Run(new DashboardForm())` |
| Account switching shows the wrong window | `DashboardForm.SignIn` | `Hide`, login result, `UpdateDashboard`, `Show` |
| Login rejects an account | `LoginButton_Click` | `AppStore.Login`, `VerifyPassword`, selected database path, saved account's archive status |
| Registration fails | `RegisterForm.SaveButton_Click` | `SaveUser` |
| Wrong catalog rows or filter | `CatalogForm.LoadRows` | `Available`, `StockState` |
| Builder submit disabled | `UpdateBuildSummary` | `CheckBuild`, session and customer choices |
| Order creation fails | `SubmitButton_Click` | `CreateOrder`, `FindActiveCustomer`, `CheckBuild` |
| Stock seems wrong | `InventoryForm.LoadRows` | `Reserved`, `Available`, `AdjustStock`, `DeductCompletedOrderStock` |
| Payment or status cannot change | `OrdersForm` button handler | `ConfirmPayment`, `AdvanceOrder`, current status/deadline |
| Data disappears after closing | `AppStore.DatabasePath` | `SaveData`, `Database.WriteRecords`, environment path override |
| CSV or receipt text is wrong | `Ui.Export` or `Ui.Receipt` | Report loader or order's saved `Lines` |
| Designer does not open | Form's `.Designer.cs` and `.resx` | Matching `partial` class names, `InitializeComponent`, `Ui.IsDesign`, build diagnostics |

In Visual Studio, click beside a handler to add a breakpoint, then run with **F5** and click the button. Use **F10** to execute a line without entering its method, **F11** to enter a called method, and hover over variables to inspect values. Good starting breakpoints are `LoginButton_Click`, `CreateOrder`, and `SaveData`.

If an exception is shown only as a message box, inspect the exception in the debugger for its type and stack trace. The stack trace lists the methods that led to the failure. Avoid printing entered passwords or saved password hashes while debugging.

The `bin/` and `obj/` folders are generated build outputs. They are not the files you study or edit to change behavior. `CustomPC.csproj` defines the Windows Forms target and package references; `CustomPC.slnx` tells Visual Studio which project to open.

## 14. Manual walkthrough you can repeat

This is a checklist with expected behavior, **not a claim that these steps have all been performed on your machine**. Use separate demonstration records when trying payments, stock changes, and cancellation. Record actual results when validating the app.

1. Open `CustomPC.slnx`, build, and run. Expect only login at startup. Close login once and verify the app exits without opening a dashboard. Run again, check the selected database path, and sign in with an active account. A wrong password must show a message and keep login open. After success, expect the dashboard. Switch accounts and verify the dashboard stays hidden until sign-in or guest access succeeds.
2. Open the same forms in **View Designer**. Confirm controls appear, then return to code view. Designer loading must be checked separately from compilation.
3. Browse as Guest. Search and filter the catalog; double-click a component. Use Back to return to the dashboard, reopen the catalog, and use Esc to return again. Expect Sign in on the Guest homepage. Choose a valid build, then Confirm must open login. Cancelling preserves the preview without an order. Customer login continues that same checkout: check the saved build's parts, Customer ID, linked order and receipt without clicking checkout again. Staff/Admin login returns to their dashboard without an order. Check Back/Esc in the remaining screens; unfinished editor inputs must not change saved records, and leaving a receipt must keep its already-created order intact.
4. Register a new customer. Try mismatched passwords and a duplicate email before using valid details. Sign in with the newly created account, close/reopen the app, and sign in again to verify persistence.
5. Choose Ryzen 5 5600 with B550M motherboard and DDR4 RAM, plus GPU, storage, the 650 W PSU, and case. Check total and compatibility messages. Change to the Intel CPU while retaining the AM4 motherboard; submit must become unavailable with a socket message. Try DDR5 RAM with the DDR4 motherboard for a memory message.
6. Submit a valid order, retain its number, and save the receipt. Verify Pending Payment, seven order lines, correct total, and a 24-hour deadline. In inventory, reserved increases while on hand stays unchanged.
7. Sign in as another customer. The first customer's order and receipt must not be visible. Sign in as staff: all orders should be visible, payment/status actions should be available, and admin-only account/supplier/rule management should remain unavailable.
8. As staff, confirm payment once. Verify one full-amount payment and Paid status. Advance through Processing, Assembly, Ready for Pickup, and Completed. At completion, each component's on hand and reserved count drops by one, with a recorded negative movement. Repeating completion must be rejected.
9. Sign in as Customer, create another pending order and cancel it. Verify the reservation is released without decreasing on-hand stock. Paid orders must reject cancellation. Staff and Admin must be denied manual cancellation and order creation.
10. Sign in as Staff and test stock adjustment on a demonstration part: positive adjustment with a reason, zero adjustment, blank reason, and an attempt to reduce stock below reservations. Only valid adjustments should persist. Sign in as Admin to inspect the stock movements report. Admin must be denied stock adjustments, including a changed quantity sent through catalog saving.
11. As admin, edit a demonstration part, supplier, account, and rule. Try invalid input and cancel an editor before saving. Reopen the screen/application to check that saved changes persist and cancelled edits do not.
12. As Admin, open reports with a known date range. Compare inventory, order, movement, and sales summaries to the underlying test records. Export CSV and inspect it in a text editor or spreadsheet. Staff/Customer must be denied these reports. Save a receipt and inspect its lines. Test printing only with an available printer or a chosen PDF printer.
13. Check deadline cancellation with an isolated test database and a deliberately expired pending test order. Opening/refreshing the app must mark it Cancelled and release its reservations. Normal testing does not require waiting 24 hours or editing real orders.

Build success checks that the code compiles. Business checks exercise rules. Runtime interaction checks that users can complete flows. Designer checks confirm editable forms. Persistence checks require reopening saved data. Printer output and the closed-app deadline limitation each need their own evidence.

## 15. Explain the project during a defense

You can use this explanation in your own words:

> CustomPC is a Windows Forms application for selecting a PC build and managing store orders. Forms handle user interaction. Models describe users, parts, orders, payments, and inventory records. A shared AppStore applies role, compatibility, stock, and order rules. Database saves those records in a local SQLite file. An order reserves stock first, staff records the physical-store payment and assembly stages, and pickup completion deducts physical inventory. The form layout is kept in Designer files so it remains editable in Visual Studio.

For a concrete demonstration, explain `StockAdjustmentForm.SaveClicked` and `AppStore.AdjustStock` together, then show the resulting movement entry. For the main feature, explain `BuilderForm.SubmitButton_Click` -> `CreateOrder` -> `SaveData`. Those two paths illustrate most of the project's structure.

CustomPC branding is fixed. Setup and Store tools do not offer business-name, receipt-header or business-contact customization.
