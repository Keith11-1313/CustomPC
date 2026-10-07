namespace CustomPC;

public partial class PartEditForm : Form
{
    private string? partId;
    public PartEditForm()
    {
        InitializeComponent(); if (Ui.IsDesign) return;
        category.Items.AddRange(AppStore.Categories); category.SelectedIndex = 0;
        supplier.DataSource = AppStore.Current.Suppliers.Where(x => !x.Archived).ToList(); supplier.SelectedIndex = -1;
        low.Value = 3; save.Click += (_, _) => Ui.Run(this, () => {
            var p = new Part { Name = partName.Text.Trim(), Category = category.Text, Brand = brand.Text.Trim(), Price = price.Value, Stock = (int)stock.Value,
                Specifications = specifications.Text.Trim(), Socket = socket.Text.Trim(), MemoryType = memory.Text.Trim(), Watts = (int)watts.Value, LowStock = (int)low.Value,
                SupplierId = (supplier.SelectedItem as Supplier)?.Id ?? "", Archived = archived.Checked };
            if (partId != null) p.Id = partId; AppStore.Current.SavePart(p); DialogResult = DialogResult.OK;
        });
    }
    public PartEditForm(string id) : this()
    {
        var p = AppStore.Current.Parts.Single(x => x.Id == id); partId = id;
        partName.Text = p.Name; category.SelectedItem = p.Category; brand.Text = p.Brand; price.Value = p.Price; stock.Value = p.Stock;
        specifications.Text = p.Specifications; socket.Text = p.Socket; memory.Text = p.MemoryType; watts.Value = p.Watts; low.Value = p.LowStock; archived.Checked = p.Archived;
        supplier.SelectedItem = supplier.Items.Cast<Supplier>().FirstOrDefault(x => x.Id == p.SupplierId);
    }
}
