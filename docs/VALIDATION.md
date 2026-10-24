# Validation

This page describes validation coverage and checks to repeat before a release. It does not certify the current revision or every Windows environment.

## Build

Install the .NET SDK required by `CustomPC/CustomPC.csproj` and the Windows desktop development tools. From the repository root on Windows, run:

```powershell
dotnet restore CustomPC/CustomPC.csproj
dotnet build CustomPC/CustomPC.csproj -c Release
git diff --check
```

A successful build verifies compilation. It does not verify runtime workflows, Visual Studio Designer loading, accessibility, native dialogs or printer output.

## Use isolated data

Run validation against a separate practice database and product-image folder. Do not use operational accounts, orders, payments or inventory as test fixtures.

`CUSTOMPC_DATABASE_PATH` selects an alternate database. For example, in PowerShell:

```powershell
$env:CUSTOMPC_DATABASE_PATH = Join-Path $env:TEMP ('CustomPC-validation-' + [guid]::NewGuid().ToString('N') + '/custompc.db')
dotnet run --project CustomPC/CustomPC.csproj
```

Complete setup with practice credentials. Close the application before clearing the override:

```powershell
Remove-Item Env:/CUSTOMPC_DATABASE_PATH
```

Keep test photographs beside the selected database in `ProductImages`. Use the application's backup and restore operations when testing recovery; copying an open SQLite database file alone may omit pending changes.

## Previously reported coverage

Development checks have exercised the following areas using isolated data and programmatic WinForms rendering:

- Account validation, role restrictions and ownership of customer orders.
- Component compatibility, invalid-checkout rejection, reservations and order processing.
- Product photographs, inventory tables, report output and database reload.
- Responsive layouts, empty states, search clearing and dashboard filter navigation.
- Automatic view updates, retained filters and table selection.
- Database and photograph backup and restore handling.
- Shared receipt preview and paginated rendering, including long names and paid/unpaid instructions.
- Policy readers, policy links and cancellation of order confirmation.

The development harnesses and their reports are not included in this repository. These are historical coverage summaries, not independently reproducible test results or proof that the current revision passes. Counts from separate runs overlap and are intentionally omitted. Later source changes require fresh validation.

Programmatic renders can reveal clipping and layout problems, but do not establish that native controls, physical input or actual display scaling work correctly.

## Manual release checklist

Record the commit, application version, Windows version, .NET version and relevant test conditions in the release report. Use generic environment descriptions; do not publish personal device identifiers, account credentials, customer data or private filesystem paths.

| Area | Check |
| --- | --- |
| Visual Studio Designer | Open forms and reusable controls with View Designer. Confirm they load without accessing the operational database. |
| Window layout | Resize and maximize the workspace. Check compact navigation, wrapping labels, scrolling, empty states and reachable actions. |
| Display scaling | Test actual Windows scaling at 100%, 125% and 150% where supported. Check dropdowns, text, focus indicators and dialog bounds. |
| Keyboard and accessibility | Navigate without a mouse. Check tab order, accessible names, password visibility, search clearing and chart navigation. Use a screen reader where available. |
| Accounts and permissions | Exercise Admin, Staff, Customer and Guest separately. Verify restricted screens and operations remain blocked. |
| Catalog and builder | Browse products, inspect details, retain draft choices across navigation and verify compatibility warnings. Confirm invalid builds cannot be ordered. |
| Order workflow | Create a practice order as Customer; record payment, assembly and pickup as Staff. Verify ownership, reservations, expiry and saved state after reopening. |
| Search and filters | Test matching and no-result queries, clear actions, dashboard constraints and preserved category/status filters. |
| Automatic updates | Change practice data and confirm visible views update without interrupting modal editors or losing valid selection. |
| Reports | Check date boundaries, role visibility, totals and CSV output. Image columns should not appear in CSV files. |
| Native photo dialogs | Import a supported image, cancel selection, remove the original source file and reopen the saved product. |
| Backup and recovery | Back up a practice store with photographs, restore through native dialogs, relaunch and verify records and images. Test removable storage separately. |
| Receipts | Inspect preview and print a practice receipt. Check margins, wrapping, page breaks, totals and payment instructions. Bitmap rendering does not verify a physical printer. |
| Policy dialogs | Open each policy link, scroll through the content and return to the owner. Cancelling confirmation must not submit an order. |

Mark each check as passed, failed or not run, and describe failures and remaining limitations. Do not present unexecuted checks as completed.
