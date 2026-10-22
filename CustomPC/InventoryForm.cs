using System.Data;

namespace CustomPC;

public partial class InventoryForm : StoreWindow
{
    private readonly Dictionary<string, (string Key, Image Image)> thumbnails = new();
    private string? filteredCategory;
    public InventoryForm(InventoryViewFilter filter) : this()
    {
        filteredCategory = filter.Category;
        if (filter.StockState != null) stockFilter.SelectedItem = filter.StockState;
        LoadRows();
    }
    public InventoryForm()
    {
        InitializeComponent();
        back.Click += BackButton_Click;
        if (Ui.IsDesign)
        {
            return;
        }
        Disposed += (_, _) => { foreach (var thumbnail in thumbnails.Values) thumbnail.Image.Dispose(); thumbnails.Clear(); };

        var workspace = new WorkspacePage { Name = "workspace" };
        workspace.Bind(this, heading, back, "Manage products, stock levels and availability in one workspace.", tableSurface,
            refresh, new[] { add, edit, adjust, history }, (searchLabel, search), (stockLabel, stockFilter));
        workspace.AddFilterOption(archived);

        AppStore store = AppStore.Current;
        if (!store.CanViewInventory)
        {
            throw new InvalidOperationException("Admin or Staff access required.");
        }

        add.Enabled = store.IsAdmin;
        edit.Enabled = store.IsAdmin;
        stockFilter.Items.AddRange(new object[] { "All stock", "Available", "Low stock", "Out of stock" });
        stockFilter.SelectedIndex = 0;

        search.TextChanged += FiltersChanged;
        stockFilter.SelectedIndexChanged += FiltersChanged;
        archived.CheckedChanged += FiltersChanged;
        add.Click += AddClicked;
        edit.Click += EditClicked;
        adjust.Click += AdjustStockClicked;
        history.Click += HistoryClicked;
        refresh.Visible = false;
        _ = new AutomaticViewRefresh(this, () => RefreshClicked(this, EventArgs.Empty));
        emptyInventory.ActionClicked += (_, _) =>
        {
            if (AppStore.Current.IsAdmin && !AppStore.Current.Parts.Any(p => !p.Archived)) AddClicked(this, EventArgs.Empty);
            else { filteredCategory = null; search.Clear(); stockFilter.SelectedIndex = 0; archived.Checked = false; LoadRows(); }
        };

        LoadRows();
    }

    private void TableSurface_Resize(object? sender, EventArgs e)
    {
        if (emptyInventory == null || tableSurface == null) return;
        int width = Math.Min((int)(480 * DeviceDpi / 96F), tableSurface.ClientSize.Width);
        emptyInventory.Width = width;
        emptyInventory.Location = new Point(Math.Max(0, (tableSurface.ClientSize.Width - width) / 2),
            Math.Max(0, (tableSurface.ClientSize.Height - emptyInventory.Height) / 2));
    }

