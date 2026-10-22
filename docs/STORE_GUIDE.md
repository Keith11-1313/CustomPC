# Using CustomPC

For permissions, read [ROLES.md](ROLES.md). For learning the code, start with [CODE_GUIDE.md](CODE_GUIDE.md).

## Start and sign in

Open the solution and press F5. Login opens before the dashboard. Guests choose Browse as guest. Closing startup login exits the app. If the database is empty, Setup creates the first Admin and requires a personal password of at least 12 characters.

The prepared Admin was `admin@gmail.com`, password `AdminPassword@123`, name `admin`. Use its current saved credentials if edited. Admin creates Staff/Customer accounts in User accounts. Public registration always creates Customer accounts. Roles are separate; Admin cannot perform Staff or Customer transactions.

The sidebar switches hosted pages. Overview/Back returns to the dashboard. Editors close without saving when Cancel is used. Sign out clears the unfinished build. Cancelling Guest Sign in returns to the Guest workspace; cancelling after signing out ends the app.

The main window opens maximized to use the monitor's working area. Restore it with the title-bar button to resize it. The sidebar stays in place and changes to an icon rail in narrower windows; hover or focus a destination for its label. Navigation can scroll on short screens. In a narrower builder, checkout moves below the parts; scroll the component list to reach the remaining categories. The total and order action stay visible.

## Products and stock

Commands are above tables; filters wrap as the page narrows. Refresh is at the upper right. Customer and Guest browse Parts catalog using the category rail, which moves above products in narrower windows. Product editors use scrolling field groups with pinned Save/Cancel buttons.

Admin adds/edits/archives products in Parts & inventory, including price, specifications and photographs. New products start at zero stock. Staff uses Adjust stock with a quantity change and reason to record actual receipts or corrections. Admin metadata edits preserve Staff stock. Available stock equals on-hand stock minus reservations.

Admin's part editor accepts JPG/PNG files up to 15 MB. It copies a normalized PNG into ProductImages beside the database; deleting the original upload does not remove the saved photo. Remove photo clears the part reference. Older copied images are retained for recovery. Missing/unreadable photos show a placeholder. Copying arbitrary pictures into ProductImages alone does not associate them with a product; use the editor.

The 42-product [manifest](CATALOG_MANIFEST.md) records researched reference prices/specifications and image limitations. Admin reviews actual prices and specifications; Staff records actual inventory. The document is not a live stock report.


The editor changes its visible specification fields when Category changes:

| Category | Category-specific inputs |
| --- | --- |
| CPU | Socket, power use, integrated graphics |
| Motherboard | Socket, memory type, power use, video output |
| RAM | Memory type, power use |
| GPU | Power use, recommended PSU capacity |
| Storage | Power use |
| Power Supply | PSU capacity |
| Case | General specifications; no compatibility inputs |

Name, brand, price, low-stock threshold, specifications and photo controls remain common. The Admin stock box is hidden. Switching categories preserves unsaved draft inputs, but saving excludes fields unrelated to the selected category. The form closes gaps and adjusts its height.

## Build, payment and pickup

1. Customer or Guest chooses CPU, motherboard, RAM, storage, PSU and case. GPU is optional only with verified CPU integrated graphics and motherboard video output.
   Motherboard choices follow the selected CPU socket; RAM follows the motherboard's memory type; PSU choices meet the selected components' stored power requirements. Changing an earlier component clears later selections that no longer fit. The integrated-graphics option appears only for a verified CPU/board pair. Guest checkout is labelled **Log in and order** and becomes available for a complete valid build.
2. Socket, memory and PSU checks always run. PSU must cover estimated component power plus 100 W and recorded recommended PSU requirements. BIOS support, case dimensions and cooler clearance still need physical review.
3. Valid Guest checkout opens login. Customer login continues the same checkout and saves the selected build/order to that account after confirmation. Cancelled login retains the preview; Admin/Staff login returns to their own dashboard.
4. The order reserves stock and has a 24-hour payment deadline. Staff records full payment received at the store.
5. Staff advances Paid → Processing → Assembly → Ready for Pickup → Completed. Completion deducts on-hand stock and ends reservations.
6. Customer can cancel their own unpaid pending order. Cancellation/expiry releases reservations. Paid orders cannot be cancelled through this feature.

Expiry runs on startup/refresh and the running timer. It does not run while the app is closed. Order buttons depend on role, selected row and status. Completed/cancelled records remain readable, with transaction actions disabled.

## Charts and reports

Admin defaults to 30 days: recorded payment amounts by local payment date, quantities of top paid parts, and current inventory health. Staff defaults to seven days: incoming counts by local creation date, current queue and current category stock counts. Staff sees operational summary counts and stock-attention links without charts. The period selector affects historical counts and Admin charts, not current snapshots.

Use Open selected, chart double-clicks or labelled navigation buttons to open filtered Orders/Inventory. Admin reports provide inventory, orders, stock movements and sales with CSV export. Receipts use the fixed CustomPC name and support text saving and printing.

## Activity & Backups

Admin can view the latest 500 activity entries and refresh them. Historical actions from before logging was implemented cannot be reconstructed. Unexpected failures display a diagnostic reference; default logs are still under `%LOCALAPPDATA%/CustomPC/Logs` (an explicit database override places logs beside that override).

Save backup creates a ZIP containing custompc.db and ProductImages. Choose a new filename; existing backups are refused. One automatic ZIP per local date is saved to Database/Backups when an existing database is opened. An app left open across days gets the next startup backup when reopened.

Restore requires the current Admin password and confirmation. It replaces records/photos from the selected ZIP, saves a safety backup first, then closes the app. Reopen with credentials from the restored snapshot. Older database-only `.db` backups remain supported but do not contain photos. If recovery itself fails, preserve the named recovery files and safety ZIP before continuing.

## Practice separately

Stop the app, open PowerShell, then run:

```powershell
$env:CUSTOMPC_DATABASE_PATH = Join-Path $env:TEMP 'CustomPC-Practice/custompc.db'
$env:CUSTOMPC_DEMO_MODE = '1'
dotnet run --project 'C:/Users/Jerald/Desktop/CustomPC/CustomPC/CustomPC.csproj'
```

A fresh practice database has demo Admin/Staff/Customer emails `admin@custompc.local`, `staff@custompc.local`, `customer@custompc.local`, password `CustomPC123!`. These are explicit practice fixtures. When finished, close the app and remove both environment variables from that shell:

```powershell
Remove-Item Env:/CUSTOMPC_DATABASE_PATH
Remove-Item Env:/CUSTOMPC_DEMO_MODE
```

Before actual store use, perform the manual Designer, file-dialog, printer, Windows DPI and external-drive recovery checks in [VALIDATION.md](VALIDATION.md).

New and changed account passwords use 12–128 characters with uppercase and lowercase letters, a number and a symbol. Existing passwords continue to work until changed. Activity & Backups shows readable saved-state summaries for new actions; double-click a row to read its full details. Older activity entries retain their original text.


Admin overview: confirmed-payment and paid-order cards use the selected 7/30/90-day period. Low-stock and out-of-stock cards are current product counts. Stock attention links open inventory with the corresponding stock filter. The three charts retain keyboard-accessible selection controls; empty charts show their headings and a relevant action. Staff sees operational summary cards and stock-attention links without charts. Parts catalog is available only to Customer and Guest; Admin and Staff use Parts & inventory.
