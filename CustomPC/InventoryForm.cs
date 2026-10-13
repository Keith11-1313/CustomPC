using System.Data;

namespace CustomPC;

public partial class InventoryForm : StoreWindow
{
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
        refresh.Click += RefreshClicked;
        emptyInventory.ActionClicked += (_, _) =>
        {
            if (AppStore.Current.IsAdmin && !AppStore.Current.Parts.Any(p => !p.Archived)) AddClicked(this, EventArgs.Empty);
            else { filteredCategory = null; search.Clear(); stockFilter.SelectedIndex = 0; LoadRows(); }
        };

        LoadRows();
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
        table.Columns.Add("Component");
        table.Columns.Add("Category");
        table.Columns.Add("Price");
        table.Columns.Add("OnHand", typeof(int));
        table.Columns.Add("Reserved", typeof(int));
        table.Columns.Add("Available", typeof(int));
        table.Columns.Add("Status");
        table.Columns.Add("Supplier");
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

            string supplierName = "Unassigned";
            foreach (Supplier supplier in store.Suppliers)
            {
                if (supplier.Id == part.SupplierId)
                {
                    supplierName = supplier.Name;
                    break;
                }
            }

            table.Rows.Add(part.Id, part.Name, part.Category, Ui.Money(part.Price),
                part.Stock, store.Reserved(part.Id), availableStock, stockStatus, supplierName, part.Archived);
        }

        Ui.Bind(grid, table);
        emptyInventory.Visible = table.Rows.Count == 0;
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
        history.Enabled = store.IsAdmin;
        // An unavailable role action should not look like a working button.
        int nextLeft = grid.Left;
        foreach (Button button in new[] { add, edit, adjust, history, refresh })
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