    private void BackButton_Click(object? sender, EventArgs e)
    {
        // Closing this screen returns to the window that opened it.
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private void FiltersChanged(object? sender, EventArgs e)
    {
        try
        {
            LoadRows();
        }
        catch (Exception exception)
        {
            Ui.ShowError(this, exception);
        }
    }

    private void AddClicked(object? sender, EventArgs e)
    {
        try
        {
            AppStore.Current.Refresh();
            ValidateAccess(AppStore.Current);
            if (!AppStore.Current.IsAdmin)
            {
                throw new InvalidOperationException("Only an administrator can add parts.");
            }

            using PartEditForm partForm = new PartEditForm();
            partForm.ShowDialog(this);
            LoadRows();
        }
        catch (Exception exception)
        {
            Ui.ShowError(this, exception);
        }
    }

    private void EditClicked(object? sender, EventArgs e)
    {
        try
        {
            AppStore.Current.Refresh();
            ValidateAccess(AppStore.Current);
            if (!AppStore.Current.IsAdmin)
            {
                throw new InvalidOperationException("Only an administrator can edit component details.");
            }

            string selectedPartId = Ui.RequireSelection(grid);
            using PartEditForm partForm = new PartEditForm(selectedPartId);
            partForm.ShowDialog(this);
            LoadRows();
        }
        catch (Exception exception)
        {
            Ui.ShowError(this, exception);
        }
    }

    private void AdjustStockClicked(object? sender, EventArgs e)
    {
        try
        {
            AppStore.Current.Refresh();
            ValidateAccess(AppStore.Current);
            string selectedPartId = Ui.RequireSelection(grid);
            using StockAdjustmentForm adjustmentForm = new StockAdjustmentForm(selectedPartId);
            adjustmentForm.ShowDialog(this);
            LoadRows();
        }
        catch (Exception exception)
        {
            Ui.ShowError(this, exception);
        }
    }

    private void HistoryClicked(object? sender, EventArgs e)
    {
        try
        {
            AppStore.Current.Refresh();
            ValidateAccess(AppStore.Current);
            using ReportsForm reportsForm = new ReportsForm("Stock movements");
            reportsForm.ShowDialog(this);
        }
        catch (Exception exception)
        {
            Ui.ShowError(this, exception);
        }
    }

    private void RefreshClicked(object? sender, EventArgs e)
    {
        try
        {
            LoadRows();
        }
        catch (Exception exception)
        {
            Ui.ShowError(this, exception);
        }
    }

    private void LoadRows()
    {
        AppStore store = AppStore.Current;
        store.Refresh();
        ValidateAccess(store);
        string searchText = search.Text.Trim();
        heading.Text = filteredCategory == null ? "Parts & inventory" : filteredCategory + " inventory";

        DataTable table = new DataTable();
        table.Columns.Add("Id");
        table.Columns.Add("Photo", typeof(Image));
        table.Columns.Add("Component");
        table.Columns.Add("Category");
        table.Columns.Add("Price");
        table.Columns.Add("OnHand", typeof(int));
        table.Columns.Add("Reserved", typeof(int));
        table.Columns.Add("Available", typeof(int));
        table.Columns.Add("Status");

        table.Columns.Add("Archived", typeof(bool));

        foreach (Part part in store.Parts)
        {
            if (filteredCategory != null && part.Category != filteredCategory) continue;
            if (part.Archived && !archived.Checked)
            {
                continue;
            }

            string searchableText = part.Name + " " + part.Category + " " + part.Brand;
            if (!searchableText.Contains(searchText, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            int availableStock = store.Available(part);
            string stockStatus = store.StockState(part);
            bool matchesStockFilter = stockFilter.SelectedIndex == 0 || stockFilter.Text == stockStatus;
            if (!matchesStockFilter)
            {
                continue;
            }

            table.Rows.Add(part.Id, InventoryThumbnail(part, store.DatabasePath), part.Name, part.Category, Ui.Money(part.Price),
                part.Stock, store.Reserved(part.Id), availableStock, stockStatus, part.Archived);
        }

        Ui.Bind(grid, table);
        DataGridViewAutoSizeColumnsMode sizing = grid.AutoSizeColumnsMode;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        try
        {
        if (grid.Columns["Photo"] is DataGridViewImageColumn photoColumn)
        {
            photoColumn.ImageLayout = DataGridViewImageCellLayout.Zoom;
            photoColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            photoColumn.MinimumWidth = photoColumn.Width = (int)(86 * DeviceDpi / 96F);
            photoColumn.DefaultCellStyle.Padding = new Padding(6);
            photoColumn.DefaultCellStyle.NullValue = null;
            photoColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
        }
        foreach (DataGridViewRow row in grid.Rows) row.Height = (int)(72 * DeviceDpi / 96F);
        if (grid.Columns["Price"] is DataGridViewColumn priceColumn)
            priceColumn.MinimumWidth = Math.Max(priceColumn.MinimumWidth, table.Rows.Cast<DataRow>()
                .Select(row => TextRenderer.MeasureText(row["Price"].ToString(), grid.Font).Width + (int)(32 * DeviceDpi / 96F)).DefaultIfEmpty(140).Max());
        }
        finally { grid.AutoSizeColumnsMode = sizing; }
        emptyInventory.Visible = table.Rows.Count == 0;
        grid.Visible = table.Rows.Count > 0;
        TableSurface_Resize(this, EventArgs.Empty);
        if (emptyInventory.Visible)
        {
            bool emptyStore = !store.Parts.Any(p => !p.Archived);
            emptyInventory.Message = emptyStore && store.IsAdmin ? "Add products to your catalog" : "No products match these filters";
            emptyInventory.Detail = emptyStore ? "Staff can receive stock after Admin adds products." : "Try another search or stock filter.";
            emptyInventory.ActionText = emptyStore && store.IsAdmin ? "Add product" : "Clear filters";
            emptyInventory.BringToFront();
        }
    }

    private Image InventoryThumbnail(Part part, string databasePath)
    {
        string key = part.PhotoFileName;
        try { if (key.Length > 0) key += "|" + File.GetLastWriteTimeUtc(ProductPhotos.Resolve(databasePath, part.PhotoFileName)).Ticks; }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or InvalidOperationException) { key += "|unavailable"; }
        if (thumbnails.TryGetValue(part.Id, out var cached) && cached.Key == key) return cached.Image;
        using Image? source = ProductPhotos.ReadForDisplay(databasePath, part.PhotoFileName);
        Bitmap thumbnail = new(96, 80);
        using (Graphics graphics = Graphics.FromImage(thumbnail))
        {
            graphics.Clear(Color.White);
            if (source != null)
            {
                float scale = Math.Min(88F / source.Width, 72F / source.Height);
                float width = source.Width * scale, height = source.Height * scale;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(source, (96 - width) / 2, (80 - height) / 2, width, height);
            }
            else
            {
                using Font font = new("Segoe UI", 9);
                TextRenderer.DrawText(graphics, part.Category + "\nNo photo", font, new Rectangle(2, 2, 92, 76), StoreTheme.Muted,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
            }
        }
        if (thumbnails.TryGetValue(part.Id, out cached)) cached.Image.Dispose();
        thumbnails[part.Id] = (key, thumbnail);
        return thumbnail;
    }

    private void ValidateAccess(AppStore store)
    {
        add.Enabled = store.IsAdmin;
        edit.Enabled = store.IsAdmin;
        adjust.Enabled = store.IsStaff;
        history.Enabled = store.IsAdmin;
        // An unavailable role action should not look like a working button.
        int nextLeft = grid.Left;
        foreach (Button button in new[] { add, edit, adjust, history })
        {
            button.Visible = button.Enabled;
            if (button.Enabled)
            {
                if (button.Parent is not FlowLayoutPanel) button.Left = nextLeft;
                nextLeft += button.Width + 12;
            }
        }

        if (!store.CanViewInventory)
        {
            grid.DataSource = null;
            throw new InvalidOperationException("Your account no longer has inventory access. Close this screen and sign in with an authorized account.");
        }
    }

}
