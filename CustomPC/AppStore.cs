using Microsoft.Data.Sqlite;

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
        get
        {
            if (Session == null)
            {
                return false;
            }

            return Session.Role == "Admin" || Session.Role == "Staff";
        }
    }

    public AppStore(string? path = null)
    {
        string? customPath = path;
        if (string.IsNullOrWhiteSpace(customPath))
        {
            customPath = Environment.GetEnvironmentVariable("CUSTOMPC_DATABASE_PATH");
        }

        if (string.IsNullOrWhiteSpace(customPath))
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            customPath = Path.Combine(appData, "CustomPC", "custompc.db");
        }

        DatabasePath = Path.GetFullPath(customPath);
        database = new Database(DatabasePath);
        database.CreateTables();

        using (SqliteConnection connection = database.OpenConnection())
        {
            LoadData(connection);
        }

        if (Users.Count == 0)
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

    private void SaveData(SqliteConnection connection, SqliteTransaction transaction)
    {
        // Related records succeed together, such as a payment and its order status.
        database.WriteRecords(connection, transaction, "Users", Users);
        database.WriteRecords(connection, transaction, "Parts", Parts);
        database.WriteRecords(connection, transaction, "Suppliers", Suppliers);
        database.WriteRecords(connection, transaction, "Rules", Rules);
        database.WriteRecords(connection, transaction, "Builds", Builds);
        database.WriteRecords(connection, transaction, "Orders", Orders);
        database.WriteRecords(connection, transaction, "Payments", Payments);
        database.WriteRecords(connection, transaction, "Movements", Movements);
        transaction.Commit();
    }

    private void UndoFailedChange(SqliteConnection connection, SqliteTransaction transaction)
    {
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
