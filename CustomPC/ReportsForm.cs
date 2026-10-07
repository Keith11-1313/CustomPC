namespace CustomPC;

public partial class ReportsForm : Form
{
    public ReportsForm()
    {
        InitializeComponent(); if (Ui.IsDesign) return; var s = AppStore.Current;
        if (!s.IsStaff) throw new InvalidOperationException("Staff access required.");
        report.Items.AddRange(new object[] { "Inventory", "Orders", "Stock movements" }); if (s.IsAdmin) report.Items.Add("Sales");
        from.Value = DateTime.Today.AddDays(-30); to.Value = DateTime.Today; report.SelectedIndex = 0;
        report.SelectedIndexChanged += (_, _) => Ui.Run(this, LoadReport); refresh.Click += (_, _) => Ui.Run(this, LoadReport);
        export.Click += (_, _) => Ui.Run(this, () => Ui.Export(this, grid)); LoadReport();
    }
    public ReportsForm(string kind) : this() { report.SelectedItem = kind; LoadReport(); }
    private void LoadReport()
    {
        var s = AppStore.Current; s.Refresh(); if (from.Value.Date > to.Value.Date) throw new InvalidOperationException("Start date must not be after end date.");
        bool InRange(DateTime value) => value.ToLocalTime().Date >= from.Value.Date && value.ToLocalTime().Date <= to.Value.Date;
        from.Enabled = to.Enabled = report.Text != "Inventory";
        if (report.Text == "Inventory")
        {
            var list = s.Parts.Where(x => !x.Archived).ToList();
            Ui.Bind(grid, list.Select(p => new { p.Id, Part = p.Name, p.Category, OnHand = p.Stock, Reserved = s.Reserved(p.Id), Available = s.Available(p), Status = s.StockState(p), Price = Ui.Money(p.Price) }).ToList());
            summary.Text = $"Current inventory: {list.Count} parts / {list.Sum(s.Available)} available units / {list.Sum(p => s.Reserved(p.Id))} reserved units";
        }
        else if (report.Text == "Orders")
        {
            var list = s.Orders.Where(o => InRange(o.CreatedUtc)).ToList();
            Ui.Bind(grid, list.Select(o => new { o.Id, Order = o.Number, Customer = o.CustomerName, o.Status, Total = Ui.Money(o.Total), Created = o.CreatedUtc.ToLocalTime().ToString("g") }).ToList());
            summary.Text = $"{list.Count} orders / {list.Count(x => x.Status == "Cancelled")} cancelled / order value {Ui.Money(list.Sum(x => x.Total))}";
        }
        else if (report.Text == "Sales")
        {
            if (!s.IsAdmin) throw new InvalidOperationException("Sales reports require admin access.");
            var list = s.Payments.Where(p => InRange(p.PaidUtc)).ToList();
            Ui.Bind(grid, list.Select(p => new { p.Id, Order = s.Orders.Single(x => x.Id == p.OrderId).Number, Paid = p.PaidUtc.ToLocalTime().ToString("g"), Amount = Ui.Money(p.Amount), ConfirmedBy = s.Users.FirstOrDefault(x => x.Id == p.StaffId)?.Name }).ToList());
            summary.Text = $"{list.Count} confirmed store payments / sales {Ui.Money(list.Sum(x => x.Amount))}";
        }
        else
        {
            var list = s.Movements.Where(x => InRange(x.CreatedUtc)).OrderByDescending(x => x.CreatedUtc).ToList();
            Ui.Bind(grid, list.Select(x => new { x.Id, Part = s.Parts.FirstOrDefault(p => p.Id == x.PartId)?.Name, x.Quantity, x.Reason, RecordedBy = s.Users.FirstOrDefault(u => u.Id == x.UserId)?.Name, Date = x.CreatedUtc.ToLocalTime().ToString("g") }).ToList());
            summary.Text = $"{list.Count} inventory adjustments / net quantity {list.Sum(x => x.Quantity)}";
        }
    }
}
