namespace CustomPC;

public partial class InventoryForm : Form
{
    public InventoryForm()
    {
        InitializeComponent(); if (Ui.IsDesign) return; var s = AppStore.Current;
        if (!s.IsStaff) throw new InvalidOperationException("Staff access required."); add.Enabled = edit.Enabled = s.IsAdmin;
        stockFilter.Items.AddRange(new object[] { "All stock", "Available", "Low stock", "Out of stock" }); stockFilter.SelectedIndex = 0;
        search.TextChanged += (_, _) => LoadRows(); stockFilter.SelectedIndexChanged += (_, _) => LoadRows(); archived.CheckedChanged += (_, _) => LoadRows();
        add.Click += (_, _) => { using var f = new PartEditForm(); f.ShowDialog(this); LoadRows(); };
        edit.Click += (_, _) => Ui.Run(this, () => { using var f = new PartEditForm(Ui.RequireSelection(grid)); f.ShowDialog(this); LoadRows(); });
        adjust.Click += (_, _) => Ui.Run(this, () => { using var f = new StockAdjustmentForm(Ui.RequireSelection(grid)); f.ShowDialog(this); LoadRows(); });
        history.Click += (_, _) => { using var f = new ReportsForm("Stock movements"); f.ShowDialog(this); };
        refresh.Click += (_, _) => Ui.Run(this, LoadRows); LoadRows();
    }
    private void LoadRows()
    {
        var s = AppStore.Current; s.Refresh();
        Ui.Bind(grid, s.Parts.Where(p => (archived.Checked || !p.Archived) && $"{p.Name} {p.Category} {p.Brand}".Contains(search.Text.Trim(), StringComparison.OrdinalIgnoreCase) &&
            (stockFilter.SelectedIndex == 0 || stockFilter.Text == s.StockState(p) || stockFilter.Text == "Available" && s.Available(p) > 0))
            .Select(p => new { p.Id, Component = p.Name, p.Category, Price = Ui.Money(p.Price), OnHand = p.Stock, Reserved = s.Reserved(p.Id), Available = s.Available(p), Status = s.StockState(p), Supplier = s.Suppliers.FirstOrDefault(x => x.Id == p.SupplierId)?.Name ?? "Unassigned", p.Archived }).ToList());
    }
}
