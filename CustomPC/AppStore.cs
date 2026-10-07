using Microsoft.Data.Sqlite;
using System.Security.Cryptography;
using System.Text.Json;

namespace CustomPC;

// Each entity has its own SQLite table. JSON holds the entity fields and order snapshots.
public sealed class AppStore
{
    public static readonly string[] Categories = ["CPU", "Motherboard", "RAM", "GPU", "Storage", "Power Supply", "Case"];
    public static readonly string[] Roles = ["Admin", "Staff", "Customer"];
    public static readonly string[] Statuses = ["Pending Payment", "Paid", "Processing", "Assembly", "Ready for Pickup", "Completed", "Cancelled"];
    private static readonly Lazy<AppStore> Shared = new(() => new AppStore());
    public static AppStore Current => Shared.Value;
    public string DatabasePath { get; }
    public UserAccount? Session { get; private set; }
    public List<UserAccount> Users { get; private set; } = [];
    public List<Part> Parts { get; private set; } = [];
    public List<Supplier> Suppliers { get; private set; } = [];
    public List<CompatibilityRule> Rules { get; private set; } = [];
    public List<PcBuild> Builds { get; private set; } = [];
    public List<CustomerOrder> Orders { get; private set; } = [];
    public List<Payment> Payments { get; private set; } = [];
    public List<InventoryMovement> Movements { get; private set; } = [];
    public bool IsAdmin => Session?.Role == "Admin";
    public bool IsStaff => Session?.Role is "Admin" or "Staff";
    public AppStore(string? path = null)
    {
        DatabasePath = path ?? Environment.GetEnvironmentVariable("CUSTOMPC_DATABASE_PATH") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CustomPC", "custompc.db");
        Directory.CreateDirectory(Path.GetDirectoryName(DatabasePath)!);
        using var db = Open();
        foreach (var table in Tables)
        {
            using var command = db.CreateCommand();
            command.CommandText = $"CREATE TABLE IF NOT EXISTS {table} (Id TEXT PRIMARY KEY, Data TEXT NOT NULL)";
            command.ExecuteNonQuery();
        }
        Load(db);
        if (Users.Count == 0) Seed();
        ExpireOrders();
    }
    private static readonly string[] Tables = ["Users", "Parts", "Suppliers", "Rules", "Builds", "Orders", "Payments", "Movements"];
    private SqliteConnection Open()
    {
        var db = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = DatabasePath }.ToString());
        db.Open();
        return db;
    }
    private static List<T> Read<T>(SqliteConnection db, string table)
    {
        using var cmd = db.CreateCommand(); cmd.CommandText = $"SELECT Data FROM {table}";
        using var reader = cmd.ExecuteReader(); var list = new List<T>();
        while (reader.Read()) list.Add(JsonSerializer.Deserialize<T>(reader.GetString(0))!);
        return list;
    }
    private void Load(SqliteConnection db)
    {
        Users = Read<UserAccount>(db, "Users"); Parts = Read<Part>(db, "Parts"); Suppliers = Read<Supplier>(db, "Suppliers");
        Rules = Read<CompatibilityRule>(db, "Rules"); Builds = Read<PcBuild>(db, "Builds"); Orders = Read<CustomerOrder>(db, "Orders");
        Payments = Read<Payment>(db, "Payments"); Movements = Read<InventoryMovement>(db, "Movements");
        if (Session != null) Session = Users.FirstOrDefault(x => x.Id == Session.Id && !x.Archived);
    }
    private static void Write<T>(SqliteConnection db, SqliteTransaction tx, string table, List<T> values)
    {
        using var clear = db.CreateCommand(); clear.Transaction = tx; clear.CommandText = $"DELETE FROM {table}"; clear.ExecuteNonQuery();
        foreach (var value in values)
        {
            using var cmd = db.CreateCommand(); cmd.Transaction = tx; cmd.CommandText = $"INSERT INTO {table}(Id,Data) VALUES($id,$data)";
            cmd.Parameters.AddWithValue("$id", typeof(T).GetProperty("Id")!.GetValue(value)!);
            cmd.Parameters.AddWithValue("$data", JsonSerializer.Serialize(value)); cmd.ExecuteNonQuery();
        }
    }
    private void Change(Action action)
    {
        using var db = Open(); using var tx = db.BeginTransaction();
        Load(db);
        try
        {
            action();
            Write(db, tx, "Users", Users); Write(db, tx, "Parts", Parts); Write(db, tx, "Suppliers", Suppliers);
            Write(db, tx, "Rules", Rules); Write(db, tx, "Builds", Builds); Write(db, tx, "Orders", Orders);
            Write(db, tx, "Payments", Payments); Write(db, tx, "Movements", Movements);
            tx.Commit();
        }
        catch { tx.Rollback(); Load(db); throw; }
    }
    public void Refresh() { using var db = Open(); Load(db); ExpireOrders(); }
    private void RequireAdmin() { if (!IsAdmin) throw new InvalidOperationException("Administrator access is required."); }
    private void RequireStaff() { if (!IsStaff) throw new InvalidOperationException("Store staff access is required."); }
    public static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 210000, HashAlgorithmName.SHA256, 32);
        return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
    }
    private static bool Verify(string password, string stored)
    {
        var pieces = stored.Split(':');
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, Convert.FromBase64String(pieces[0]), 210000, HashAlgorithmName.SHA256, 32);
        return CryptographicOperations.FixedTimeEquals(hash, Convert.FromBase64String(pieces[1]));
    }
    public void Login(string email, string password)
    {
        Refresh(); var user = Users.FirstOrDefault(x => !x.Archived && x.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase));
        if (user == null || !Verify(password, user.PasswordHash)) throw new InvalidOperationException("Email or password is incorrect, or the account is archived.");
        Session = user;
    }
    public void Logout() => Session = null;
    public void SaveUser(UserAccount user, string password, bool registration = false)
    {
        Change(() =>
        {
            if (!registration) RequireAdmin();
            if (string.IsNullOrWhiteSpace(user.Name) || !System.Net.Mail.MailAddress.TryCreate(user.Email, out _)) throw new InvalidOperationException("Enter a name and valid email address.");
            if (Users.Any(x => x.Id != user.Id && x.Email.Equals(user.Email.Trim(), StringComparison.OrdinalIgnoreCase))) throw new InvalidOperationException("This email address is already registered.");
            var existing = Users.FirstOrDefault(x => x.Id == user.Id);
            if (registration && existing != null) throw new InvalidOperationException("Registration must create a new account.");
            if (registration) { user.Role = "Customer"; user.Archived = false; }
            if (!Roles.Contains(user.Role)) throw new InvalidOperationException("Choose a valid role.");
            if (existing == null || password.Length > 0)
            {
                if (password.Length < 8) throw new InvalidOperationException("Use a password of at least 8 characters.");
                user.PasswordHash = HashPassword(password);
            }
            else user.PasswordHash = existing.PasswordHash;
            if (user.Id == Session?.Id && (user.Archived || user.Role != "Admin")) throw new InvalidOperationException("You cannot archive or demote your current admin account.");
            user.Email = user.Email.Trim(); Users.RemoveAll(x => x.Id == user.Id); Users.Add(user);
        });
    }
    public int Reserved(string id) => Orders.Where(x => x.Status is "Pending Payment" or "Paid" or "Processing" or "Assembly" or "Ready for Pickup").SelectMany(x => x.Lines).Where(x => x.PartId == id).Sum(x => x.Quantity);
    public int Available(Part p) => p.Stock - Reserved(p.Id);
    public string StockState(Part p) => Available(p) == 0 ? "Out of stock" : Available(p) <= p.LowStock ? "Low stock" : "Available";
    public void SavePart(Part part)
    {
        Change(() =>
        {
            RequireAdmin();
            if (string.IsNullOrWhiteSpace(part.Name) || string.IsNullOrWhiteSpace(part.Brand) || !Categories.Contains(part.Category) || part.Price <= 0 || part.Stock < Reserved(part.Id) || part.LowStock < 0 || part.Watts < 0)
                throw new InvalidOperationException("Enter a name, brand, category and positive price. Stock cannot be below reserved quantity.");
            if (part.SupplierId.Length > 0 && !Suppliers.Any(x => x.Id == part.SupplierId && !x.Archived)) throw new InvalidOperationException("Choose an active supplier.");
            var old = Parts.FirstOrDefault(x => x.Id == part.Id);
            if (old?.Stock != part.Stock) Movements.Add(new() { PartId = part.Id, Quantity = part.Stock - (old?.Stock ?? 0), Reason = "Admin stock entry", UserId = Session!.Id });
            Parts.RemoveAll(x => x.Id == part.Id); Parts.Add(part);
        });
    }
    public void AdjustStock(string id, int quantity, string reason)
    {
        Change(() => { RequireStaff(); var part = Parts.Single(x => x.Id == id);
            if (quantity == 0 || string.IsNullOrWhiteSpace(reason) || part.Stock + quantity < Reserved(id)) throw new InvalidOperationException("Enter a nonzero adjustment and a reason. Reserved stock must remain available.");
            part.Stock += quantity; Movements.Add(new() { PartId = id, Quantity = quantity, Reason = reason.Trim(), UserId = Session!.Id }); });
    }
    public void SaveSupplier(Supplier supplier)
    {
        Change(() => { RequireAdmin(); if (string.IsNullOrWhiteSpace(supplier.Name) || string.IsNullOrWhiteSpace(supplier.Contact)) throw new InvalidOperationException("Supplier name and contact are required.");
            Suppliers.RemoveAll(x => x.Id == supplier.Id); Suppliers.Add(supplier); });
    }
    public void SaveRule(CompatibilityRule rule)
    {
        Change(() => { RequireAdmin(); if (string.IsNullOrWhiteSpace(rule.Name) || !new[] { "Socket", "Memory", "Power" }.Contains(rule.Kind)) throw new InvalidOperationException("Enter a rule name and supported rule type.");
            if (Rules.Any(x => x.Id != rule.Id && x.Kind == rule.Kind)) throw new InvalidOperationException("Edit the existing rule of this type.");
            Rules.RemoveAll(x => x.Id == rule.Id); Rules.Add(rule); });
    }
    public List<string> CheckBuild(List<Part> parts)
    {
        var errors = new List<string>();
        foreach (var c in Categories) if (parts.Count(x => x.Category == c) != 1) errors.Add($"Select one {c}.");
        foreach (var p in parts) if (p.Archived || Available(p) < 1) errors.Add($"{p.Name} is unavailable.");
        var cpu = parts.FirstOrDefault(x => x.Category == "CPU"); var board = parts.FirstOrDefault(x => x.Category == "Motherboard");
        var ram = parts.FirstOrDefault(x => x.Category == "RAM"); var psu = parts.FirstOrDefault(x => x.Category == "Power Supply");
        if (Rules.Any(x => x.Enabled && x.Kind == "Socket") && cpu != null && board != null && (cpu.Socket.Length == 0 || !cpu.Socket.Equals(board.Socket, StringComparison.OrdinalIgnoreCase))) errors.Add("CPU and motherboard sockets do not match or are unspecified.");
        if (Rules.Any(x => x.Enabled && x.Kind == "Memory") && ram != null && board != null && (ram.MemoryType.Length == 0 || !ram.MemoryType.Equals(board.MemoryType, StringComparison.OrdinalIgnoreCase))) errors.Add("RAM and motherboard memory types do not match or are unspecified.");
        if (Rules.Any(x => x.Enabled && x.Kind == "Power") && psu != null && (psu.Watts <= 0 || psu.Watts < parts.Where(x => x.Category != "Power Supply").Sum(x => x.Watts) + 100)) errors.Add("Power supply needs at least estimated component wattage plus 100 W headroom.");
        return errors;
    }
    public CustomerOrder CreateOrder(List<string> ids, string customerId)
    {
        CustomerOrder? result = null;
        Change(() =>
        {
            ExpireInMemory();
            if (Session == null) throw new InvalidOperationException("Sign in to place an order.");
            if (!IsStaff && customerId != Session.Id) throw new InvalidOperationException("You can only order for your own account.");
            var customer = Users.SingleOrDefault(x => x.Id == customerId && !x.Archived && x.Role == "Customer") ?? throw new InvalidOperationException("Choose an active customer account.");
            var parts = ids.Select(id => Parts.Single(x => x.Id == id)).ToList(); var errors = CheckBuild(parts);
            if (errors.Count > 0) throw new InvalidOperationException(string.Join(Environment.NewLine, errors));
            var build = new PcBuild { CustomerId = customerId, PartIds = ids.ToList() }; Builds.Add(build);
            result = new CustomerOrder { Number = $"PC-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}", CustomerId = customerId, CustomerName = customer.Name, BuildId = build.Id,
                Lines = parts.Select(p => new OrderLine { PartId = p.Id, Name = p.Name, Price = p.Price }).ToList() };
            Orders.Add(result);
        });
        return result!;
    }
    private void ExpireInMemory() { foreach (var order in Orders.Where(x => x.Status == "Pending Payment" && x.DeadlineUtc <= DateTime.UtcNow)) order.Status = "Cancelled"; }
    public void ExpireOrders() { if (Orders.Any(x => x.Status == "Pending Payment" && x.DeadlineUtc <= DateTime.UtcNow)) Change(ExpireInMemory); }
    public List<CustomerOrder> VisibleOrders() => Orders.Where(x => IsStaff || x.CustomerId == Session?.Id).OrderByDescending(x => x.CreatedUtc).ToList();
    public void ConfirmPayment(string id)
    {
        ExpireOrders();
        Change(() => { RequireStaff(); var o = Orders.Single(x => x.Id == id);
            if (o.Status != "Pending Payment" || o.DeadlineUtc <= DateTime.UtcNow) throw new InvalidOperationException("Only an unexpired pending order can be paid.");
            Payments.Add(new() { OrderId = id, StaffId = Session!.Id, Amount = o.Total }); o.Status = "Paid"; });
    }
    public void AdvanceOrder(string id)
    {
        Change(() => { RequireStaff(); var o = Orders.Single(x => x.Id == id); var index = Array.IndexOf(Statuses, o.Status);
            if (index < 1 || index >= 5) throw new InvalidOperationException("Confirm payment first, or this order already ended.");
            o.Status = Statuses[index + 1];
            if (o.Status == "Completed") foreach (var line in o.Lines) { Parts.Single(x => x.Id == line.PartId).Stock -= line.Quantity; Movements.Add(new() { PartId = line.PartId, Quantity = -line.Quantity, Reason = "Completed " + o.Number, UserId = Session!.Id }); }
        });
    }
    public void CancelOrder(string id)
    {
        Change(() => { var o = Orders.Single(x => x.Id == id);
            if (Session == null || (!IsStaff && o.CustomerId != Session.Id)) throw new InvalidOperationException("You cannot cancel this order.");
            if (o.Status != "Pending Payment") throw new InvalidOperationException("Only unpaid pending orders can be cancelled."); o.Status = "Cancelled"; });
    }
    private void Seed()
    {
        Change(() =>
        {
            foreach (var role in Roles) Users.Add(new() { Name = role == "Customer" ? "Demo Customer" : role + " Demo", Email = role.ToLowerInvariant() + "@custompc.local", Role = role, PasswordHash = HashPassword("CustomPC123!") });
            var supplier = new Supplier { Name = "Demo Parts Distributor", Contact = "0917 000 0000", Email = "supplier@example.com", Address = "Caloocan City" }; Suppliers.Add(supplier);
            void Add(string name, string category, string brand, decimal price, string spec, string socket = "", string memory = "", int watts = 0) => Parts.Add(new() { Name = name, Category = category, Brand = brand, Price = price, Specifications = spec, Socket = socket, MemoryType = memory, Watts = watts, Stock = 10, SupplierId = supplier.Id });
            Add("Ryzen 5 5600", "CPU", "AMD", 6500, "6 cores / 12 threads", "AM4", watts: 65);
            Add("Core i5-12400F", "CPU", "Intel", 8000, "6 cores / 12 threads", "LGA1700", watts: 65);
            Add("B550M motherboard", "Motherboard", "MSI", 5500, "Micro ATX / AM4 / DDR4", "AM4", "DDR4", 40);
            Add("B660M motherboard", "Motherboard", "ASUS", 6500, "Micro ATX / LGA1700 / DDR4", "LGA1700", "DDR4", 40);
            Add("16 GB DDR4 kit", "RAM", "Kingston", 2500, "2 x 8 GB / 3200 MHz", memory: "DDR4", watts: 10);
            Add("16 GB DDR5 kit", "RAM", "Kingston", 3500, "2 x 8 GB / 5200 MHz", memory: "DDR5", watts: 10);
            Add("GeForce RTX 3060", "GPU", "NVIDIA", 14000, "12 GB GDDR6", watts: 170);
            Add("Radeon RX 6600", "GPU", "AMD", 12000, "8 GB GDDR6", watts: 132);
            Add("500 GB NVMe SSD", "Storage", "WD", 2200, "M.2 / PCIe NVMe", watts: 8);
            Add("1 TB NVMe SSD", "Storage", "Samsung", 4000, "M.2 / PCIe NVMe", watts: 8);
            Add("650 W Bronze PSU", "Power Supply", "Corsair", 3500, "650 W / 80 Plus Bronze", watts: 650);
            Add("Micro ATX tower", "Case", "Tecware", 2000, "Micro ATX / mesh front");
            Rules.AddRange([new() { Name = "CPU / motherboard socket", Kind = "Socket" }, new() { Name = "RAM / motherboard memory", Kind = "Memory" }, new() { Name = "PSU capacity (+100 W)", Kind = "Power" }]);
        });
    }
}
