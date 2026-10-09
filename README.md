# CustomPC Windows Forms

Open `CustomPC.slnx` in Visual Studio with the **.NET desktop development** workload and .NET 10 installed. Restore NuGet packages if prompted, then press **F5**.

## Understand the code

Start with [the beginner code guide](docs/CODE_GUIDE.md). Lessons 1–5 show you how to open a form, identify its controls, read Sign in's code, and watch it run in the debugger. Each lesson gives an action, the expected result, and a checkpoint with an answer. Later lessons cover accounts, the builder, and saving with a separate practice database.

Use [the code reference](docs/CODE_REFERENCE.md) afterward for all 17 forms, the C# lookup table, roles, order stages, SQLite details, and the advanced manual checklist.

The guide explains expected behavior. A successful build alone does not verify login against your saved database, runtime workflows, Visual Studio Designer loading, or printer output.

Read [the validation results](docs/VALIDATION.md) for the checks that actually ran, corrected issues, and remaining manual checks. The 9 October run passed 261 automated checks and 3 separate login-button checks against a copy of the saved database.

## Edit a screen

Right-click a form such as `LoginForm.cs` or `BuilderForm.cs` and choose **View Designer** (**Shift+F7**).

- `FormName.cs`: button clicks, reading inputs, and opening other screens. Write application code here.
- `FormName.Designer.cs`: controls, sizes, positions, and other layout settings maintained by Visual Studio.
- `FormName.resx`: resources belonging to the form.

These files belong to the same form. Use the visual Designer to change its layout.

## Where the code lives

| File | Responsibility |
| --- | --- |
| `Program.cs` | Starts the app and opens the dashboard. |
| `DashboardForm.cs` | Opens the login screen, then shows the menu for the user's role. |
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

These credentials apply when the app creates sample data in a database **with no user accounts**. They are not an automatic password reset for an existing database.

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
