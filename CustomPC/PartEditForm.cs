namespace CustomPC;

public partial class PartEditForm : StoreWindow
{
    private string? partId;
    private string photoFileName = "";
    private string? photoImportPath;
    private bool photoRemoved;
    private EditorSheet? editorSheet;

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

        specifications.Multiline = true;
        specifications.ScrollBars = ScrollBars.Vertical;
        editorSheet = new EditorSheet { Name = "editorSheet" };
        editorSheet.Bind(this, heading, back, save, "Product identity, pricing and category-specific specifications. Stock is managed separately.", true,
            (partNameLabel, partName), (categoryLabel, category), (brandLabel, brand), (priceLabel, price),
            (lowLabel, low), (specificationsLabel, specifications), (socketLabel, socket), (memoryLabel, memory),
            (wattsLabel, watts), (recommendedPsuLabel, recommendedPsu));
        editorSheet.AddOption(integratedGraphics); editorSheet.AddOption(videoOutput); editorSheet.AddOption(archived);
        editorSheet.AddPhoto(photoPreview, choosePhoto, removePhoto, photoStatus);
        category.Items.AddRange(AppStore.Categories);
        category.SelectedIndex = 0;
        category.SelectedIndexChanged += CategoryChanged;
        UpdateCategoryFields();

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

    }

    private void BackButton_Click(object? sender, EventArgs e)
    {
        // Return to the previous screen without saving changes.
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private void CategoryChanged(object? sender, EventArgs e)
    {
        UpdateCategoryFields();
    }

    // Hidden fields keep their draft values, but unrelated values are not saved.
    private void UpdateCategoryFields()
    {
        if (editorSheet == null) return;
        string selected = category.Text;
        editorSheet.SetFieldVisible(socket, selected is "CPU" or "Motherboard");
        editorSheet.SetFieldVisible(memory, selected is "Motherboard" or "RAM");
        editorSheet.SetFieldVisible(watts, selected != "Case");
        editorSheet.SetFieldVisible(recommendedPsu, selected == "GPU");
        editorSheet.SetFieldVisible(integratedGraphics, selected == "CPU");
        editorSheet.SetFieldVisible(videoOutput, selected == "Motherboard");
        wattsLabel.Text = selected == "Power Supply" ? "PSU capacity (W)" : "Power use (W)";
    }
    private void PartEditForm_Shown(object? sender, EventArgs e)
    {
        UpdateCategoryFields();
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
            part.Socket = category.Text is "CPU" or "Motherboard" ? socket.Text.Trim() : "";
            part.MemoryType = category.Text is "Motherboard" or "RAM" ? memory.Text.Trim() : "";
            part.HasIntegratedGraphics = category.Text == "CPU" && integratedGraphics.Checked;
            part.HasVideoOutput = category.Text == "Motherboard" && videoOutput.Checked;
            part.Watts = category.Text == "Case" ? 0 : (int)watts.Value;
            part.RecommendedPsuWatts = category.Text == "GPU" ? (int)recommendedPsu.Value : 0;
            part.LowStock = (int)low.Value;
            part.Archived = archived.Checked;

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
