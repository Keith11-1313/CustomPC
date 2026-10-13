# CustomPC Windows Forms

This project is **under active revision**. Production-quality code is the goal; it is not a finished or deployed store product. There is no release package in this repository.

Open `CustomPC.slnx` in Visual Studio with the **.NET desktop development** workload and .NET 10 installed. Restore NuGet packages if prompted, then press **F5**.

For testing the store workflows, read [the store guide](docs/STORE_GUIDE.md). A new empty database opens **Set up your store** first: choose your own administrator and a password of at least 12 characters. Sample accounts and stock are created only when `CUSTOMPC_DEMO_MODE=1` is explicitly set. Existing records are preserved.

After setup, login appears first. Sign in or choose **Browse as guest** to open the homepage. Closing login exits the app. **Back** or **Esc** returns without saving unfinished editor inputs. Admins have **Store tools & backups** for receipt details, verified backups, recovery and activity history.

Read [who can do what](docs/ROLES.md) for the strict permission matrix and steps to assign roles. Admin manages the system and monitors orders; Staff changes stock and records payments/progress; Customers place and cancel only their own unpaid orders. Admin does not inherit Staff or Customer actions.

On this computer, `jerald@gmail.com` is Admin and keeps its existing password. The old public-password demo accounts were archived; add your own Staff and Customer accounts through User accounts. This is saved account data, not an Admin email hardcoded into the application.

The builder uses the signed-in Customer automatically. Choose CPU, motherboard, RAM, storage, PSU and case. A dedicated GPU can be omitted only when the catalog records CPU integrated graphics and motherboard video output. Admin sets those two verified fields in the part editor; older records default to false until checked.

## Understand the code

Start with [the beginner code guide](docs/CODE_GUIDE.md). Lessons 1–5 show you how to open a form, identify its controls, read Sign in's code, and watch it run in the debugger. Each lesson gives an action, the expected result, and a checkpoint with an answer. Later lessons cover accounts, the builder, and saving with a separate practice database.

Use [the code reference](docs/CODE_REFERENCE.md) afterward for all 19 forms, the C# lookup table, roles, order stages, SQLite details, and the advanced manual checklist.

The guide explains expected behavior. A successful build alone does not verify login against your saved database, runtime workflows, Visual Studio Designer loading, or printer output.

Read [the validation results](docs/VALIDATION.md) for executed checks and remaining manual checks. The latest source run passed 667 checks with 0 failures, including 153 strict-role checks and real font-family checks on all 19 forms. Testing does not mark the product finished. Actual printer, Designer, Windows scaling and external-drive recovery checks remain open.

Read [the role screen audit](docs/SCREEN_VALIDATION.md) for the actual dashboard routes, buttons tested per role, normal store workflow and disabled-action explanation. Its separate 627 checks include a correction for payment/progress buttons on completed orders.

Pressing F5 generates `bin` and `obj` folders and a debug `.exe`; those are normal temporary build outputs used to run your code. They are ignored by Git and are not a release. The earlier published EXE folder and ZIP have been removed. Validation builds are kept outside this project.

## Edit a screen

Right-click a form such as `LoginForm.cs` or `BuilderForm.cs` and choose **View Designer** (**Shift+F7**).

- `FormName.cs`: button clicks, reading inputs, and opening other screens. Write application code here.
- `FormName.Designer.cs`: controls, sizes, positions, and other layout settings maintained by Visual Studio.
- `FormName.resx`: resources belonging to the form.

These files belong to the same form. Use the visual Designer to change its layout.

## Where the code lives

