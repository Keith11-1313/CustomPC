# Technical overview

## Stack

CustomPC is a C# desktop application targeting `net10.0-windows10.0.19041`. It uses Windows Forms and local SQLite storage. It does not use Entity Framework or a remote application server.

Direct dependencies in `CustomPC/CustomPC.csproj`:

| Package | Version | Use |
| --- | --- | --- |
| Microsoft.Data.Sqlite | 10.0.12 | Database connections and SQL commands |
| SQLitePCLRaw.bundle_e_sqlite3 | 3.0.5 | Native SQLite engine |
| LiveChartsCore.SkiaSharpView.WinForms | 2.0.4 | Admin charts |

## Structure

| Layer | Main files | Responsibility |
| --- | --- | --- |
| Interface | Forms, Designer files, WorkspacePage, EditorSheet | Inputs, navigation, tables, dialogs |
| Business logic | AppStore partial files | Sessions, permissions, validation, orders, inventory |
| Models | Models.cs | Saved record classes |
| Storage | Database.cs, StoreDataPath.cs | Schema, loading, and SQLite writes |

`Program.cs` initializes WinForms, handles setup, and opens login before the dashboard. The dashboard hosts pages; editors, policies, and receipts use dialogs. `BuildDraft` retains selected part IDs across page navigation until checkout or confirmed sign-out.

A save follows **click → read inputs → refresh and check role → validate → write in a transaction → update the view**. Failed business writes roll back and reload memory.

## Database

The default location is `Database/custompc.db`, beside the solution during development or beside a copied executable. An explicit path or `CUSTOMPC_DATABASE_PATH` can select a practice database.

| Table | Contents |
| --- | --- |
| Users | Details, role, archive state, password hash |
| Parts | Details, price, stock, specifications, photo filename |
| Builds | Customer ID and selected part IDs |
| Orders | Customer/build references, status, deadline, line snapshots |
| Payments | Amount, order, Staff reference, timestamp |
| Movements | Quantity change, part, reason, user, timestamp |
| AuditLog | Time, actor, action, details |
| LoginAttempts | Failure count and lockout deadline |

The first six tables use `Id TEXT PRIMARY KEY` and `Data TEXT NOT NULL`. `System.Text.Json` serializes records. Relationships and business constraints are primarily checked in C#, rather than SQL foreign keys. Writes compare JSON and save changed rows using parameterized SQL.

Photographs are managed copies in `Database/ProductImages`. ZIP backups include the database and photographs. Restore validates the backup and creates recovery copies before replacement.

## Authentication

Email matching ignores capitalization and outer spaces. New emails are format-checked and checked for duplicates; mailbox ownership is not verified.

New passwords differ from the email and contain 12–128 characters, uppercase and lowercase letters, a digit, and a symbol. PBKDF2-SHA256 uses a random 16-byte salt, 600,000 iterations, and a 32-byte hash. Verification uses fixed-time comparison. Legacy hashes remain readable until changed.

Five failed attempts for an email cause a five-minute lockout. Admin-assisted reset changes the hash and clears that email's lockout. Business methods recheck permissions independently of visible controls. See [Roles](ROLES.md).

## Compatibility

The builder requires one CPU, motherboard, RAM, storage device, PSU, and case, with at most one dedicated GPU. Omitting the GPU requires integrated CPU graphics and motherboard video output.

Checks cover availability, sockets, memory types, and PSU capacity. Required capacity is the greater of estimated component power plus 100 W or the highest recorded recommended PSU requirement. Staff separately checks BIOS support, connectors, and physical clearance.

## Orders and stock

```text
Pending Payment → Paid → Processing → Assembly → Ready for Pickup → Completed
       └────────→ Cancelled (Customer cancellation or unpaid-order expiry)
```

Checkout rechecks prices, compatibility, and stock inside the transaction. Order lines retain confirmed names and prices. The deadline is creation time plus 24 hours.

Active orders reserve stock. Payment records the full amount without deducting physical stock. Completion deducts stock once, adds movements, and ends reservations. Cancellation/expiry releases reservations without stock deduction. Expiry runs at startup, refresh, and on a timer; it does not run when the app is closed.

## Reports and receipts

Admin reports cover current inventory, orders, movements, and payments. Date-based reports use inclusive local dates. CSV export reads all matching pagination rows. Receipt preview and printing share `ReceiptDocument`; PDF output uses Windows printing. There is no receipt text-file save action.

See [Code reference](CODE_REFERENCE.md) for implementation details and [Testing](VALIDATION.md) for release checks.
