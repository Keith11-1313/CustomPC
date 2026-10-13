# CustomPC validation results

Validated on **13 October 2026**, Windows, .NET SDK 10.0.401. This source project is under active revision. The latest run covers the strict Admin/Staff/Customer permission split, role-specific menus, EasyPC-inspired green palette, supplied logo and bundled Righteous/Roboto fonts. No published release package is retained.

**Result: 667 automated checks passed, 0 failed.** This includes 153 strict-role checks and 38 customer-revision/GPU/inventory-button checks alongside the business, navigation, store-hardening and font/render checks. The earlier branding-only total of 476 is historical; one former Staff-report UI check was removed because Staff no longer has report access. The build completed with **0 warnings and 0 errors** after a successful network-enabled restore. These results verify tested code paths; they do not mean the product is finished.

Separately, **6 current-database copy-upgrade checks passed** on 13 October. A read-only SQLite snapshot of your saved database was upgraded on a separate path. Account hashes and record counts were preserved, and the original file's SHA-256 remained unchanged. The earlier **9 October** run passed **3 login-button checks** against its then-current saved-database copy; those historical credentials were not retested against the newer original file.

These results apply to the source saved during this validation. Start with [CODE_GUIDE.md](CODE_GUIDE.md) to learn the code. The [code reference](CODE_REFERENCE.md) contains the advanced manual validation checklist.

## Fresh role screen audit

The follow-up [screen-by-screen audit](SCREEN_VALIDATION.md) passed **627 checks, 0 failures**: 420 for actual dashboard routes and nested actions, 146 for an empty normal-mode store's Admin → Staff → Customer → Staff workflow, and 61 for selected-order action availability. The existing 667-check regression suite was rerun after the correction and passed again. These totals overlap; they are separate runs.

All 16 permitted dashboard routes across Admin, Staff, Customer and Guest were opened through real buttons. Nested account role assignment, part/supplier/rule saves, stock adjustments, receipts, customer submission/cancellation, payment, each order stage, reports and store settings were exercised. Screens were rendered from their runtime controls; compact working-area checks verified buttons remained reachable with scrolling.

The audit found transaction buttons enabled on completed orders. `OrdersForm.UpdateOrderActions` now uses role, explicit selection, deadline and status. `OrdersForm_Shown` prevents an automatic first-row selection from activating actions. The selected row survives payment/progress refresh when still in the filtered list, and disabled transaction buttons use a muted background. Checks confirmed that completed/cancelled rows permit receipt viewing only, filtering/clearing selection disables record actions, and disabled buttons cannot create another payment or stock deduction.

The original database was inspected read-only during this follow-up: integrity was `ok`, `jerald@gmail.com` was the only active account with Admin role, and the three demo accounts were archived. All workflow mutations used external isolated databases. Successful native file-picker operations, physical printing, Designer loading and actual Windows DPI changes remain unverified; they are explicitly listed in the screen audit.

## Annotated customer-screen revisions

- Customer greetings omit the role suffix. Customer dashboards and order lists omit the redundant Customer column; Admin/Staff retain it for monitoring multiple customers.
- The builder takes ownership from the signed-in session. Its account dropdown and explanatory subtitle/limits text were removed.
- Page subtitles were removed from all 19 forms. Headers and body positions were adjusted, and Sign out uses final product wording.
- The GPU dropdown includes No dedicated GPU. Six-part builds require a CPU marked HasIntegratedGraphics and a motherboard marked HasVideoOutput; duplicate GPUs, missing graphics support and absent board output are rejected.
- Six-part totals, reservations, payment, pickup and stock movements were tested. A no-GPU order neither reserves nor deducts dedicated graphics cards.
- Inventory hides unavailable actions. Tests used the actual Admin Add/Edit/Movement history/Refresh buttons and the actual Staff Adjust stock/Refresh buttons through their parent windows.

