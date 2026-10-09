# CustomPC validation results

Validated on **9 October 2026**, Windows, .NET SDK 10.0.401. The final workflow run completed at **23:02 Philippine time**.

**Result: 261 automated checks passed, 0 failed.** A separate run passed **3 login-button checks against a copy of your existing database**. The application and test harness built with **0 warnings and 0 errors**.

These results apply to the source saved during this validation. Start with [CODE_GUIDE.md](CODE_GUIDE.md) to learn the code. The [code reference](CODE_REFERENCE.md) contains the advanced manual validation checklist.

## Your saved database and the failed login screenshot

The existing file was inspected read-only at:

```text
C:\Users\Jerald\AppData\Local\CustomPC\custompc.db
```

- SQLite `PRAGMA integrity_check` returned `ok`.
- It contained 3 accounts, 12 parts, 1 supplier, and 3 compatibility rules. Builds, orders, payments, and stock movements were empty.
- Admin, Staff, and Customer demo accounts were active and had their expected roles.
- All three stored password hashes matched `CustomPC123!`.
- The actual `LoginForm` text fields and **Sign in** button authenticated all three accounts using a separate database copy. The original file's SHA-256 was compared before and after testing and remained unchanged.

The exact failed input in the screenshot could not be recovered or reproduced. The checks do **not** establish why that particular attempt failed. No account was reset and the real database was not replaced.

To retry the current build, run it from Visual Studio, enter `admin@custompc.local` and `CustomPC123!`, and check **Show password** to inspect the exact text. The password is case-sensitive and ends with `!`. An account edited later by Admin keeps its saved password.

## What was exercised

The 261 checks include **142 business/data checks** and **119 form checks**. Form checks created real WinForms controls, used `PerformClick` for buttons, changed selectors/search inputs, and inspected resulting saved records and displayed data. Permission checks also called reload handlers and attempted restricted screen construction. Test windows stayed hidden; native confirmation/error dialogs were answered within the test process.

| Area | Checks | Evidence covered |
| --- | ---: | --- |
| Fresh database and authentication | 21 | Seed data, three roles, logout, wrong/missing password, blank credentials, salted hashes, malformed hash, reopen without duplicate seeding |
| Registration and account management | 18 | Duplicate/invalid accounts, customer registration, password changes, archives, persistence, current-admin protection |
| Permissions and dashboard roles | 28 | Guest/customer/staff restrictions, role-specific buttons, own-account ordering |
| Inventory, suppliers, rules | 25 | Valid/invalid edits, movements, archive links, stock limits, overflow rejection, rule changes, persistence |
| Builds and reservations | 21 | Missing/duplicate categories, socket/RAM/PSU checks, current price, receipt snapshots, reservations and cancellation |
| Payment and order lifecycle | 17 | Staff-only payment, duplicate payment rejection, stages through pickup, stock deducted once, persistence |
| Expiry and transactions | 12 | Deadline expiry, stale second instance, last-unit overbooking rejection, injected SQLite failure and rollback |
| Login/register/guest form buttons | 16 | Actual login, wrong/blank input, show-password event, registration dialog, registration save, guest session |
| Editor save buttons | 12 | Supplier/part/user/rule saves, archive preservation, password retention, staff stock adjustment |
| Short part editor | 4 | Vertical scrolling at 720px window height, whole Save button reachable, scrolled save persists |
| Catalog and builder controls | 18 | Live search/category/availability filters, totals, invalid-build feedback, changed-price rejection, confirm and receipt |
| Order form buttons | 11 | Pay, advance, cancel, displayed status, search/status filters |
| Customer access and reports | 29 | Own orders/receipt, denied other-customer receipt, management permissions, report dates/totals, hidden record IDs |
| Access revoked with a screen open | 29 | Clear protected grids, disable actions, block export, Sales demotion, staff archive |
| **Full run** | **261** | **All passed** |
| Existing-database login buttons, separate run | 3 | Admin, Staff, Customer login using the saved database copy; all passed |

Rollback checks deliberately caused a SQLite insert to fail. Both the persisted records and in-memory records returned to their original state. This checks failure behavior as well as successful operations.

## Issues corrected during validation

1. **Archived supplier links:** editing a part now keeps its currently assigned supplier even when that supplier is archived. New assignments still require an active supplier.
2. **Short displays:** the part editor fits the working area and scrolls, making lower fields and Save reachable.
3. **Revoked access:** open management screens refresh the signed-in account, clear inaccessible records, and disable restricted actions. Reports recheck access before export, including Admin access for Sales.
4. **Screen-opening errors:** dashboard screen construction and startup errors are handled by the app's error display.
5. **Stock overflow:** large adjustments use `long` arithmetic and reject results above 10,000,000 before saving. Invalid stock edits leave stock and movement history unchanged.
6. **Changed prices:** confirmation includes the displayed total. The order transaction checks it against current prices, rejects a changed quote, and refreshes the builder before another confirmation.
7. **Login diagnostics:** added Show password, clear blank-input validation, accurate initial-account wording, and graceful rejection of a malformed stored password hash.

## What still needs manual verification

- **Visual Studio Designer:** open the forms with View Designer / Shift+F7. Compilation and runtime form creation do not prove the Visual Studio design host loads them.
- **Printing:** receipt text was checked, but a Windows print dialog, printer driver, page layout, and physical/PDF output were not exercised.
- **Successful file saving:** report access checks were tested, including blocked export after access revocation. A successful CSV save through the Windows file picker and receipt `.txt` save still need the manual walkthrough in the code reference.
- **Full visual/keyboard use:** rendered login and scrolled part-editor images were inspected. This does not cover every screen at every DPI, window size, or keyboard sequence.
- **Compatibility scope:** automatic rules cover recorded socket, RAM type, and PSU wattage/headroom. BIOS support, clearances, dimensions, and connectors require store review.
- **Closed app:** expiry runs on startup/refresh and the running dashboard timer. No background process cancels orders while the application is closed.

## Reproduce the automated checks on this computer

The harness and generated databases were kept outside the application project. `.qa` and `Verification` were not recreated.

In PowerShell:

```powershell
Set-Location 'C:\Users\Jerald\.codex\visualizations\2026\10\07\01a11539-e782-7ae3-bb38-3eafc37ec703\custompc-runtime-validation'
dotnet build RuntimeValidation.csproj --no-restore
dotnet bin\Debug\net10.0-windows\RuntimeValidation.dll --ui
dotnet bin\Debug\net10.0-windows\RuntimeValidation.dll --ui --copy-only --database-copy actual-database-copy.db
```

Wait for the build to succeed before running the commands. The full run creates fresh test databases in a timestamped folder. The copy-only command uses the previously captured database copy; it does not inspect newer edits to the real file. A future login investigation needs a new copy.

Machine-readable evidence on this computer:

- [Full run results](C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703/custompc-runtime-validation/run-20261009-230203-898/report.json)
- [Existing-database login results](C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703/custompc-runtime-validation/run-20261009-224736-007/report.json)
- [Read-only database inspection](C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703/custompc-runtime-validation/actual-database-evidence.json)

The external harness is specific to this computer. The manual checklist in the code reference can be used with a separate demonstration database on another computer.
