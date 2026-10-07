namespace CustomPC;

public class UserAccount
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = "Customer";
    public bool Archived { get; set; }
}
public class Part
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
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
    public override string ToString() => $"{Name}  |  PHP {Price:N2}";
}
public class Supplier
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public string Contact { get; set; } = "";
    public string Email { get; set; } = "";
    public string Address { get; set; } = "";
    public bool Archived { get; set; }
    public override string ToString() => Name;
}
public class CompatibilityRule
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public string Kind { get; set; } = "Socket";
    public bool Enabled { get; set; } = true;
}
public class OrderLine
{
    public string PartId { get; set; } = "";
    public string Name { get; set; } = "";
    public int Quantity { get; set; } = 1;
    public decimal Price { get; set; }
}
public class PcBuild
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string CustomerId { get; set; } = "";
    public List<string> PartIds { get; set; } = [];
}
public class CustomerOrder
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Number { get; set; } = "";
    public string CustomerId { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string BuildId { get; set; } = "";
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime DeadlineUtc { get; set; } = DateTime.UtcNow.AddHours(24);
    public string Status { get; set; } = "Pending Payment";
    public List<OrderLine> Lines { get; set; } = [];
    public decimal Total => Lines.Sum(x => x.Price * x.Quantity);
}
public class Payment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string OrderId { get; set; } = "";
    public string StaffId { get; set; } = "";
    public decimal Amount { get; set; }
    public DateTime PaidUtc { get; set; } = DateTime.UtcNow;
}
public class InventoryMovement
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string PartId { get; set; } = "";
    public int Quantity { get; set; }
    public string Reason { get; set; } = "";
    public string UserId { get; set; } = "";
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
