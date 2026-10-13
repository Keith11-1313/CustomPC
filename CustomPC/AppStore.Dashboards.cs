namespace CustomPC;

public sealed record DailyValue(DateTime Date, double Value);
public sealed record NamedValue(string Name, double Value, string? Id = null);
public sealed record CategoryStock(string Category, int Available, int LowStock, int OutOfStock);
public sealed record AdminDashboardData(List<DailyValue> Sales, List<NamedValue> TopParts,
    List<NamedValue> Inventory, DateTime FirstDate, DateTime LastDate);
public sealed record StaffDashboardData(List<DailyValue> Incoming, List<NamedValue> Queue,
    List<CategoryStock> Stock, DateTime FirstDate, DateTime LastDate);

public sealed partial class AppStore
{
    // These methods refresh permissions first; hiding a chart is not an access check.
    // No cached result survives an account switch. Dates are local calendar dates, inclusive.
    public AdminDashboardData ReadAdminDashboard(int days, DateTime? localToday = null)
    {
        Refresh(); RequireAdmin();
        DateTime last = (localToday ?? DateTime.Today).Date;
        DateTime first = PeriodStart(days, last);
        var payments = Payments.Where(p => p.PaidUtc.ToLocalTime().Date >= first
            && p.PaidUtc.ToLocalTime().Date <= last).ToList();
        var sales = Enumerable.Range(0, days).Select(offset =>
        {
            DateTime date = first.AddDays(offset);
            return new DailyValue(date, (double)payments.Where(p => p.PaidUtc.ToLocalTime().Date == date).Sum(p => p.Amount));
        }).ToList();
        var paidIds = payments.Select(p => p.OrderId).ToHashSet();
        var top = Orders.Where(o => paidIds.Contains(o.Id)).SelectMany(o => o.Lines)
            .GroupBy(line => line.PartId).Select(group => new NamedValue(group.First().Name, group.Sum(line => line.Quantity), group.Key))
            .OrderByDescending(item => item.Value).ThenBy(item => item.Name).Take(5).ToList();
        var inventory = new[] { "Available", "Low stock", "Out of stock" }
            .Select(state => new NamedValue(state, Parts.Count(p => !p.Archived && StockState(p) == state))).ToList();
        return new(sales, top, inventory, first, last);
    }

    public StaffDashboardData ReadStaffDashboard(int days, DateTime? localToday = null)
    {
        Refresh(); RequireStaff();
        DateTime last = (localToday ?? DateTime.Today).Date;
        DateTime first = PeriodStart(days, last);
        var incoming = Enumerable.Range(0, days).Select(offset =>
        {
            DateTime date = first.AddDays(offset);
            return new DailyValue(date, Orders.Count(o => o.CreatedUtc.ToLocalTime().Date == date));
        }).ToList();
        var queue = Statuses.Take(5).Select(status => new NamedValue(status, Orders.Count(o => o.Status == status))).ToList();
        var stock = Categories.Select(category =>
        {
            var parts = Parts.Where(p => !p.Archived && p.Category == category).ToList();
            return new CategoryStock(category, parts.Count(p => StockState(p) == "Available"),
                parts.Count(p => StockState(p) == "Low stock"), parts.Count(p => StockState(p) == "Out of stock"));
        }).ToList();
        return new(incoming, queue, stock, first, last);
    }

    private static DateTime PeriodStart(int days, DateTime last)
    {
        if (days != 7 && days != 30 && days != 90) throw new ArgumentException("Choose 7, 30 or 90 days.");
        return last.AddDays(1 - days);
    }
}
