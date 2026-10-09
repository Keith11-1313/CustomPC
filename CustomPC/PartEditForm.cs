namespace CustomPC;

public partial class PartEditForm : Form
{
    private string? partId;

    public PartEditForm()
    {
        InitializeComponent();
        if (Ui.IsDesign)
        {
            return;
        }

        category.Items.AddRange(AppStore.Categories);
        category.SelectedIndex = 0;

        List<Supplier> activeSuppliers = new List<Supplier>();
        foreach (Supplier savedSupplier in AppStore.Current.Suppliers)
        {
            if (!savedSupplier.Archived)
            {
                activeSuppliers.Add(savedSupplier);
            }
        }
        supplier.DataSource = activeSuppliers;
        supplier.SelectedIndex = -1;

        low.Value = 3;
        save.Click += SaveClicked;
    }

    // The same form can add a new part or edit an existing part.
    public PartEditForm(string id) : this()
    {
        partId = id;
        Part? part = null;
        foreach (Part savedPart in AppStore.Current.Parts)
        {
            if (savedPart.Id == id)
            {
                part = savedPart;
                break;
            }
        }

        if (part == null)
        {
            throw new InvalidOperationException("The selected part no longer exists.");
        }

        partName.Text = part.Name;
        category.SelectedItem = part.Category;
        brand.Text = part.Brand;
        price.Value = part.Price;
        stock.Value = part.Stock;
        specifications.Text = part.Specifications;
        socket.Text = part.Socket;
        memory.Text = part.MemoryType;
        watts.Value = part.Watts;
        low.Value = part.LowStock;
        archived.Checked = part.Archived;

        foreach (Supplier savedSupplier in supplier.Items)
        {
            if (savedSupplier.Id == part.SupplierId)
            {
                supplier.SelectedItem = savedSupplier;
                break;
            }
        }
    }

    private void SaveClicked(object? sender, EventArgs e)
    {
        try
        {
            Part part = new Part();
            part.Name = partName.Text.Trim();
            part.Category = category.Text;
            part.Brand = brand.Text.Trim();
            part.Price = price.Value;
            part.Stock = (int)stock.Value;
            part.Specifications = specifications.Text.Trim();
            part.Socket = socket.Text.Trim();
            part.MemoryType = memory.Text.Trim();
            part.Watts = (int)watts.Value;
            part.LowStock = (int)low.Value;
            part.Archived = archived.Checked;

            if (supplier.SelectedItem is Supplier selectedSupplier)
            {
                part.SupplierId = selectedSupplier.Id;
            }

            if (partId != null)
            {
                part.Id = partId;
            }

            AppStore.Current.SavePart(part);
            DialogResult = DialogResult.OK;
        }
        catch (Exception exception)
        {
            Ui.ShowError(this, exception);
        }
    }
}