The requested account change was tested on a read-only snapshot copy before applying it to the saved database: jerald@gmail.com was promoted to Admin with its password unchanged, and the three public-password demo accounts were archived. Account counts and all seven business tables were preserved. A verified before-admin-setup backup was created in the normal Backups folder. This is an explicitly requested account update, separate from the isolated app tests.

## Strict role revision

The confirmed role split is documented in [ROLES.md](ROLES.md). Admin does not inherit Staff or Customer. Shared order/inventory read access is explicitly named.

- Admin is denied order creation/cancellation, payment confirmation, order progress and stock adjustment. Catalog saves also reject hidden stock changes; new parts start at zero.
- Staff is denied Customer ordering/cancellation and Admin account, catalog, supplier, rules, settings, backup, audit and report actions.
- Customer ordering/cancellation is limited to the signed-in account, with payment and progress restricted to Staff. Guests may browse and preview but cannot open order management or submit builds.
- The sidebar shows only permitted screens. Order headings and action buttons distinguish monitoring, processing and personal orders.
- Tests invoke hidden Admin handlers directly, attempt role-assignment escalation and tamper with the disabled stock input. Denied operations leave business records unchanged.
- Tests exercise a Staff delivery during an Admin catalog edit, promotion from Staff to Admin while a stock editor is open, receipt access after account archival, and rejection of an unknown saved role.
## Store hardening and revised screens

- First store setup creates the chosen Admin and compatibility rules without public accounts or fake stock. Sample data requires an explicit practice-mode environment variable.
- Passwords use a versioned 600,000-round hash while preserving verification of older accounts. Normal new/reset passwords require 12–128 characters. Five failed attempts persist a five-minute throttle, and a failed sign-in clears previous privileges.
- Normal login rejects the public sample password. A personal Customer can order without being blocked by unrelated sample accounts. Recovery requires a usable Admin password, so normal mode rejects a backup whose only Admin has blocked sample credentials.
- Saves update changed rows and append successful activity inside the same transaction. A trigger-based check verified that saving a part does not delete unrelated user records. Rollback checks still passed.
- Admin-only store details, verified backups and restore were exercised. Restore requires the current password, validates the snapshot, preserves a safety copy and clears the session. Invalid files and unauthorized actions were rejected. Failed operations produced no success audit entry.
- All 19 forms were rendered from their real controls. Each form passed assertions for Roboto body text and Righteous headings. The supplied logo is embedded as Assets/logo.png and displayed in login, the dashboard and window icons. Its SHA-256 matches the original image exactly. Each form retains its matching Designer and resource files.
- Simulated 125% control scaling was constrained to a 1366×768 display's working area with scrollbars; this did not change Windows' actual DPI setting. Short editor scrolling and Save behavior were separately exercised.
- CSV escaping rejected formula-like text beginning with operators, leading spaces and control characters.

Visual review caught panel layering and a clipped setup Exit button. Navigation tests caught Esc being cleared when Back moved into a header panel. These were corrected before the final clean run. The earlier failed/intermediate reports remain external evidence; they are not the final result.

This is a source project being revised for one workstation. The earlier published EXE folder, release ZIP and generated build folders were removed at your request. A previous run included 9 published-executable checks; those are historical and are excluded from this current total. Complete the actual printer, file-picker, Windows scaling, Designer and external-drive recovery checks in [STORE_GUIDE.md](STORE_GUIDE.md) as development continues. Accounting, paid-order returns/refunds and multi-computer operation still require further work or separate store processes.

## Login-first startup and Back navigation

The earlier startup code created the dashboard first, then opened login over it. `Program.Main` now opens login before it creates the dashboard. Sign in or Browse as guest opens the homepage; closing login exits. Switching accounts hides the homepage while login is open.

All 15 secondary forms now have a Back button in their `.Designer.cs` file. Back closes the current screen and returns to the existing screen that opened it. Esc activates the same action. Leaving an editor with Back discards unfinished inputs. Leaving a receipt keeps its already-created order.

