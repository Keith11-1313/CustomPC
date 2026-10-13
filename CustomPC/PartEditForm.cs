namespace CustomPC;

public partial class PartEditForm : StoreWindow
{
    private string? partId;
    private string photoFileName = "";
    private string? photoImportPath;
    private bool photoRemoved;

    public PartEditForm()
    {
        InitializeComponent();
        back.Click += BackButton_Click;
        if (Ui.IsDesign)
        {
            return;
        }

        AppStore.Current.Refresh();
        if (!AppStore.Current.IsAdmin)
            throw new InvalidOperationException("Administrator access is required.");

        category.Items.AddRange(AppStore.Categories);
        category.SelectedIndex = 0;
        category.SelectedIndexChanged += CategoryChanged;
        UpdateGraphicsFields();

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
        choosePhoto.Click += ChoosePhoto_Click;
        removePhoto.Click += (_, _) =>
        {
            photoImportPath = null; photoRemoved = true;
            photoPreview.Image?.Dispose(); photoPreview.Image = null;
            photoStatus.Text = "Photo will be removed when you save.";
        };
        FormClosed += (_, _) => photoPreview.Image?.Dispose();
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
        photoFileName = part.PhotoFileName;
        photoPreview.Image = ProductPhotos.Read(AppStore.Current.DatabasePath, photoFileName);
        photoStatus.Text = photoPreview.Image == null ? "Photo unavailable" : "Saved product photograph";
        category.SelectedItem = part.Category;
        brand.Text = part.Brand;
        price.Value = part.Price;
        // Older records may exceed today's input limit. Still allow opening them to correct it.
        stock.Maximum = Math.Max(stock.Maximum, part.Stock);
        stock.Value = part.Stock;
        specifications.Text = part.Specifications;
        socket.Text = part.Socket;
        memory.Text = part.MemoryType;
        integratedGraphics.Checked = part.HasIntegratedGraphics;
        videoOutput.Checked = part.HasVideoOutput;
        watts.Maximum = Math.Max(watts.Maximum, part.Watts);
        watts.Value = part.Watts;
        recommendedPsu.Maximum = Math.Max(recommendedPsu.Maximum, part.RecommendedPsuWatts);
        recommendedPsu.Value = part.RecommendedPsuWatts;
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

    private void BackButton_Click(object? sender, EventArgs e)
    {
        // Return to the previous screen without saving changes.
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private void CategoryChanged(object? sender, EventArgs e)
    {
        UpdateGraphicsFields();
    }

    private void UpdateGraphicsFields()
    {
        integratedGraphics.Enabled = category.Text == "CPU";
        videoOutput.Enabled = category.Text == "Motherboard";
        if (!integratedGraphics.Enabled) integratedGraphics.Checked = false;
        if (!videoOutput.Enabled) videoOutput.Checked = false;
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
            AppStore store = AppStore.Current;
            store.Refresh();
            Part part = new Part();
            part.Name = partName.Text.Trim();
            part.PhotoFileName = photoFileName;
            part.CatalogCode = partId == null ? "" : store.Parts.First(p => p.Id == partId).CatalogCode;
            part.Category = category.Text;
            part.Brand = brand.Text.Trim();
            part.Price = price.Value;
            part.Stock = partId == null ? 0 : store.Parts.First(p => p.Id == partId).Stock;
            part.Specifications = specifications.Text.Trim();
            part.Socket = socket.Text.Trim();
            part.MemoryType = memory.Text.Trim();
            part.HasIntegratedGraphics = category.Text == "CPU" && integratedGraphics.Checked;
            part.HasVideoOutput = category.Text == "Motherboard" && videoOutput.Checked;
            part.Watts = (int)watts.Value;
            part.RecommendedPsuWatts = (int)recommendedPsu.Value;
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

            AppStore.Current.SavePartWithPhoto(part, photoImportPath, photoRemoved);
            DialogResult = DialogResult.OK;
        }
        catch (Exception exception)
        {
            Ui.ShowError(this, exception);
        }
    }

    private void ChoosePhoto_Click(object? sender, EventArgs e)
    {
        using OpenFileDialog picker = new() { Filter = "Product photograph (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png", CheckFileExists = true };
        if (picker.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            using Image original = Image.FromFile(picker.FileName);
            Image preview = new Bitmap(original);
            photoPreview.Image?.Dispose(); photoPreview.Image = preview;
            photoImportPath = picker.FileName; photoRemoved = false;
            photoStatus.Text = "New photograph will be copied when you save.";
        }
        catch (Exception error) { Ui.ShowError(this, new InvalidOperationException("This photograph could not be opened.", error)); }
    }
}
