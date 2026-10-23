using System.Data;

namespace CustomPC;

public partial class InventoryForm : StoreWindow
{
    private readonly ProductTablePhotos thumbnails = new();
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
        Disposed += (_, _) => thumbnails.Dispose();

        var workspace = new WorkspacePage { Name = "workspace" };
        workspace.Bind(this, heading, back, "Manage products, stock levels and availability in one workspace.", tableSurface,
            refresh, new[] { add, edit, adjust }, (searchLabel, search), (stockLabel, stockFilter));
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

            table.Rows.Add(part.Id, thumbnails.Read(part, store.DatabasePath), part.Name, part.Category, Ui.Money(part.Price),
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

    private void ValidateAccess(AppStore store)
    {
        add.Enabled = store.IsAdmin;
        edit.Enabled = store.IsAdmin;
        adjust.Enabled = store.IsStaff;
        // An unavailable role action should not look like a working button.
        int nextLeft = grid.Left;
        foreach (Button button in new[] { add, edit, adjust })
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