| File | Responsibility |
| --- | --- |
| `Program.cs` | Opens login first; opens the dashboard only after sign-in or guest access. |
| `DashboardForm.cs` | Shows the menu for the user's role and handles switching accounts. |
| `LoginForm.cs`, `RegisterForm.cs` | Sign-in, registration, and guest access. |
| `CatalogForm.cs`, `BuilderForm.cs` | Browse parts, choose components, check compatibility, and create an order. |
| `OrdersForm.cs`, `ReceiptForm.cs` | Track orders, confirm store payment, update status, and print receipts. |
| Inventory, supplier, user, compatibility, and report forms | Staff and admin screens. Each screen keeps its own event handlers. |
| `Models.cs` | Record classes such as `Part`, `UserAccount`, and `CustomerOrder`. |
| `AppStore.cs` | Shared records, the signed-in user, loading, and saving. |
| `AppStore.Accounts.cs` | Account validation, roles, and passwords. |
| `AppStore.Inventory.cs` | Parts, suppliers, available stock, and stock adjustments. |
| `AppStore.Compatibility.cs` | Socket, memory, and power checks. |
| `AppStore.Orders.cs` | Order creation, reservations, payments, status changes, and expiry. |
| `AppStore.SampleData.cs` | Initial demo accounts and sample parts. |
| `AppStore.Maintenance.cs` | First store setup, receipt details, backups, restore, activity and login throttling. |
| `StoreWindow.cs` | Shared form base that provides scrolling on a smaller display. |
| `BrandLogo.cs` | Displays your supplied `Assets/logo.png` and uses it for the window icon. |
| `AppFonts.cs` | Loads bundled Righteous and Roboto fonts for this process without installing them in Windows. |
| `AppLog.cs` | Local diagnostic references for unexpected errors. |
| `Database.cs` | Opens SQLite and runs the SQL commands. |
| `Ui.cs` | Display helpers for errors, prices, grids, exports, and receipts. |

The `AppStore.*.cs` files are sections of **one class**, declared with `partial`. They share the same fields and methods; each file groups one subject. Forms access that shared object through `AppStore.Current`.

Suggested reading order for later: **Program → LoginForm → Models → CatalogForm → BuilderForm → OrdersForm → Database**. Open the relevant `AppStore` section when a form calls a method such as `Login` or `CreateOrder`.

## How a button works

A named event handler is a method called when the user clicks a button. For example, `StockAdjustmentForm.cs` connects the save button with `save.Click += SaveClicked;`. Inside that method:

```csharp
int stockChange = (int)quantity.Value;
AppStore.Current.AdjustStock(partId, stockChange, reason.Text);
DialogResult = DialogResult.OK;
```

The usual flow is **click → read inputs → validate → save → refresh**. Here the form reads the quantity, `AdjustStock` validates and saves it, the dialog closes, and `InventoryForm` reloads its grid. A `try/catch` around the handler displays a readable error if the operation fails.

## Initial demo accounts

These credentials work only in explicitly enabled practice mode (`CUSTOMPC_DEMO_MODE=1`). Normal store setup does not create them. Normal mode refuses this public password at login. Your own Customer account can place orders independently of unused sample records. Existing sample stock must be reviewed separately.

| Role | Email | Password |
| --- | --- | --- |
| Admin | admin@custompc.local | CustomPC123! |
| Staff | staff@custompc.local | CustomPC123! |
| Customer | customer@custompc.local | CustomPC123! |

Existing accounts retain saved password changes and archive status. If a demo email rejects the initial password, check which database is being opened and the account's saved state before changing or deleting data. Guest access is available from the login screen.

## Saved data and order deadline

The local SQLite file is `%LOCALAPPDATA%\CustomPC\custompc.db`, normally `C:\Users\Jerald\AppData\Local\CustomPC\custompc.db`. Closing the app keeps your records.

Each table has `Id` and `Data` columns. `Data` contains the record's fields as JSON. This format is unchanged, so existing records remain usable. Manage records through the app's admin and staff screens.

Orders have a 24-hour payment deadline. The desktop app cancels expired unpaid orders during refresh and while its timer is running. If the app is closed, cancellation happens when it next opens. Store payment and pickup are recorded by staff.
