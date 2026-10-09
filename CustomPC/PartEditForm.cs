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
        Shown += PartEditForm_Shown;
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
        // Older records may exceed today's input limit. Still allow opening them to correct it.
        stock.Maximum = Math.Max(stock.Maximum, part.Stock);
        stock.Value = part.Stock;
        specifications.Text = part.Specifications;
        socket.Text = part.Socket;
        memory.Text = part.MemoryType;
        watts.Value = part.Watts;
        low.Value = part.LowStock;
        archived.Checked = part.Archived;

        List<Supplier> supplierChoices = new List<Supplier>();
        foreach (Supplier savedSupplier in AppStore.Current.Suppliers)
        {
            if (!savedSupplier.Archived || savedSupplier.Id == part.SupplierId)
            {
                supplierChoices.Add(savedSupplier);
            }
        }

        supplier.DataSource = supplierChoices;
        supplier.SelectedIndex = -1;
        foreach (Supplier savedSupplier in supplierChoices)
        {
            if (savedSupplier.Id == part.SupplierId)
            {
                supplier.SelectedItem = savedSupplier;
                if (savedSupplier.Archived)
                {
                    supplierLabel.Text = "Supplier (current supplier is archived)";
                }
                break;
            }
        }
    }

    private void PartEditForm_Shown(object? sender, EventArgs e)
    {
        // Small laptop displays can be shorter than this editor. Fit the window, then scroll.
        Rectangle workingArea = Screen.FromControl(this).WorkingArea;
        MinimumSize = new Size(Math.Min(MinimumSize.Width, workingArea.Width),
            Math.Min(MinimumSize.Height, workingArea.Height));
        if (Height > workingArea.Height)
        {
            Height = workingArea.Height;
            Top = workingArea.Top;
        }
        if (Width > workingArea.Width)
        {
            Width = workingArea.Width;
            Left = workingArea.Left;
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
