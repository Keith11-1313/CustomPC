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
        string selected = category.Text;
        bool hasSocket = selected is "CPU" or "Motherboard";
        bool hasMemory = selected is "Motherboard" or "RAM";
        bool hasPower = selected != "Case";
        bool isGpu = selected == "GPU";

        SuspendLayout();
        AutoScrollPosition = Point.Empty;
        // Use the existing scaled width to respect Windows DPI and font scaling.
        float scale = partName.Width / 480f;
        int Px(int value) => (int)Math.Round(value * scale);
        int left = partName.Left, second = left + Px(244);
        void FieldRow(Label label, Control input, bool show, int x, int y)
        {
            label.Visible = input.Visible = show;
            input.TabStop = show;
            if (!show) return;
            label.Location = new Point(x, y);
            input.Location = new Point(x, y + Px(30));
        }

        stockLabel.Visible = stock.Visible = false;
        stock.TabStop = false; // Stock changes belong to Staff, not this editor.
        FieldRow(lowLabel, low, true, second, priceLabel.Top);
        int row = specifications.Bottom + Px(18);
        FieldRow(socketLabel, socket, hasSocket, left, row);
        FieldRow(memoryLabel, memory, hasMemory, hasSocket ? second : left, row);
        if (hasSocket || hasMemory) row += Px(80);

        wattsLabel.Text = selected == "Power Supply" ? "PSU capacity (W)" : "Power use (W)";
        FieldRow(wattsLabel, watts, hasPower, left, row);
        recommendedPsuLabel.Text = "Recommended PSU (W)";
        recommendedPsuLabel.Height = Px(28);
        FieldRow(recommendedPsuLabel, recommendedPsu, isGpu, second, row);
        if (hasPower) row += Px(80);

        integratedGraphics.Visible = selected == "CPU";
        videoOutput.Visible = selected == "Motherboard";
        integratedGraphics.Enabled = videoOutput.Enabled = true;
        integratedGraphics.TabStop = selected == "CPU";
        videoOutput.TabStop = selected == "Motherboard";
        integratedGraphics.Location = videoOutput.Location = new Point(left, row);
        if (selected is "CPU" or "Motherboard") row += Px(48);

        archived.Location = new Point(left, row);
        save.Location = new Point(second + Px(16), row - Px(4));
        int height = Math.Max(save.Bottom, photoStatus.Bottom) + Px(24);
        AutoScrollMinSize = new Size(Px(830), height);
        int availableHeight = Screen.FromControl(this).WorkingArea.Height - (Height - ClientSize.Height);
        ClientSize = new Size(ClientSize.Width, Math.Min(height, availableHeight));
        ResumeLayout(true);
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
