using System.Data;

namespace CustomPC;

public partial class InventoryForm : Form
{
    public InventoryForm()
    {
        InitializeComponent();
        if (Ui.IsDesign)
        {
            return;
        }

        AppStore store = AppStore.Current;
        if (!store.IsStaff)
        {
            throw new InvalidOperationException("Staff access required.");
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

        LoadRows();
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
        string searchText = search.Text.Trim();

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
            if (stockFilter.Text == "Available" && availableStock > 0)
            {
                matchesStockFilter = true;
            }
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
    }
}
