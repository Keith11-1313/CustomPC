# Test CustomPC store workflows

CustomPC is being developed for **one Windows workstation and one Windows user profile**. Staff sign in with separate application accounts on that workstation. Records are now stored in the project-root `Database\custompc.db`, with ProductImages beside it. Do not put the database on a network share or use it across computers.

Read [ROLES.md](ROLES.md) before testing. Roles are separate jobs: Admin manages the system and reports, Staff handles stock/payment/progress, and Customers create or cancel their own unpaid orders. Admin is blocked from Staff and Customer actions.

## Run the project while revising

Open CustomPC.slnx in Visual Studio and press F5. There is no published release or installer to use yet. A debug EXE in bin is a normal result of running C# code, not a finished product. Build caches live in obj; both folders are ignored by Git.

Use a separate practice database when testing changes. To start with sample parts and accounts, close any running app, open a PowerShell terminal in this project folder, and run:

```powershell
$env:CUSTOMPC_DATABASE_PATH = Join-Path $env:TEMP 'CustomPC-practice\custompc.db'
$env:CUSTOMPC_DEMO_MODE = '1'
dotnet run --project .\CustomPC\CustomPC.csproj
```

The variables apply to that terminal and its child app. They do not change a Visual Studio window that was already open. To test first-time setup instead, use a new database filename and remove CUSTOMPC_DEMO_MODE with Remove-Item Env:CUSTOMPC_DEMO_MODE. Do not point practice tests at your real database.

## First store setup

1. Open the app. If this profile has no accounts, **Set up your store** appears.
2. Enter your administrator name and email. Choose your own password with **at least 12 characters**. Keep it securely; the app cannot reveal a saved password.
3. Click **Create store administrator**. Expect login next. Sign in with your new account.
4. Receipts use the fixed CustomPC name. Store tools provides backup, recovery and activity history.
5. Add distributors through **Suppliers**, then add actual components through **Parts & inventory → Add part**. Check price and specifications. New parts start at zero stock; sign in as Staff and use Adjust stock to record received quantities. New stores contain no fake stock.
6. Add individual Staff accounts through **User accounts**. Customers can register from login.

An existing database opens login directly. Its old accounts, parts, orders and payments remain saved. Normal mode refuses the public demo password. Sign in with your own Admin account and create the Staff/Customer accounts you need. You cannot archive your currently signed-in Admin. Secure Customer accounts are not blocked from ordering because of unrelated sample accounts. Review sample prices and stock separately before selling.

## Daily order workflow

1. The Customer registers and signs in with their own account. Staff and Admin cannot place orders for another customer.
2. The Customer chooses the six required components and a dedicated GPU when needed, then checks compatibility and total. Staff must also check actual dimensions, connectors and other specifications; automatic checks cover recorded socket, RAM type and PSU wattage/headroom.
3. Click **Confirm build & order**. The receipt opens. Components become reserved and the order awaits payment for 24 hours.
4. Switch to a Staff account. After receiving the full amount at the physical store, open **Orders & payments**, select the pending order, and click **Confirm store payment**. This records the payment and Staff identity. Admin and Customer accounts cannot confirm payment. It does not collect money or verify external payments.
5. Advance through Processing, Assembly and Ready for Pickup. Mark Completed when the customer collects the PC. Completion deducts stock once and records movements.
6. A Customer can cancel only their own unpaid order. Staff and Admin cannot cancel it manually. Automatic expiry is processed on startup/refresh and the running timer; a closed app processes it when next opened.

Admin-only reports show recorded inventory, orders, stock movements and sales, with CSV export. Staff monitors daily work through inventory and Orders; Admin monitors orders read-only. The receipt is the PC order record. Accounting/tax invoicing and paid-order returns/refunds remain part of your separate store process.

Order actions start disabled until you select a row. A pending order enables Staff payment or its Customer's cancellation; a paid/in-progress order enables Staff's next stage. Completed/cancelled orders allow reading the receipt, with transaction actions disabled. The selected row stays selected after payment/progress if it still matches the filters. If a search or status filter removes that row, select another visible order before acting.

## Backups and recovery

One automatic backup per date is created **before opening an existing database**, in `Database\Backups`. An app left open across days gets its next startup backup when restarted. Old backups are not silently deleted.

At the end of each working day, sign in as Admin and select **Store tools & backups → Save backup**. Choose a new filename on an external drive. Expect the screen to show a verified-backup confirmation. Existing files are refused rather than overwritten. Keep the drive securely: backups contain customer records and password hashes. The app does not encrypt database or backup files; protect them through your Windows account and backup location.

To restore:

1. Sign in as an active Admin and open Store tools.
2. Enter your current password in **Your password to restore**.
3. Click **Restore backup**, choose the correct `.zip` (or an older `.db`), and read the filename in the confirmation. Restoring replaces current records with that snapshot.
4. The app validates it and creates a complete `before-restore-...zip` and database safety copy first. After success, the app closes. Reopen it with an account/password that existed in the restored backup.

The restore screen requires a working database and Admin login. If corruption prevents login, close the app, preserve the damaged file, and have a maintainer recover from a verified backup. Do not delete the database repeatedly to troubleshoot.

## Accounts, history and errors

