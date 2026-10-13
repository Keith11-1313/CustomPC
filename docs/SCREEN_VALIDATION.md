# Role screen validation — 13 October 2026

The screen audit operated real Windows Forms controls and their Click/selection events, then checked the saved results. It opened each permitted screen through its dashboard button and returned through Back. Test windows were kept invisible; screen images were rendered from their actual controls with `DrawToBitmap`. These are runtime tests, not a claim of human mouse testing or Visual Studio Designer validation.

**627 screen/workflow checks passed; 0 failed.** The existing **667-check regression suite** was also rerun after the order-screen correction. The totals overlap in coverage and should not be added together.

## What each role actually has

All 16 permitted dashboard routes were exercised across the four roles. Prohibited routes were checked as hidden and disabled.

| Role | Visible dashboard routes | Actions exercised from those screens |
| --- | --- | --- |
| Admin | Parts catalog, View orders, Parts & inventory, Suppliers, User accounts, Compatibility rules, Reports, Store tools & backups | Browse/search/refresh; read a selected order receipt; add/edit/archive parts; open movement history; add/edit/archive suppliers; add accounts with Admin/Staff/Customer roles, change a role, archive and search accounts; edit a compatibility rule; generate all four report types; save store details and refresh activity. |
| Staff | Parts catalog, Orders & payments, Parts & inventory | Browse/search/refresh; open receipts; confirm payment; advance through Processing, Assembly, Ready for Pickup and Completed; adjust stock with a reason and refresh inventory. |
| Customer | Parts catalog, PC builder, My orders | Browse/search/refresh; choose a valid build, confirm it and open its receipt; view personal orders; cancel an owned unpaid order; refresh. The Customer column is absent from their order table. |
| Guest | Parts catalog, PC builder preview | Browse/search/refresh and select components for a preview. Confirm cannot place an order. Login's Create account opens registration and saves a Customer account. |

Every role's Sign out button was exercised. It hid the dashboard during login and removed private routes after continuing as Guest. Buttons were checked for tab navigation and reachability in a simulated 1366×768 screen with a 1366×728 working area; scrolling was used for the taller part editor.

## A normal store workflow was also exercised

This test used normal mode, with demo mode disabled, on a separate empty database:

1. Save store setup and sign in as the new Admin using a personal password.
2. From Dashboard → User accounts → Add account, create Staff and Customer accounts.
3. From Dashboard → Parts & inventory → Add part, add seven compatible components. Each starts at zero stock.
4. Sign out and sign in as Staff. Record five units of each component through Adjust stock.
5. Sign out and sign in as Customer. Confirm a valid build. Check ownership, pending status, reservation and the configured receipt header.
6. Sign out and sign in as Staff. Confirm payment and advance the order through pickup.
7. Check that completion leaves four units per component, releases reservations and retains exactly one payment.

This proves the tested workflow does not depend on public demo credentials. Sample component descriptions were entered only into that isolated test store.

## The availability problem found and fixed

Payment and progress originally stayed enabled for Staff even when an order was completed. The business layer rejected invalid changes, but the screen suggested the actions were available.

`OrdersForm.UpdateOrderActions` now checks both the role and the selected order. `OrdersForm_Shown` clears WinForms' automatic first-row selection. `LoadRows` keeps an explicitly selected order after payment/progress when it still matches the filters. Disabled transaction buttons use a muted background.

| Selected row | Staff | Customer | Admin |
| --- | --- | --- | --- |
| No selected row | Record actions disabled | Record actions disabled | Receipt disabled |
| Unexpired Pending Payment | Receipt and Confirm store payment | Receipt and Cancel unpaid order | Receipt only |
| Paid / Processing / Assembly / Ready for Pickup | Receipt and Next order stage | Receipt only | Receipt only |
| Completed / Cancelled | Receipt only | Receipt only | Receipt only |

The added checks exercised Pending Payment, Paid, Completed and Cancelled rows for all three account roles, clearing selection, filtering out a selected row, retained selection after payment, and progression to completion without reselecting. Disabled completed actions produced no duplicate payment or stock deduction. The normal store workflow separately exercised each intermediate stage.

The code reference also had two stale permissions: Admin payment and Staff/Admin cancellation. Both were corrected to match [ROLES.md](ROLES.md), along with its explanation of Admin catalog saves preserving stock.

## Why a function may appear unavailable

- **The menu/button is hidden:** the signed-in role cannot perform it. Admin cannot build/order, adjust stock, confirm payment, advance orders or cancel them manually. Staff cannot manage accounts, catalog details, suppliers, rules or reports. This follows the strict split you approved.
- **An order button is visible but grey:** select an eligible row. Completed/cancelled orders offer receipt viewing; other transactions remain disabled.
- **No component appears in the builder:** that category has no active part with available stock. Admin adds catalog details, then Staff receives stock.
- **No Staff or Customer workspace on this computer yet:** the saved database was inspected read-only. Its only active account is `jerald@gmail.com`, with Admin role. The three old sample accounts are archived. Add your own accounts in User accounts; this audit did not add people or orders to your real store database.

## Remaining manual checks

No missing permitted route was found in the exercised screens. The following have a narrower validation boundary:

| Function | Verified | Still unverified in this screen audit |
| --- | --- | --- |
| Export CSV | Admin report screen, all report types, visible/enabled export control; CSV formatting in regression tests | Successfully choosing a destination through the native Windows Save dialog |
| Receipt Save / Print | Selected receipt route, contents, visible/enabled controls, access restrictions | Successful native Save dialog and physical printer output |
| Backup / Restore | Admin controls available; database backup, validation, password checks and recovery logic in regression tests | Successful native file-picker workflow and restoring a backup from an external drive |
| Designer / Windows scaling | Matching Designer files; runtime fonts, controls and compact working-area checks | Loading in Visual Studio Designer and changing the actual Windows DPI setting |

These pending checks are not counted as successful UI operations. The project remains under active revision. See [STORE_GUIDE.md](STORE_GUIDE.md) for manual store/recovery checks and [VALIDATION.md](VALIDATION.md) for the wider regression coverage.

## Open the recorded evidence

These files are kept outside the source project so no QA or verification folder is added to the app:

- [627-check screen audit report](C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703/custompc-runtime-validation/run-20261013-103328-752-25796/report.json).
- [667-check regression report after the fix](C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703/custompc-runtime-validation/run-20261013-103328-736-27332/report.json).
- [Recorded screen/button inventory](C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703/custompc-runtime-validation/run-20261013-103328-752-25796/screen-inventory.json).
- [Admin dashboard](C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703/custompc-runtime-validation/run-20261013-103328-752-25796/Audit-admin-DashboardForm.png), [Staff dashboard](C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703/custompc-runtime-validation/run-20261013-103328-752-25796/Audit-staff-DashboardForm.png), [Customer dashboard](C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703/custompc-runtime-validation/run-20261013-103328-752-25796/Audit-customer-DashboardForm.png), [Guest dashboard](C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703/custompc-runtime-validation/run-20261013-103328-752-25796/Audit-guest-DashboardForm.png).
- [Completed order with Staff transactions disabled](C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703/custompc-runtime-validation/run-20261013-103328-752-25796/Audit-staff-Completed-order-actions.png).
- [Normal-mode Customer build ready to confirm](C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703/custompc-runtime-validation/run-20261013-103328-752-25796/Audit-normal-customer-Builder-ready.png).

Demo names in some images belong only to isolated fixtures. The normal-mode images use accounts created through setup/User accounts in a separate empty test database. Neither set represents new accounts in your real store.
