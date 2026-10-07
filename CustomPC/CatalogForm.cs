namespace CustomPC;

public partial class CatalogForm : Form
{
    public CatalogForm()
    {
        InitializeComponent(); if (Ui.IsDesign) return;
        category.Items.Add("All categories"); category.Items.AddRange(AppStore.Categories); category.SelectedIndex = 0;
        availability.Items.AddRange(new object[] { "All active parts", "Available", "Low stock", "Out of stock" }); availability.SelectedIndex = 0;
        search.TextChanged += (_, _) => LoadRows(); category.SelectedIndexChanged += (_, _) => LoadRows(); availability.SelectedIndexChanged += (_, _) => LoadRows();
        refresh.Click += (_, _) => Ui.Run(this, () => { AppStore.Current.Refresh(); LoadRows(); }); LoadRows();
        grid.CellDoubleClick += (_, e) => Ui.Run(this, () => {
            if (e.RowIndex < 0) return;
            var id = grid.Rows[e.RowIndex].Cells["Id"].Value?.ToString();
            var p = AppStore.Current.Parts.Single(x => x.Id == id);
            MessageBox.Show(this, $"{p.Name}\n{p.Brand} / {p.Category}\n{Ui.Money(p.Price)}\n\n{p.Specifications}\nSocket: {p.Socket}\nMemory type: {p.MemoryType}\nWattage: {p.Watts}\nAvailable: {AppStore.Current.Available(p)}", "Component details");
        });
    }
    private void LoadRows()
    {
        var s = AppStore.Current; var term = search.Text.Trim();
        Ui.Bind(grid, s.Parts.Where(p => !p.Archived && (category.SelectedIndex == 0 || p.Category == category.Text) &&
            (availability.SelectedIndex == 0 || s.StockState(p) == availability.Text || availability.Text == "Available" && s.Available(p) > 0) &&
            $"{p.Name} {p.Brand} {p.Specifications}".Contains(term, StringComparison.OrdinalIgnoreCase))
            .Select(p => new { p.Id, Component = p.Name, p.Category, p.Brand, Price = Ui.Money(p.Price), p.Specifications, Available = s.Available(p), Status = s.StockState(p) }).ToList());
    }
}
