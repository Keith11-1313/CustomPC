using Microsoft.Data.Sqlite;
using System.Text.Json;

namespace CustomPC;

// SQLite commands live here. AppStore handles validation and business rules.
internal class Database
{
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

        SqliteConnection connection = new SqliteConnection(settings.ToString());
        connection.Open();
        return connection;
    }

    public void CreateTables()
    {
        using (SqliteConnection connection = OpenConnection())
        {
            foreach (string tableName in tableNames)
            {
                using (SqliteCommand command = connection.CreateCommand())
                {
                    // Existing records use Id and JSON Data, so keep the same table format.
                    command.CommandText = $"CREATE TABLE IF NOT EXISTS {tableName} (Id TEXT PRIMARY KEY, Data TEXT NOT NULL)";
                    command.ExecuteNonQuery();
                }
            }
        }
    }

    // T is the record type: Part, UserAccount, CustomerOrder, etc.
    public List<T> ReadRecords<T>(SqliteConnection connection, string tableName) where T : DatabaseRecord
    {
        List<T> records = new List<T>();

        using (SqliteCommand command = connection.CreateCommand())
        {
            command.CommandText = $"SELECT Data FROM {tableName}";

            using (SqliteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string savedJson = reader.GetString(0);
                    T? record = JsonSerializer.Deserialize<T>(savedJson);

                    if (record == null)
                    {
                        throw new InvalidOperationException($"A record in {tableName} could not be read.");
                    }

                    records.Add(record);
                }
            }
        }

        return records;
    }

    public void WriteRecords<T>(SqliteConnection connection, SqliteTransaction transaction,
        string tableName, List<T> records) where T : DatabaseRecord
    {
        using (SqliteCommand clearTable = connection.CreateCommand())
        {
            clearTable.Transaction = transaction;
            clearTable.CommandText = $"DELETE FROM {tableName}";
            clearTable.ExecuteNonQuery();
        }

        foreach (T record in records)
        {
            using (SqliteCommand saveRecord = connection.CreateCommand())
            {
                saveRecord.Transaction = transaction;
                saveRecord.CommandText = $"INSERT INTO {tableName} (Id, Data) VALUES ($id, $data)";

                // Parameters keep record values separate from SQL instructions.
                saveRecord.Parameters.AddWithValue("$id", record.Id);
                saveRecord.Parameters.AddWithValue("$data", JsonSerializer.Serialize(record));
                saveRecord.ExecuteNonQuery();
            }
        }
    }
}
