# CustomPC Windows Forms

A Windows Forms adaptation of the supplied CustomPC proposal. Targets .NET 10 on Windows. Open `CustomPC.slnx` in Visual Studio with the .NET desktop development workload, restore NuGet packages, and press F5.

## Open a screen in the Designer

In Solution Explorer, right-click a form's **.cs** file and choose **View Designer**, or select it and press **Shift+F7**. Open `LoginForm.cs`, `DashboardForm.cs`, `BuilderForm.cs`, or any form below. Controls are declared and positioned in each matching `.Designer.cs` file; business logic remains in the main `.cs` file. Do not open the `.Designer.cs` file expecting the visual view. Database access is skipped in design mode.

## Demo sign-in

| Role | Email | Password |
| --- | --- | --- |
| Admin | admin@custompc.local | CustomPC123! |
| Staff | staff@custompc.local | CustomPC123! |
| Customer | customer@custompc.local | CustomPC123! |

Accounts and illustrative parts/prices are seeded on the first run. Guest browsing is available from the login screen. Admin can change passwords using User accounts. Passwords use salted PBKDF2-SHA256 hashes; the database does not store plaintext passwords. Demo credentials are for project demonstrations.

## Screens and proposal coverage

| Form | Purpose |
| --- | --- |
| LoginForm / RegisterForm | Sign-in, customer registration, guest entry |
| DashboardForm | Role-specific dashboard and navigation |
| CatalogForm | Component search, category and availability filters; name, brand and specification search |
| BuilderForm | Seven component selections, socket/RAM/PSU checks, live PHP total, customer selection for staff |
| OrdersForm | Customer-owned order tracking; staff payments and status management; cancellation |
| ReceiptForm | Receipt details, order number, quantities, prices, payment deadline; print or save text |
| InventoryForm / PartEditForm | Component add/edit/archive, prices/specifications/suppliers, available/reserved/low/out-of-stock quantities |
| StockAdjustmentForm | Staff restocking/corrections with reason and audit record |
| SuppliersForm / SupplierEditForm | Supplier contacts, editing/archiving, supplied component links |
| UsersForm / UserEditForm | Admin accounts, role changes, editing/archiving and password changes |
| CompatibilityForm / RuleEditForm | Admin management of enabled socket, memory and power checks |
| ReportsForm | Inventory, orders and stock movements for staff; sales report for admin; date filtering and CSV export |

Orders reserve one unit of each selected component. Staff confirm the full payment at the physical store, then advance through **Paid → Processing → Assembly → Ready for Pickup → Completed**. Completion records pickup and deducts on-hand stock once. Cancelling an unpaid order releases its reservations. Paid orders retain reservations until completion; paid-order cancellation/refunds are outside the proposal.

The app checks the 24-hour payment deadline every 30 seconds while running, when data refreshes, and on startup. If the app is closed, expired orders are cancelled the next time it opens. An always-running service would be needed for cancellation while the desktop app is closed.

## Database and boundaries

The SQLite database is at `%LOCALAPPDATA%\CustomPC\custompc.db`. It has separate tables for users/customers, parts, suppliers, compatibility rules, saved confirmed builds, orders, payments and inventory movements. Entity fields are serialized as JSON within each table, with IDs as primary keys. Receipts retain the names and prices from order creation. Reservations, payments and stock changes are saved in transactions. This is a local desktop database, not a hosted multi-store server. `CUSTOMPC_DATABASE_PATH` can override the database location for isolated verification.

Compatibility uses stored socket, RAM generation, and component power data, with 100 W power headroom. Physical case dimensions, cooler clearance and unrecorded specifications require manual review. No online payments, shipping, or delivery were added.

## Verify

```powershell
dotnet build CustomPC\CustomPC.csproj
dotnet run --project Verification\Verification.csproj
```

Verification uses a new database under `.qa`, exercises role restrictions, compatibility errors, reservations, expiry, payment and pickup, then renders all forms off-screen. The verification suite passed 50 checks, and the build completed with zero warnings and errors. It does not modify the normal application database. It does not test a physical printer or prove Visual Studio Designer loading; verify those locally with View Designer and the receipt print dialog.

For a manual walkthrough: sign in as Customer, choose Ryzen 5 5600, B550M motherboard, 16 GB DDR4, RTX 3060, 500 GB SSD, 650 W PSU and Micro ATX tower. The illustrative total is **PHP 36,200.00**. Confirm, then switch to Staff to confirm payment and advance the order. Check inventory quantities and reports as Admin.
