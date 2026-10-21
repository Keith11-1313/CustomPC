using Microsoft.Data.Sqlite;
using System.Data;
using System.Globalization;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace CustomPC;

// Setup, store details, audit history and recovery belong here rather than in forms.
public sealed partial class AppStore
{
    public string BackupFolder => Path.Combine(Path.GetDirectoryName(DatabasePath)!, "Backups");



    public void CompleteSetup(string adminName, string email, string password)
    {
        using SqliteConnection connection = database.OpenConnection();
        using SqliteTransaction transaction = connection.BeginTransaction();
        try
        {
            LoadData(connection);
            if (!NeedsSetup) throw new InvalidOperationException("Store setup has already been completed.");
            if (
                string.IsNullOrWhiteSpace(adminName) || adminName.Length > 100 || !MailAddress.TryCreate(email, out _))
                throw new InvalidOperationException("Enter an administrator name and valid email address.");
            ValidateNewPassword(password, email);
            Users.Add(new UserAccount { Name = adminName.Trim(), Email = email.Trim(), Role = "Admin", PasswordHash = HashPassword(password) });
            SaveData(connection, transaction);
        }
        catch { UndoFailedChange(connection, transaction); throw; }
    }

    private void WriteAudit(SqliteConnection connection, SqliteTransaction transaction, string action, string details)
    {
        using SqliteCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "INSERT INTO AuditLog (CreatedUtc, ActorId, Action, Details) VALUES ($time, $actor, $action, $details)";
        command.Parameters.AddWithValue("$time", DateTime.UtcNow.ToString("O"));
        command.Parameters.AddWithValue("$actor", Session?.Id ?? "System");
        command.Parameters.AddWithValue("$action", action);
        // Never put passwords, hashes or raw account data in the activity log.
        command.Parameters.AddWithValue("$details", details);
        command.ExecuteNonQuery();
    }

