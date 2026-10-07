namespace CustomPC;

public partial class SuppliersForm : Form
{
    public SuppliersForm()
    {
        InitializeComponent(); if (Ui.IsDesign) return;
        if (!AppStore.Current.IsAdmin) throw new InvalidOperationException("Admin access required.");
        search.TextChanged += (_, _) => LoadRows(); archived.CheckedChanged += (_, _) => LoadRows();
        add.Click += (_, _) => { using var f = new SupplierEditForm(); f.ShowDialog(this); LoadRows(); };
        edit.Click += (_, _) => Ui.Run(this, () => { using var f = new SupplierEditForm(Ui.RequireSelection(grid)); f.ShowDialog(this); LoadRows(); });
        parts.Click += (_, _) => Ui.Run(this, () => { var id = Ui.RequireSelection(grid); var lines = AppStore.Current.Parts.Where(x => x.SupplierId == id).Select(x => $"{x.Name} | {x.Category} | stock {x.Stock}"); MessageBox.Show(this, string.Join("\n", lines.DefaultIfEmpty("No components assigned.")), "Supplied components"); }); LoadRows();
    }
    private void LoadRows() { var s = AppStore.Current; s.Refresh(); Ui.Bind(grid, s.Suppliers.Where(x => (archived.Checked || !x.Archived) && $"{x.Name} {x.Contact}".Contains(search.Text.Trim(), StringComparison.OrdinalIgnoreCase)).Select(x => new { x.Id, Supplier = x.Name, x.Contact, x.Email, x.Address, Components = s.Parts.Count(p => p.SupplierId == x.Id), x.Archived }).ToList()); }
}
