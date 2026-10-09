namespace CustomPC;

// Models describe saved records. Every record has an Id, inherited from this base class.
public class DatabaseRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
}

public class UserAccount : DatabaseRecord
{
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = "Customer";
    public bool Archived { get; set; }
}

public class Part : DatabaseRecord
{
    public string Name { get; set; } = "";
    public string Category { get; set; } = "CPU";
    public string Brand { get; set; } = "";
    public string Specifications { get; set; } = "";
    public string Socket { get; set; } = "";
    public string MemoryType { get; set; } = "";
    public int Watts { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int LowStock { get; set; } = 3;
    public string SupplierId { get; set; } = "";
    public bool Archived { get; set; }

    // ComboBoxes in the PC builder display this text for each part.
    public override string ToString()
    {
        return Name + "  |  PHP " + Price.ToString("N2");
    }
}

public class Supplier : DatabaseRecord
{
    public string Name { get; set; } = "";
    public string Contact { get; set; } = "";
    public string Email { get; set; } = "";
    public string Address { get; set; } = "";
    public bool Archived { get; set; }

    public override string ToString()
    {
        return Name;
    }
}

public class CompatibilityRule : DatabaseRecord
{
    public string Name { get; set; } = "";
    public string Kind { get; set; } = "Socket";
    public bool Enabled { get; set; } = true;
}

public class OrderLine
{
    // Receipts keep the name and price from order creation, even if the catalog changes.
    public string PartId { get; set; } = "";
    public string Name { get; set; } = "";
    public int Quantity { get; set; } = 1;
    public decimal Price { get; set; }
}

public class PcBuild : DatabaseRecord
{
    public string CustomerId { get; set; } = "";
    public List<string> PartIds { get; set; } = new List<string>();
}

public class CustomerOrder : DatabaseRecord
{
    public string Number { get; set; } = "";
    public string CustomerId { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string BuildId { get; set; } = "";
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime DeadlineUtc { get; set; } = DateTime.UtcNow.AddHours(24);
    public string Status { get; set; } = "Pending Payment";
    public List<OrderLine> Lines { get; set; } = new List<OrderLine>();

    public decimal Total
    {
        get
        {
            decimal total = 0;

            foreach (OrderLine line in Lines)
            {
                total += line.Price * line.Quantity;
            }

            return total;
        }
    }
}

public class Payment : DatabaseRecord
{
    public string OrderId { get; set; } = "";
    public string StaffId { get; set; } = "";
    public decimal Amount { get; set; }
    public DateTime PaidUtc { get; set; } = DateTime.UtcNow;
}

public class InventoryMovement : DatabaseRecord
{
    public string PartId { get; set; } = "";
    public int Quantity { get; set; }
    public string Reason { get; set; } = "";
    public string UserId { get; set; } = "";
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