Five failed attempts for the same email block further attempts for five minutes. This survives restarting the app and expires automatically. An Admin can replace a user's password in User accounts.

Unexpected failures display a diagnostic reference. Logs are in `%LOCALAPPDATA%\CustomPC\Logs`; they contain error types and stack locations, not entered passwords or database contents. Input errors such as insufficient stock display their validation message directly. **Store tools → Activity history** shows the latest 500 successful changes and their users. History begins with this version and cannot reconstruct earlier activity. Local history is not tamper-proof against direct database access.

Lock Windows or close the app when leaving the workstation. Back does not sign out; use **Sign out**.

## Before the first real customer

On the actual store computer, use a separate practice database to check:

- Monitor size and Windows scaling; scroll to Save and Back where needed.
- One complete order through payment, assembly, pickup and stock movement.
- Actual printer output and page layout; CSV and text-receipt saves through Windows file pickers.
- External-drive backup and recovery: reopen after restoring and verify accounts, stock, orders and payments.
- Visual Studio Designer loading if you will edit forms. Runtime creation is a separate check.

Then load verified stock, prices and accounts into the live database. Read [VALIDATION.md](VALIDATION.md) for executed checks and remaining manual checks and revisions.

## For the junior developer maintaining it

Start with [CODE_GUIDE.md](CODE_GUIDE.md). For this update:

| Want to change | Start here | Look for |
| --- | --- | --- |
| Logo | `Assets/logo.png` and `BrandLogo.cs` | Your supplied image, shown using a PictureBox |
| Layout or colours | The form's `.Designer.cs` through View Designer | Ordinary Panels, Labels, Buttons and fields |
| Small monitor support | `StoreWindow.cs` | `OnLoad` calls `FitToWorkingArea`; all forms inherit this class |
| First administrator | `SetupForm.cs` | Save calls `AppStore.CompleteSetup` |
| Backup or recovery | `StoreToolsForm.cs` | Button handlers call `AppStore.Maintenance.cs` |
| SQLite writes | `Database.cs` | `WriteRecords` compares JSON and upserts changed rows |
| Atomic save and audit | `AppStore.cs` | `SaveData` writes records and history in the same transaction |

The colours follow [EasyPC](https://easypc.com.ph/): green `#54B137`, charcoal `#202923`, white surfaces and pale grey `#F3F5F7`. Green buttons use dark text for contrast. Status text carries meaning as well as colour. Headings use Righteous; body text and controls use Roboto.

Backups follow Microsoft's [SQLite online backup API](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/backup), which copies a consistent SQLite snapshot rather than blindly copying an open file.

## Change the branding yourself

1. Open LoginForm.cs with View Designer (Shift+F7). Select heading and find Font in Properties. Righteous is the heading font; Roboto is used for inputs and buttons.
2. The logo is CustomPC/Assets/logo.png. Replace that image if you change your branding. BrandLogo.cs reads the embedded asset and displays it in login, the dashboard and window icons.
3. AppFonts.cs reads the three embedded TTF files. StoreWindow.cs applies their font families to the controls, so fonts work without installation. The conventional font assignments remain in each Designer file.
4. Keep the font licenses in Assets/Fonts. Righteous uses the SIL Open Font License; these Roboto files use the Apache License 2.0.

Sources: [Righteous font files and license](https://github.com/google/fonts/tree/main/ofl/righteous), [Roboto static font files](https://github.com/googlefonts/roboto-2/tree/main/src/hinted), [Roboto license](https://github.com/googlefonts/roboto-2/blob/main/LICENSE).


## Modern catalog, charts and photos

The prepared Database folder already contains 42 ordinary products with photos and zero stock. Review the [manifest](CATALOG_MANIFEST.md), prices and supplier assignments. Admin adds/edits products; Staff records actual receipts. The old accounts and orders were reset after a safety backup. Sign in as admin@gmail.com with password admin@gmail.com, then add your Staff and Customer accounts.

Admin can Choose photo or Remove photo in the part editor. Choose JPG/PNG up to 15 MB; the app copies and normalizes it into ProductImages beside custompc.db. Deleting the original upload does not affect the saved photo. Removing the reference retains old copied files for recovery. Missing photos show a placeholder.

New manual and automatic backups are complete **ZIP files containing database and photographs**. Older database-only .db recovery is supported, but cannot recover missing photographs. If rollback fails, preserve the named recovery folders and before-restore ZIP and obtain maintainer help before continuing.

Admin defaults to 30-day confirmed payments and paid-part quantities. Staff defaults to seven-day incoming-order counts. Staff sees current queue counts and stock readiness without Admin financial charts. Inventory and queue charts remain current regardless of period. Use the selection list and Open selected, double-click a chart day/bar, or use the labelled navigation buttons. Guests receive no personal-order panels.

Separate forms are hosted inside one main window. Overview and Back return to the dashboard; the sidebar changes pages. An unfinished build survives page navigation. Sign out clears it. A valid Guest checkout prompts login and Customer login continues with the chosen parts.

For the implementation, read [MODERN_UI_GUIDE.md](MODERN_UI_GUIDE.md). For current test evidence and manual checks, read [VALIDATION.md](VALIDATION.md).
CustomPC branding is fixed. Setup and Store tools do not offer business-name, receipt-header or business-contact customization.
