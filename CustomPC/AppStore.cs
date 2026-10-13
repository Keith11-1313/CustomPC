using Microsoft.Data.Sqlite;
using System.Runtime.CompilerServices;

namespace CustomPC;

// Forms share this object: the signed-in user, records, and business operations.
// AppStore.*.cs files are sections of this same class, like a form's Designer file.
public sealed partial class AppStore
{
    public static readonly string[] Categories =
    {
        "CPU", "Motherboard", "RAM", "GPU", "Storage", "Power Supply", "Case"
    };

    public static readonly string[] Roles = { "Admin", "Staff", "Customer" };

    public static readonly string[] Statuses =
    {
        "Pending Payment", "Paid", "Processing", "Assembly",
        "Ready for Pickup", "Completed", "Cancelled"
    };

    private static AppStore? currentStore;
    private readonly Database database;

    public static AppStore Current
    {
        get
        {
            // Only a running form needs to open the database; the Designer does not.
            if (currentStore == null)
            {
                currentStore = new AppStore();
            }

            return currentStore;
        }
    }

    public string DatabasePath { get; }
    public UserAccount? Session { get; private set; }
    public bool NeedsSetup => Users.Count == 0;
    public static bool DemoMode => Environment.GetEnvironmentVariable("CUSTOMPC_DEMO_MODE") == "1";

    public List<UserAccount> Users { get; private set; } = new List<UserAccount>();
    public List<Part> Parts { get; private set; } = new List<Part>();
    public List<Supplier> Suppliers { get; private set; } = new List<Supplier>();
    public List<CompatibilityRule> Rules { get; private set; } = new List<CompatibilityRule>();
    public List<PcBuild> Builds { get; private set; } = new List<PcBuild>();
    public List<CustomerOrder> Orders { get; private set; } = new List<CustomerOrder>();
    public List<Payment> Payments { get; private set; } = new List<Payment>();
    public List<InventoryMovement> Movements { get; private set; } = new List<InventoryMovement>();

    public bool IsAdmin
    {
        get { return Session != null && Session.Role == "Admin"; }
    }

    public bool IsStaff
    {
        get { return Session != null && Session.Role == "Staff"; }
    }

    public bool IsCustomer => Session != null && Session.Role == "Customer";

    // Shared read access is explicit. Admin does not inherit Staff or Customer actions.
    public bool CanViewInventory => IsAdmin || IsStaff;
    public bool CanViewAllOrders => IsAdmin || IsStaff;

    public AppStore(string? path = null)
    {
        string? customPath = path;
        if (string.IsNullOrWhiteSpace(customPath))
        {
            customPath = Environment.GetEnvironmentVariable("CUSTOMPC_DATABASE_PATH");
        }

        if (string.IsNullOrWhiteSpace(customPath))
        {
            customPath = StoreDataPath.DefaultDatabase();
        }

        DatabasePath = Path.GetFullPath(customPath);
        database = new Database(DatabasePath);
        // Preserve existing data before applying a schema update. Backups include SQLite's live state.
        EnsureDailyBackup();
        database.CreateTables();

        using (SqliteConnection connection = database.OpenConnection())
        {
            LoadData(connection);
        }

        if (Users.Count == 0 && DemoMode)
        {
            CreateSampleData();
        }

        ExpireOrders();
    }

    public void Refresh()
    {
        using (SqliteConnection connection = database.OpenConnection())
        {
            LoadData(connection);
        }

        ExpireOrders();
    }

    private void LoadData(SqliteConnection connection)
    {
        // Forms use these typed lists instead of writing SQL themselves.
        Users = database.ReadRecords<UserAccount>(connection, "Users");
        Parts = database.ReadRecords<Part>(connection, "Parts");
        Suppliers = database.ReadRecords<Supplier>(connection, "Suppliers");
        Rules = database.ReadRecords<CompatibilityRule>(connection, "Rules");
        Builds = database.ReadRecords<PcBuild>(connection, "Builds");
        Orders = database.ReadRecords<CustomerOrder>(connection, "Orders");
        Payments = database.ReadRecords<Payment>(connection, "Payments");
        Movements = database.ReadRecords<InventoryMovement>(connection, "Movements");

        // An archived account or changed role must affect an already signed-in user too.
        if (Session != null)
        {
            string signedInId = Session.Id;
            Session = null;

            foreach (UserAccount account in Users)
            {
                if (account.Id == signedInId && !account.Archived)
                {
                    Session = account;
                    break;
                }
            }
        }
    }

    private void SaveData(SqliteConnection connection, SqliteTransaction transaction,
        [CallerMemberName] string action = "")
    {
        // Related records succeed together, such as a payment and its order status.
        database.ChangedRecords.Clear();
        int changed = database.WriteRecords(connection, transaction, "Users", Users);
        changed += database.WriteRecords(connection, transaction, "Parts", Parts);
        changed += database.WriteRecords(connection, transaction, "Suppliers", Suppliers);
        changed += database.WriteRecords(connection, transaction, "Rules", Rules);
        changed += database.WriteRecords(connection, transaction, "Builds", Builds);
        changed += database.WriteRecords(connection, transaction, "Orders", Orders);
        changed += database.WriteRecords(connection, transaction, "Payments", Payments);
        changed += database.WriteRecords(connection, transaction, "Movements", Movements);
        if (changed > 0)
        {
            List<string> details = new List<string>();
            foreach (var record in database.ChangedRecords.Take(20))
            {
                string description = record.Table switch
                {
                    "Orders" => "Order " + Orders.Find(x => x.Id == record.Id)?.Number,
                    "Parts" => "Part " + Parts.Find(x => x.Id == record.Id)?.Name,
                    "Users" => "Account " + Users.Find(x => x.Id == record.Id)?.Name,
                    "Suppliers" => "Supplier " + Suppliers.Find(x => x.Id == record.Id)?.Name,
                    _ => record.Table + " " + record.Id[..Math.Min(8, record.Id.Length)]
                };
                details.Add(description);
            }
            WriteAudit(connection, transaction, action, changed + " records changed: " + string.Join("; ", details));
        }
        transaction.Commit();
    }

    private void UndoFailedChange(SqliteConnection connection, SqliteTransaction transaction)
    {
        // Undo SQLite changes and reload the lists too, so the UI cannot display a failed edit.
        transaction.Rollback();
        LoadData(connection);
    }

    private void RequireAdmin()
    {
        if (!IsAdmin)
        {
            throw new InvalidOperationException("Administrator access is required.");
        }
    }

    private void RequireStaff()
    {
        if (!IsStaff)
        {
            throw new InvalidOperationException("Store staff access is required.");
        }
    }

    private void RequireCustomer()
    {
        if (!IsCustomer)
        {
            throw new InvalidOperationException("Customer access is required. Sign in with a customer account.");
        }
    }

    private Part FindPart(string partId)
    {
        foreach (Part part in Parts)
        {
            if (part.Id == partId)
            {
                return part;
            }
        }

        throw new InvalidOperationException("The selected component no longer exists.");
    }

    private CustomerOrder FindOrder(string orderId)
    {
        foreach (CustomerOrder order in Orders)
        {
            if (order.Id == orderId)
            {
                return order;
            }
        }

        throw new InvalidOperationException("The selected order no longer exists.");
    }
}
