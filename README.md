# CustomPC Windows Forms

CustomPC is a Windows Forms application for PC builds, inventory, orders and store payments on one Windows computer. It is under active revision.

## Run and edit

Open `CustomPC.slnx` in Visual Studio with the .NET desktop development workload and .NET 10 installed. Restore packages, build, then press **F5**. The project targets `net10.0-windows10.0.19041`.

Right-click a form's `.cs` file and choose **View Designer** (Shift+F7). Layout lives in `.Designer.cs`; event handlers live in `.cs`; form resources live in `.resx`. There are **16 forms**. Catalog and management pages are hosted inside DashboardForm; login, editors and receipts are dialogs.

Keep `bin` and `obj`: Visual Studio and the Designer need their generated files. A debug EXE is a build output, not a release package. QA tooling and reports are outside the source project.

## Read the right guide

| Need | Guide |
| --- | --- |
| Learn the code step by step | [Beginner lessons](docs/CODE_GUIDE.md) |
| Find a file or understand a data flow | [Code reference](docs/CODE_REFERENCE.md) |
| Use accounts, orders, inventory and backups | [Operating guide](docs/STORE_GUIDE.md) |
| See exact role permissions | [Role matrix](docs/ROLES.md) |
| Review executed tests and manual checks | [Validation evidence](docs/VALIDATION.md) |
| Review researched models, prices and image sources | [Product manifest](docs/CATALOG_MANIFEST.md) |

## Saved data

```text
Database/
├── custompc.db
├── ProductImages/
└── Backups/          created as backups are saved
```

The default Database folder is beside the solution. If the app is copied outside the solution, it uses Database beside the app. `CUSTOMPC_DATABASE_PATH` overrides the path for testing. Close the app before manually copying the whole folder; use a ZIP backup for normal recovery. Database and its images are ignored by Git.

The prepared catalog contains 42 researched products with local photos. They are ordinary SQLite records, not embedded assets or a runtime seed pack. Current stock, accounts, prices and order counts are saved data and can change. Initial prepared Admin credentials were name `admin`, email `admin@gmail.com`, password `admin@gmail.com`; use the saved password if it has since changed. Add Staff and Customer accounts through User accounts.

A missing/empty database opens **Set up CustomPC** to create an administrator. It does not recreate the researched catalog. `CUSTOMPC_DEMO_MODE=1` explicitly enables separate practice fixtures; normal startup does not create demo accounts or stock.

## Current behavior

Admin manages catalog metadata/photos, accounts, reports and **Activity & Backups**. Staff records stock changes, payments and order progress. Customers order and cancel their own unpaid orders. Guests browse and prepare a build; valid checkout asks for login and Customer login continues with the selected parts.

Compatibility checks are mandatory: required categories, stock, graphics output, socket, RAM type and PSU capacity. There are no editable compatibility rules, supplier management, business-name customization or database-location field. Receipts use **CustomPC**.

Admin charts show confirmed payments, top-selling parts and current inventory health. Staff charts show incoming-order counts, current work queue and stock readiness. Date charts offer 7/30/90 days; queue and inventory remain current snapshots.
