using Microsoft.Data.Sqlite;

namespace CustomPC;

public sealed partial class AppStore
{
    // This runs only for a new database with no user accounts.
    private void CreateSampleData()
    {
        using (SqliteConnection connection = database.OpenConnection())
        using (SqliteTransaction transaction = connection.BeginTransaction())
        {
            try
            {
                LoadData(connection);

                foreach (string role in Roles)
                {
                    UserAccount account = new UserAccount();
                    account.Name = role + " Demo";
                    if (role == "Customer")
                    {
                        account.Name = "Demo Customer";
                    }

                    account.Email = role.ToLowerInvariant() + "@custompc.local";
                    account.Role = role;
                    account.PasswordHash = HashPassword("CustomPC123!");
                    Users.Add(account);
                }

                Supplier supplier = new Supplier();
                supplier.Name = "Demo Parts Distributor";
                supplier.Contact = "0917 000 0000";
                supplier.Email = "supplier@example.com";
                supplier.Address = "Caloocan City";
                Suppliers.Add(supplier);

                // These are demonstration prices and specifications.
                AddSamplePart(supplier.Id, "Ryzen 5 5600", "CPU", "AMD", 6500, "6 cores / 12 threads", "AM4", "", 65);
                AddSamplePart(supplier.Id, "Core i5-12400F", "CPU", "Intel", 8000, "6 cores / 12 threads", "LGA1700", "", 65);
                AddSamplePart(supplier.Id, "B550M motherboard", "Motherboard", "MSI", 5500, "Micro ATX / AM4 / DDR4", "AM4", "DDR4", 40);
                AddSamplePart(supplier.Id, "B660M motherboard", "Motherboard", "ASUS", 6500, "Micro ATX / LGA1700 / DDR4", "LGA1700", "DDR4", 40);
                AddSamplePart(supplier.Id, "16 GB DDR4 kit", "RAM", "Kingston", 2500, "2 x 8 GB / 3200 MHz", "", "DDR4", 10);
                AddSamplePart(supplier.Id, "16 GB DDR5 kit", "RAM", "Kingston", 3500, "2 x 8 GB / 5200 MHz", "", "DDR5", 10);
                AddSamplePart(supplier.Id, "GeForce RTX 3060", "GPU", "NVIDIA", 14000, "12 GB GDDR6", "", "", 170);
                AddSamplePart(supplier.Id, "Radeon RX 6600", "GPU", "AMD", 12000, "8 GB GDDR6", "", "", 132);
                AddSamplePart(supplier.Id, "500 GB NVMe SSD", "Storage", "WD", 2200, "M.2 / PCIe NVMe", "", "", 8);
                AddSamplePart(supplier.Id, "1 TB NVMe SSD", "Storage", "Samsung", 4000, "M.2 / PCIe NVMe", "", "", 8);
                AddSamplePart(supplier.Id, "650 W Bronze PSU", "Power Supply", "Corsair", 3500, "650 W / 80 Plus Bronze", "", "", 650);
                AddSamplePart(supplier.Id, "Micro ATX tower", "Case", "Tecware", 2000, "Micro ATX / mesh front", "", "", 0);

                AddSampleRule("CPU / motherboard socket", "Socket");
                AddSampleRule("RAM / motherboard memory", "Memory");
                AddSampleRule("PSU capacity (+100 W)", "Power");
                SaveData(connection, transaction);
            }
            catch
            {
                UndoFailedChange(connection, transaction);
                throw;
            }
        }
    }

    private void AddSamplePart(string supplierId, string name, string category, string brand,
        decimal price, string specifications, string socket, string memoryType, int watts)
    {
        Part part = new Part();
        part.SupplierId = supplierId;
        part.Name = name;
        part.Category = category;
        part.Brand = brand;
        part.Price = price;
        part.Specifications = specifications;
        part.Socket = socket;
        part.MemoryType = memoryType;
        part.Watts = watts;
        part.Stock = 10;
        Parts.Add(part);
    }

    private void AddSampleRule(string name, string kind)
    {
        CompatibilityRule rule = new CompatibilityRule();
        rule.Name = name;
        rule.Kind = kind;
        rule.Enabled = true;
        Rules.Add(rule);
    }
}