    public DataTable ActivityHistory()
    {
        Refresh(); RequireAdmin();
        DataTable table = new DataTable();
        table.Columns.Add("Time"); table.Columns.Add("User"); table.Columns.Add("Action"); table.Columns.Add("Details");
        using SqliteConnection connection = database.OpenConnection();
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "SELECT CreatedUtc, ActorId, Action, Details FROM AuditLog ORDER BY Id DESC LIMIT 500";
        using SqliteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            string actor = reader.GetString(1);
            string name = Users.Find(u => u.Id == actor)?.Name ?? actor;
            string action = reader.GetString(2) switch
            {
                "CompleteSetup" => "Store set up", "CreateSampleData" => "Demo data created",
                "SaveUser" => "Account saved", "SavePart" => "Part saved",
                "AdjustStock" => "Stock adjusted", "CreateOrder" => "Order confirmed",
                "ConfirmPayment" => "Payment recorded", "AdvanceOrder" => "Order stage updated",
                "CancelOrder" => "Order cancelled", "ExpireOrders" => "Unpaid orders expired",
                _ => reader.GetString(2)
            };
            table.Rows.Add(DateTime.Parse(reader.GetString(0), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind).ToLocalTime().ToString("yyyy-MM-dd HH:mm"), name, action, reader.GetString(3));
        }
        return table;
    }

    private string DescribeActivityRecord(string table, string id)
    {
        switch (table)
        {
            case "Orders":
                CustomerOrder? order = Orders.Find(x => x.Id == id);
                return order == null ? "Order unavailable" : $"Order {order.Number}: {order.Status}; customer {order.CustomerName}; total PHP {order.Total:N2}";
            case "Parts":
                Part? part = Parts.Find(x => x.Id == id);
                return part == null ? "Product unavailable" : $"Product {part.Name}: PHP {part.Price:N2}; on hand {part.Stock}; {(part.Archived ? "archived" : "active")}";
            case "Users":
                UserAccount? user = Users.Find(x => x.Id == id);
                return user == null ? "Account unavailable" : $"Account {user.Name}: role {user.Role}; {(user.Archived ? "archived" : "active")}";
            case "Payments":
                Payment? payment = Payments.Find(x => x.Id == id);
                return payment == null ? "Payment unavailable" : $"Payment PHP {payment.Amount:N2} for order {Orders.Find(x => x.Id == payment.OrderId)?.Number}";
            case "Movements":
                InventoryMovement? movement = Movements.Find(x => x.Id == id);
                return movement == null ? "Stock movement unavailable" : $"Stock {Parts.Find(x => x.Id == movement.PartId)?.Name}: {movement.Quantity:+0;-0;0} units; reason: {movement.Reason}";
            case "Builds":
                PcBuild? build = Builds.Find(x => x.Id == id);
                return build == null ? "Build unavailable" : $"Build for {Users.Find(x => x.Id == build.CustomerId)?.Name}: {build.PartIds.Count} components";
            default: return table + " updated";
        }
    }

    public int DefaultPasswordAccounts()
    {
        // Only the three known legacy/demo addresses need this migration warning.
        return Users.Count(u => !u.Archived && u.Email.EndsWith("@custompc.local", StringComparison.OrdinalIgnoreCase)
            && VerifyPassword("CustomPC123!", u.PasswordHash));
    }

    private void EnsureDailyBackup()
    {
        if (!File.Exists(DatabasePath) || new FileInfo(DatabasePath).Length == 0) return;
        Directory.CreateDirectory(BackupFolder);
        string target = Path.Combine(BackupFolder, "automatic-" + DateTime.Today.ToString("yyyy-MM-dd") + ".zip");
        if (!File.Exists(target)) CreateCompleteBackup(target);
    }

    public void CreateBackup(string target)
    {
        Refresh(); RequireAdmin();
        if (Path.GetExtension(target).Equals(".zip", StringComparison.OrdinalIgnoreCase))
        {
            CreateCompleteBackup(target);
            return;
        }
        CopyDatabase(DatabasePath, target);
    }

    private static void CopyDatabase(string source, string target)
    {
        source = Path.GetFullPath(source); target = Path.GetFullPath(target);
        if (source.Equals(target, StringComparison.OrdinalIgnoreCase) || File.Exists(target))
            throw new InvalidOperationException("Choose a new backup file. Existing files are never overwritten.");
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        // CreateNew claims the destination so a mistaken filename cannot overwrite store data.
        using (FileStream file = new FileStream(target, FileMode.CreateNew)) { }
        try
        {
            using SqliteConnection from = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = source, Mode = SqliteOpenMode.ReadOnly, Pooling = false }.ToString());
            using SqliteConnection to = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = target, Pooling = false }.ToString());
            from.Open(); to.Open(); from.BackupDatabase(to);
            using SqliteCommand check = to.CreateCommand(); check.CommandText = "PRAGMA integrity_check";
            if ((string?)check.ExecuteScalar() != "ok") throw new InvalidOperationException("The backup failed its integrity check.");
        }
        catch { File.Delete(target); throw; }
    }

    public void RestoreBackup(string source, string adminPassword)
    {
        Refresh(); RequireAdmin();
        if (!VerifyPassword(adminPassword, Session!.PasswordHash)) throw new InvalidOperationException("Administrator password is incorrect.");
        if (Path.GetExtension(source).Equals(".zip", StringComparison.OrdinalIgnoreCase))
        {
            RestoreCompleteBackup(source, adminPassword);
            return;
        }
        source = Path.GetFullPath(source);
        if (source.Equals(DatabasePath, StringComparison.OrdinalIgnoreCase) || !File.Exists(source))
            throw new InvalidOperationException("Select a different, existing CustomPC backup file.");
        // Validate every business record before touching the live file.
        using SqliteConnection from = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = source, Mode = SqliteOpenMode.ReadOnly, Pooling = false }.ToString());
        from.Open();
        using SqliteCommand check = from.CreateCommand(); check.CommandText = "PRAGMA integrity_check";
        if ((string?)check.ExecuteScalar() != "ok") throw new InvalidOperationException("This backup failed its integrity check.");
        check.CommandText = "PRAGMA user_version";
        if (Convert.ToInt32(check.ExecuteScalar()) > 1) throw new InvalidOperationException("Install the newer CustomPC version before restoring this backup.");
        List<UserAccount> accounts = database.ReadRecords<UserAccount>(from, "Users");
        if (!accounts.Any(u => u.Role == "Admin" && !u.Archived && ValidStoredHash(u.PasswordHash)
            && (DemoMode || !VerifyPassword("CustomPC123!", u.PasswordHash))))
            throw new InvalidOperationException("The backup must contain an active administrator with a personal password.");
        List<Part> parts = database.ReadRecords<Part>(from, "Parts");
        List<PcBuild> builds = database.ReadRecords<PcBuild>(from, "Builds");
        List<CustomerOrder> orders = database.ReadRecords<CustomerOrder>(from, "Orders");
        List<Payment> payments = database.ReadRecords<Payment>(from, "Payments");
        List<InventoryMovement> movements = database.ReadRecords<InventoryMovement>(from, "Movements");
        bool invalid = accounts.Any(u => !MailAddress.TryCreate(u.Email, out _) || !Roles.Contains(u.Role) || !ValidStoredHash(u.PasswordHash))
            || parts.Any(p => p.Stock < 0 || p.Stock > MaximumStock || p.Price <= 0 || !Categories.Contains(p.Category)
                || p.Watts < 0 || p.Watts > MaximumComponentWatts || p.RecommendedPsuWatts < 0 || p.RecommendedPsuWatts > MaximumComponentWatts
                || p.LowStock < 0 || p.LowStock > MaximumStock || string.IsNullOrWhiteSpace(p.Name) || string.IsNullOrWhiteSpace(p.Brand))

            || orders.Any(o => !accounts.Any(u => u.Id == o.CustomerId) || !builds.Any(b => b.Id == o.BuildId) ||
                !Statuses.Contains(o.Status) || o.Lines == null || o.Lines.Any(l => l.Quantity <= 0 || l.Price <= 0 || !parts.Any(p => p.Id == l.PartId)))
            || payments.Any(p => p.Amount <= 0 || !orders.Any(o => o.Id == p.OrderId) || !accounts.Any(u => u.Id == p.StaffId))
            || movements.Any(m => !parts.Any(p => p.Id == m.PartId));
        if (invalid) throw new InvalidOperationException("The backup contains invalid or disconnected records. Select a verified CustomPC backup.");
        foreach (Part part in parts)
            if (part.PhotoFileName.Length > 0) ProductPhotos.Resolve(DatabasePath, part.PhotoFileName);
        CreateCompleteBackup(Path.Combine(BackupFolder, "before-restore-" + DateTime.Now.ToString("yyyyMMdd-HHmmss-fff") + "-" + Guid.NewGuid().ToString("N") + ".zip"));
        string safety = Path.Combine(BackupFolder, "before-restore-" + DateTime.Now.ToString("yyyyMMdd-HHmmss-fff") + ".db");
        CopyDatabase(DatabasePath, safety);
        using SqliteConnection destination = database.OpenConnection();
        from.BackupDatabase(destination);
        database.CreateTables();
        Session = null;
        LoadData(destination);
        using SqliteTransaction audit = destination.BeginTransaction();
        WriteAudit(destination, audit, "Backup restored", "Current data preserved in a safety backup before recovery");
        audit.Commit();
    }

    private static bool ValidStoredHash(string value)
    {
        if (string.IsNullOrEmpty(value)) return false;
        string[] fields = value.Split(':');
        int index = 0;
        if (fields.Length == 4 && fields[0] == "PBKDF2-SHA256")
        {
            if (!int.TryParse(fields[1], out int rounds) || rounds < 210000 || rounds > 1000000) return false;
            index = 2;
        }
        else if (fields.Length != 2) return false;
        try { return Convert.FromBase64String(fields[index]).Length == 16 && Convert.FromBase64String(fields[index + 1]).Length == 32; }
        catch (FormatException) { return false; }
    }

    private static string LoginKey(string email) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(email.Trim().ToUpperInvariant())));

    private void CheckLoginThrottle(string email)
    {
        using SqliteConnection connection = database.OpenConnection();
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "SELECT LockedUntilUtc FROM LoginAttempts WHERE Key = $key";
        command.Parameters.AddWithValue("$key", LoginKey(email));
        string? until = command.ExecuteScalar()?.ToString();
        if (DateTime.TryParse(until, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime locked) && locked > DateTime.UtcNow)
            throw new InvalidOperationException("Too many failed attempts. Wait five minutes before trying this email again.");
    }

    private void RecordLoginResult(string email, bool success)
    {
        using SqliteConnection connection = database.OpenConnection();
        using SqliteCommand command = connection.CreateCommand();
        command.Parameters.AddWithValue("$key", LoginKey(email));
        if (success) command.CommandText = "DELETE FROM LoginAttempts WHERE Key = $key";
        else
        {
            command.CommandText = """
                INSERT INTO LoginAttempts (Key, Failures, LockedUntilUtc) VALUES ($key, 1, '')
                ON CONFLICT(Key) DO UPDATE SET
                Failures = CASE WHEN LockedUntilUtc <> '' AND LockedUntilUtc <= $now THEN 1 ELSE Failures + 1 END,
                LockedUntilUtc = CASE WHEN LockedUntilUtc <> '' AND LockedUntilUtc <= $now THEN ''
                    WHEN Failures + 1 >= 5 THEN $until ELSE '' END
                """;
            command.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O"));
            command.Parameters.AddWithValue("$until", DateTime.UtcNow.AddMinutes(5).ToString("O"));
        }
        command.ExecuteNonQuery();
    }
}
