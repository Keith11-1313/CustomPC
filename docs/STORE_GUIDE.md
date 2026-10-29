# User guide

## Accounts and navigation

A new database opens **Set up CustomPC** to create the first Admin. Otherwise, sign in with an existing account. Customers can register from login; registration requires agreement to the Terms & Conditions and acknowledgement of the Privacy Notice. Guest access supports browsing and build previews.

New passwords require 12–128 characters, uppercase and lowercase letters, a number, and a symbol, and must differ from the account email. Forgotten-password assistance is handled by shop personnel. Admin uses **User accounts → Reset password** after checking identity. Reset clears the account's failed-login lockout; archived accounts must be reactivated first.

The sidebar changes pages. Overview or Back returns to the dashboard. Cancel closes an editor without saving. Confirmed sign-out clears an unfinished build. Roles have separate permissions; see [Roles](ROLES.md).

## Customer: build and order

1. Open **Parts catalog**. Search or filter products and inspect details.
2. Add parts to a build, then open **PC builder**.
3. Select CPU, motherboard, RAM, storage, PSU, and case. A dedicated GPU can be omitted only with integrated CPU graphics and motherboard video output.
4. Resolve the compatibility messages and review the total.
5. Confirm the order. Guest **Log in and order** retains selections and continues after Customer login.
6. Open **My orders** to track progress or view a receipt.

The builder checks availability, socket, memory, graphics, and PSU requirements. Staff still verifies physical fit, connectors, and BIOS support.

Orders reserve components for a 24-hour payment window. Pay at the physical store before the displayed deadline. Customers can cancel their own unpaid pending orders. Cancellation or expiry releases reservations. Paid-order concerns go through the service counter.

Receipts provide **Print / save PDF**, using the Windows print dialog and Microsoft Print to PDF when available. There is no text-receipt save action.

## Staff: stock and fulfilment

### Receive stock

Open **Parts & inventory**, select a part, and choose **Adjust stock**. Enter a positive quantity for receipts or a negative quantity for a correction, with a reason. Zero changes, missing reasons, and reductions below reserved stock are rejected.

**Available = On hand − Reserved**

### Process an order

1. Open **Orders & payments** and select an unexpired pending order.
2. Receive the full payment at the store, then choose **Confirm store payment**.
3. Use **Next order stage** as assembly progresses.
4. Mark Completed when the customer collects the PC.

```text
Pending Payment → Paid → Processing → Assembly → Ready for Pickup → Completed
```

Payment leaves on-hand stock unchanged. Completion deducts it once and records stock movements. Buttons depend on the selected order and its status. Staff cannot place Customer orders, cancel them manually, or perform Admin management.

## Admin: products and accounts

### Products

Use **Parts & inventory** to add, edit, archive, or reactivate products. Maintain prices, specifications, and low-stock thresholds. New products start at zero stock; Staff records received quantities.

Category-specific inputs cover sockets, memory, graphics, and power. Admin catalog changes preserve Staff-managed stock. Photo import accepts JPG/PNG up to 15 MB and copies the image into managed storage. Deleting the original upload does not remove the saved photograph. Missing photographs use a placeholder.

### Accounts

Use **User accounts** to add accounts, assign roles, edit details, or archive/reactivate users. Archived accounts cannot sign in. Admin cannot archive or demote their own signed-in account. Changing a saved role removes its former permissions on refresh or the next protected operation.

Use **Reset password** for password changes after checking the account holder's identity. This is assisted recovery, not email-based recovery or a temporary-password workflow.

## Reports

Admin reports cover Inventory, Orders, Stock movements, and Sales. Historical reports use inclusive local dates. Inventory shows current stock, so dates do not apply. Sales represents confirmed payments, not unpaid order values.

**Export CSV** includes every matching record, not just the visible page. Visible data updates automatically as saved records change. Search and filters apply across all result pages; changing them returns to page one.

## Activity & Backups

Admin can review the latest 500 activity entries and double-click an entry for its details. The history records successful saved changes; it does not reconstruct activity from before logging existed.

### Save

Choose **Save backup** and a new filename. The ZIP includes `custompc.db` and `ProductImages`. Existing backup files are not overwritten. A startup backup is also created once per local date when opening an existing database.

### Restore

1. Choose **Restore backup** and verify the current Admin password.
2. Select a backup and confirm replacement.
3. After restoration closes the app, reopen CustomPC.
4. Sign in with credentials that existed in the restored backup.

Restoration creates safety recovery files before replacing records and photographs. Older database-only `.db` backups do not recover missing images. If recovery fails, preserve the named recovery files. If corruption prevents login, keep the damaged file and seek maintainer assistance rather than repeatedly deleting the database.

## Policies and operating limits

Terms & Conditions, Warranty & Returns, and Privacy Notice are available from relevant registration, product, checkout, and receipt screens. Warranty claims and privacy requests remain shop procedures. Policy acknowledgement is not a versioned consent-history feature.

The application uses one local workstation. Payment and pickup happen in-store; there is no online payment or computer-to-computer synchronization. Unpaid-order expiry runs while the app is open or when it next starts, not while it is closed.

For installation and record transfer, see [Installation](INSTALLATION.md). See [the screenshot gallery](screenshots/README.md) for example screens and [Testing](VALIDATION.md) for the release checklist.
