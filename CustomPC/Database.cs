using Microsoft.Data.Sqlite;
using System.Text.Json;

namespace CustomPC;

// SQLite commands live here. AppStore handles validation and business rules.
internal class Database
{
    public List<(string Table, string Id)> ChangedRecords { get; } = new List<(string, string)>();
    private readonly string databasePath;
    private readonly string[] tableNames =
    {
        "Users", "Parts", "Suppliers", "Rules", "Builds", "Orders", "Payments", "Movements"
    };

    public Database(string path)
    {
        databasePath = path;
        string folder = Path.GetDirectoryName(databasePath)!;
        Directory.CreateDirectory(folder);
    }

    public SqliteConnection OpenConnection()
    {
        SqliteConnectionStringBuilder settings = new SqliteConnectionStringBuilder();
        settings.DataSource = databasePath;
        settings.DefaultTimeout = 10;

        SqliteConnection connection = new SqliteConnection(settings.ToString());
        connection.Open();
        return connection;
    }

    public void CreateTables()
    {
        using (SqliteConnection connection = OpenConnection())
        {
            using SqliteCommand version = connection.CreateCommand();
            version.CommandText = "PRAGMA user_version";
            if (Convert.ToInt32(version.ExecuteScalar()) > 1)
                throw new InvalidOperationException("This database was created by a newer CustomPC version. Install that version before opening it.");
            using SqliteTransaction transaction = connection.BeginTransaction();
            foreach (string tableName in tableNames)
            {
                using (SqliteCommand command = connection.CreateCommand())
                {
                    // Existing records use Id and JSON Data, so keep the same table format.
                    command.CommandText = $"CREATE TABLE IF NOT EXISTS {tableName} (Id TEXT PRIMARY KEY, Data TEXT NOT NULL)";
                    command.Transaction = transaction;
                    command.ExecuteNonQuery();
                }
            }
            using SqliteCommand support = connection.CreateCommand();
            support.Transaction = transaction;
            support.CommandText = """
                CREATE TABLE IF NOT EXISTS Settings (Key TEXT PRIMARY KEY, Value TEXT NOT NULL);
                CREATE TABLE IF NOT EXISTS AuditLog (Id INTEGER PRIMARY KEY AUTOINCREMENT, CreatedUtc TEXT NOT NULL,
                    ActorId TEXT NOT NULL, Action TEXT NOT NULL, Details TEXT NOT NULL);
                CREATE TABLE IF NOT EXISTS LoginAttempts (Key TEXT PRIMARY KEY, Failures INTEGER NOT NULL, LockedUntilUtc TEXT NOT NULL);
                PRAGMA user_version = 1;
                """;
            support.ExecuteNonQuery();
            transaction.Commit();
        }
    }

    // T is the record type: Part, UserAccount, CustomerOrder, etc.
    public List<T> ReadRecords<T>(SqliteConnection connection, string tableName) where T : DatabaseRecord
    {
        List<T> records = new List<T>();

        using (SqliteCommand command = connection.CreateCommand())
        {
            command.CommandText = $"SELECT Id, Data FROM {tableName} ORDER BY rowid";

            using (SqliteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string savedJson = reader.GetString(1);
                    // Convert one SQLite Data cell back into a normal C# model object.
                    T? record = JsonSerializer.Deserialize<T>(savedJson);

                    if (record == null || record.Id != reader.GetString(0))
                    {
                        throw new InvalidOperationException($"A record in {tableName} could not be read.");
                    }

                    records.Add(record);
                }
            }
        }

        return records;
    }

    public int WriteRecords<T>(SqliteConnection connection, SqliteTransaction transaction,
        string tableName, List<T> records) where T : DatabaseRecord
    {
        // Compare saved JSON with the current records. Unchanged rows are left alone.
        Dictionary<string, string> saved = new Dictionary<string, string>();
        using (SqliteCommand read = connection.CreateCommand())
        {
            read.Transaction = transaction;
            read.CommandText = $"SELECT Id, Data FROM {tableName}";
            using SqliteDataReader reader = read.ExecuteReader();
            while (reader.Read()) saved.Add(reader.GetString(0), reader.GetString(1));
        }
        int changed = 0;
        HashSet<string> currentIds = new HashSet<string>();
        foreach (T record in records)
        {
            if (!currentIds.Add(record.Id)) throw new InvalidOperationException("Duplicate record identifier.");
            string json = JsonSerializer.Serialize(record);
            if (saved.TryGetValue(record.Id, out string? previous) && previous == json) continue;
            using (SqliteCommand saveRecord = connection.CreateCommand())
            {
                saveRecord.Transaction = transaction;
                saveRecord.CommandText = $"INSERT INTO {tableName} (Id, Data) VALUES ($id, $data) ON CONFLICT(Id) DO UPDATE SET Data = excluded.Data";

                // Parameters keep record values separate from SQL instructions.
                saveRecord.Parameters.AddWithValue("$id", record.Id);
                // Convert the model's properties into JSON for the Data column.
                saveRecord.Parameters.AddWithValue("$data", json);
                saveRecord.ExecuteNonQuery();
                changed++;
                ChangedRecords.Add((tableName, record.Id));
            }
        }
        foreach (string id in saved.Keys)
        {
            if (currentIds.Contains(id)) continue;
            using SqliteCommand remove = connection.CreateCommand();
            remove.Transaction = transaction;
            remove.CommandText = $"DELETE FROM {tableName} WHERE Id = $id";
            remove.Parameters.AddWithValue("$id", id);
            remove.ExecuteNonQuery();
            changed++;
            ChangedRecords.Add((tableName, id));
        }
        return changed;
    }
}