The added checks exercised:

- The actual application entry point in a fresh process for cancelled login, guest access, and Admin login. No dashboard existed while initial login was open; successful access opened exactly one dashboard after login closed.
- The actual Switch account button: homepage hidden and timer stopped during login, guest permissions restored afterward, and cancelled login closed the homepage without leaving a hidden app.
- The actual Catalog and Registration Back buttons through the screens that opened them, returning to the same homepage or login window.
- All 15 Back buttons: visible within the client area, no overlap with the heading, Escape assigned, and closing with a Cancel result. Reloaded records across all eight collections remained unchanged, including unfinished editor inputs.
- Escape executing the real Catalog Back action and a confirmed order remaining saved after leaving its receipt.

These tests used fresh databases outside the application project. They did not change your normal AppData database. The manual Designer and visual checks below remain separate.

## Your saved database and the failed login screenshot

On **9 October 2026**, the existing file was inspected read-only at:

```text
C:\Users\Jerald\AppData\Local\CustomPC\custompc.db
```

- SQLite `PRAGMA integrity_check` returned `ok`.
- It contained 3 accounts, 12 parts, 1 supplier, and 3 compatibility rules. Builds, orders, payments, and stock movements were empty.
- Admin, Staff, and Customer demo accounts were active and had their expected roles.
- All three stored password hashes matched `CustomPC123!`.
- The actual `LoginForm` text fields and **Sign in** button authenticated all three accounts using a separate database copy. The original file's SHA-256 was compared before and after testing and remained unchanged.

The exact failed input in the screenshot could not be recovered or reproduced. The checks do **not** establish why that particular attempt failed. No account was reset and the real database was not replaced.

For the current saved database, sign in with `jerald@gmail.com` and its existing personal password. The sample-login findings above are historical; those three accounts are now archived, and normal mode refuses their public password. No password reset was performed.

## What was exercised

The 667 checks include 142 business/data checks, 118 existing form checks, 99 startup/navigation checks, 51 store-hardening checks, 7 setup/maintenance button checks, 59 render/font/sizing checks, 153 strict-role checks and 38 customer-revision checks. Fixtures now sign in with Customer to create test orders and Staff to receive stock or confirm payments; they no longer rely on Admin inheriting those actions. Form checks used real controls and events, changed selectors/search inputs and inspected saved records. Test windows stayed invisible; native confirmation/error dialogs were answered within the test process.

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
| Customer access and reports | 28 | Own orders/receipt, denied other-customer receipt, management permissions, report dates/totals, hidden record IDs |
| Access revoked with a screen open | 29 | Clear protected grids, disable actions, block export, Sales demotion, staff archive |
| Login-first startup and Back navigation | 99 | Initial login before dashboard creation, account switching, cancel/exit, return to parent, 15 Back buttons, Escape, discarded inputs, unchanged saved order |
| Store hardening | 51 | Secure setup, versioned/legacy hashes, persistent throttling, backups, restore, audit, permission denial, defaults remediation, changed-row writes and CSV injection checks |
| Setup and Store tools buttons | 7 | Actual setup save/Exit/Esc, store identity save, activity view, Back discarding unsaved input |
| Form renders, fonts and sizing | 59 | Isolated fixture, 19 real form renders, 38 bundled font-family assertions, simulated 125% scaling with a smaller work area |
| Strict roles and bypass attempts | 153 | Four roles, ownership, denial without saved changes, hidden-handler attempts, role-specific menus, live role changes, stock-editor race and revoked receipts |
| Customer revisions and inventory buttons | 38 | Conditional six-part builds, ownership without an account selector, private customer columns, actual parent buttons, normal credentials and safe recovery |
| **Current full run, 13 October** | **667** | **All passed** |
| Current-database copy upgrade, separate 13 October run | 6 | Integrity, preserved business counts and hashes, isolated migration, unchanged original SHA-256 |
| Existing-database login buttons, separate 9 October run | 3 | Admin, Staff, Customer login using the saved database copy; all passed |

