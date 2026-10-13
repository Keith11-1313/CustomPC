# Current database revision

The product data now lives in Database/custompc.db and Database/ProductImages, with one Admin and no transactions. The current 55-check report is in [VALIDATION.md](VALIDATION.md). [Current catalog render](C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703/data-folder-build/bin/DataValidation/debug/results-20261014-020102/Catalog.png). The five-screen previews and metrics below document the previous UI revision with isolated practice records; the starter importer has since been removed.

# Modern screen review — 14 October 2026

Runtime controls were operated by the external WinForms harness and rendered with DrawToBitmap. These are actual form controls and saved isolated test data. They are not browser mockups or proof of human mouse/Visual Studio Designer testing.

The full regression passed **750 checks, 0 failures**. The final modern suite passed **234 checks, 0 failures**. Coverage overlaps; do not add these totals. See [VALIDATION.md](VALIDATION.md) for run evidence and manual checks.

## Review the five requested screens

| Screen | Preview | What to inspect |
| --- | --- | --- |
| Catalog | [Open](C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703/modern-ui-build/bin/ModernValidation/debug/previews-20261014-014901/Catalog.png) | Photos, prices, search/category/stock filters, Add to build, card scrolling |
| Builder | [Open](C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703/modern-ui-build/bin/ModernValidation/debug/previews-20261014-014901/Builder.png) | Required selections, optional GPU, total and validation, checkout panel |
| Login | [Open](C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703/modern-ui-build/bin/ModernValidation/debug/previews-20261014-014901/Login.png) | Logo, brand panel, white auth surface and focused account actions |
| Admin | [Open populated](C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703/modern-ui-build/bin/ModernValidation/debug/previews-20261014-014901/Admin-populated.png) | Confirmed-payment sales, paid-part quantities, current stock health and selectors |
| Staff | [Open populated](C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703/modern-ui-build/bin/ModernValidation/debug/previews-20261014-014901/Staff-populated.png) | Incoming counts, current work queue and category stock counts; no sales amounts |

[Customer first-order state](C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703/modern-ui-build/bin/ModernValidation/debug/previews-20261014-014901/Customer.png) · [No matching products](C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703/modern-ui-build/bin/ModernValidation/debug/previews-20261014-014901/Catalog-empty-search.png) · [Admin zero-payment period](C:/Users/Jerald/.codex/visualizations/2026/10/07/01a11539-e782-7ae3-bb38-3eafc37ec703/modern-ui-build/bin/ModernValidation/debug/previews-20261014-014901/Admin.png)

Populated screenshots use saved practice receipts/payments in an isolated database. Managed-scale screenshots in the same directory simulate 100%, 125%, 150%; at larger sizes, scrolling is required. The rollback fixture deliberately revises one reference price in those scale previews; it is not a retailer quote or a live-store change.

## Role routes and actions

| Role | Pages | Permitted actions |
| --- | --- | --- |
| Admin | Overview, Catalog, View orders, Inventory, Suppliers, Accounts, Rules, Reports, Store tools | Product metadata/photos/starter import; account roles; supplier/rules/settings; financial reports and ZIP recovery. Orders and inventory transactions are read-only. |
| Staff | Overview, Catalog, Orders & payments, Inventory | Actual stock receipts/adjustments with reason, payment confirmation, Processing/Assembly/Ready/Completed. Operational charts only. |
| Customer | Overview, Catalog, Builder, My orders | Select parts, own build checkout, own unpaid cancellation, own receipt. |
| Guest | Overview, Catalog, Builder preview | Browse/select; valid checkout asks for login, then continues only for Customer. Sign in replaces Sign out. No personal order panel. |

Page forms live inside one main window. Back returns to Overview. Editor/receipt/login dialogs remain focused. BuildDraft retains selected category IDs between Catalog and Builder. Sign out clears the draft. Guest-to-Customer checkout preserves it until the owned build and order are saved.

Payments/progress buttons require a selected eligible row. Completed/cancelled orders have no transaction action. Zero-stock Add to build and invalid checkout are disabled with availability/validation text. Forbidden role actions are hidden and business methods independently reject them.

## Manual review

Run the practice database described in STORE_GUIDE. Tab through filters/cards/checkout/period selectors, scroll to the last product and Save/Back buttons, verify images after deleting originals, and complete an actual native-dialog/printer/external-drive test. Designer construction and managed scaling evidence do not replace those checks.