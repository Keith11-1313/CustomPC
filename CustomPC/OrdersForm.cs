namespace CustomPC;

public partial class OrdersForm : Form
{
    private readonly System.Windows.Forms.Timer timer = new() { Interval = 30000 };
    public OrdersForm()
    {
        InitializeComponent(); if (Ui.IsDesign) return;
        var s = AppStore.Current; pay.Enabled = advance.Enabled = s.IsStaff;
        status.Items.Add("All statuses"); status.Items.AddRange(AppStore.Statuses); status.SelectedIndex = 0;
        search.TextChanged += (_, _) => LoadRows(); status.SelectedIndexChanged += (_, _) => LoadRows();
        refresh.Click += (_, _) => Ui.Run(this, LoadRows);
        receipt.Click += (_, _) => Ui.Run(this, () => { using var f = new ReceiptForm(Ui.RequireSelection(grid)); f.ShowDialog(this); });
        pay.Click += (_, _) => Ui.Run(this, () => { var id = Ui.RequireSelection(grid); if (Ui.Confirm(this, "Confirm the customer paid the full amount at the physical store?")) { s.ConfirmPayment(id); LoadRows(); } });
        advance.Click += (_, _) => Ui.Run(this, () => { var id = Ui.RequireSelection(grid); if (Ui.Confirm(this, "Move this order to the next stage? Completion records pickup and deducts stock.")) { s.AdvanceOrder(id); LoadRows(); } });
        cancel.Click += (_, _) => Ui.Run(this, () => { var id = Ui.RequireSelection(grid); if (Ui.Confirm(this, "Cancel this unpaid order and release its reserved parts?")) { s.CancelOrder(id); LoadRows(); } });
        timer.Tick += (_, _) => Ui.Run(this, LoadRows); timer.Start(); FormClosed += (_, _) => timer.Dispose(); LoadRows();
    }
    private void LoadRows()
    {
        var s = AppStore.Current; s.Refresh();
        Ui.Bind(grid, s.VisibleOrders().Where(o => (status.SelectedIndex == 0 || o.Status == status.Text) && $"{o.Number} {o.CustomerName}".Contains(search.Text.Trim(), StringComparison.OrdinalIgnoreCase))
            .Select(o => new { o.Id, Order = o.Number, Customer = o.CustomerName, Total = Ui.Money(o.Total), o.Status, Payment = s.Payments.Any(p => p.OrderId == o.Id) ? "Paid" : "Unpaid", PayBefore = o.DeadlineUtc.ToLocalTime().ToString("MMM dd, hh:mm tt") }).ToList());
    }
}