Rollback checks deliberately caused a SQLite insert to fail. Both the persisted records and in-memory records returned to their original state. This checks failure behavior as well as successful operations.

## Issues corrected during validation

1. **Archived supplier links:** editing a part now keeps its currently assigned supplier even when that supplier is archived. New assignments still require an active supplier.
2. **Short displays:** the part editor fits the working area and scrolls, making lower fields and Save reachable.
3. **Revoked access:** open management screens refresh the signed-in account, clear inaccessible records, and disable restricted actions. Reports recheck access before export, including Admin access for Sales.
4. **Screen-opening errors:** dashboard screen construction and startup errors are handled by the app's error display.
5. **Stock overflow:** large adjustments use `long` arithmetic and reject results above 10,000,000 before saving. Invalid stock edits leave stock and movement history unchanged.
6. **Changed prices:** confirmation includes the displayed total. The order transaction checks it against current prices, rejects a changed quote, and refreshes the builder before another confirmation.
7. **Login diagnostics:** added Show password, clear blank-input validation, accurate initial-account wording, and graceful rejection of a malformed stored password hash.
8. **Two windows at startup:** login now finishes before the homepage is created. Account switching hides the homepage and pauses its timer until login finishes.
9. **Missing Back navigation:** added Designer buttons and named handlers to all 15 secondary forms, with Esc as the keyboard shortcut. Back closes a screen without calling its save handler.

## What still needs manual verification

- **Visual Studio Designer:** open the forms with View Designer / Shift+F7. Compilation and runtime form creation do not prove the Visual Studio design host loads them.
- **Printing:** receipt text was checked, but a Windows print dialog, printer driver, page layout, and physical/PDF output were not exercised.
- **Successful file saving:** report access checks were tested, including blocked export after access revocation. A successful CSV save through the Windows file picker and receipt `.txt` save still need the manual walkthrough in the code reference.
- **Full visual/keyboard use:** rendered screens and the simulated scaling image were reviewed. This does not cover every actual Windows DPI, monitor size, focus sequence or assistive-technology combination.
- **Compatibility scope:** automatic rules cover recorded socket, RAM type, and PSU wattage/headroom. BIOS support, clearances, dimensions, and connectors require store review.
- **Closed app:** expiry runs on startup/refresh and the running dashboard timer. No background process cancels orders while the application is closed.

## Reproduce the automated checks on this computer

The harness and generated databases were kept outside the application project. `.qa` and `Verification` were not recreated.

In PowerShell, build the external harness with an external output directory:

```powershell
Set-Location 'C:\Users\Jerald\.codex\visualizations\2026\10\07\01a11539-e782-7ae3-bb38-3eafc37ec703\custompc-runtime-validation'
dotnet build RuntimeValidation.csproj --artifacts-path '..\custompc-brand-revision\build' -p:UseAppHost=false
dotnet '..\custompc-brand-revision\build\bin\RuntimeValidation\debug\RuntimeValidation.dll' --ui
```

The full run creates fresh test databases and uses the current project source. It does not publish a release or require a packaged EXE. The harness explicitly opts in to sample fixtures, then disables demo mode for normal-store cases. The separate saved-database checks below are historical; the automated workflows use isolated data. The requested real-account update is documented separately above.

Machine-readable evidence on this computer:

- [Full run results, 13 October](C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703/custompc-runtime-validation/run-20261013-094722-899/report.json)
- [Current saved database copy upgrade](C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703/custompc-runtime-validation/run-20261013-013839-218/report.json)
- [Existing-database login results](C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703/custompc-runtime-validation/run-20261009-224736-007/report.json)
- [Read-only database inspection](C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703/custompc-runtime-validation/actual-database-evidence.json)

The external harness is specific to this computer. The manual checklist in the code reference can be used with a separate demonstration database on another computer.
