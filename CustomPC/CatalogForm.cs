using System.Data;

namespace CustomPC;

public partial class CatalogForm : StoreWindow
{
    private BuildDraft draft = new();
    public CatalogForm(BuildDraft sharedDraft) : this() { draft = sharedDraft; }
    public CatalogForm()
    {
        InitializeComponent();
        back.Click += BackButton_Click;

        if (Ui.IsDesign)
        {
            return;
        }

        category.Items.Add("All categories");
        category.Items.AddRange(AppStore.Categories);
        category.SelectedIndex = 0;

        availability.Items.AddRange(new object[]
        {
            "All active parts", "Available", "Low stock", "Out of stock"
        });
        availability.SelectedIndex = 0;

        search.TextChanged += FilterChanged;
        category.SelectedIndexChanged += FilterChanged;
        availability.SelectedIndexChanged += FilterChanged;
        refresh.Click += RefreshButton_Click;
        emptyProducts.ActionClicked += EmptyProducts_Click;
        products.SizeChanged += (_, _) => FitProductCards();
        products.ControlAdded += (_, _) => FitProductCards();
        grid.CellDoubleClick += PartsGrid_CellDoubleClick;

        LoadRows();
    }

    private void BackButton_Click(object? sender, EventArgs e)
    {
        // Closing this screen returns to the window that opened it.
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private void FitProductCards()
    {
        // Fill each row evenly instead of leaving a large unused strip on the right.
        int available = products.ClientSize.Width - products.Padding.Horizontal - SystemInformation.VerticalScrollBarWidth;
        int columns = Math.Max(1, available / 278);
        int width = Math.Max(220, available / columns - 20);
        foreach (ProductCard card in products.Controls.OfType<ProductCard>()) card.Width = width;
    }

    private void FilterChanged(object? sender, EventArgs e)
    {
        LoadRows();
    }

    private void RefreshButton_Click(object? sender, EventArgs e)
    {
        try
        {
            AppStore.Current.Refresh();
            LoadRows();
        }
        catch (Exception exception)
        {
            AppLog.Write(exception);
            emptyProducts.Message = "Products could not be loaded";
            emptyProducts.Detail = "Check store file access, then retry.";
            emptyProducts.ActionText = "Retry";
            emptyProducts.Visible = true;
            emptyProducts.BringToFront();
        }
    }

    private void PartsGrid_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0)
        {
            return;
        }

        try
        {
            string? partId = grid.Rows[e.RowIndex].Cells["Id"].Value?.ToString();
            Part? selectedPart = null;

            foreach (Part part in AppStore.Current.Parts)
            {
                if (part.Id == partId)
                {
                    selectedPart = part;
                    break;
                }
            }

            if (selectedPart == null)
            {
                throw new InvalidOperationException("This component could not be found. Refresh the catalog.");
            }

            using ProductDetailsForm details = new(selectedPart);
            details.ShowDialog(this);
        }
        catch (Exception exception)
        {
            Ui.ShowError(this, exception);
        }
    }

    private void LoadRows()
    {
        AppStore store = AppStore.Current;
        string searchText = search.Text.Trim();

        // The table contains only the columns displayed in the catalog.
        DataTable table = new DataTable();
        foreach (Control card in products.Controls.Cast<Control>().ToArray()) card.Dispose();
        products.Controls.Clear();
        table.Columns.Add("Id", typeof(string));
        table.Columns.Add("Component", typeof(string));
        table.Columns.Add("Category", typeof(string));
        table.Columns.Add("Brand", typeof(string));
        table.Columns.Add("Price", typeof(string));
        table.Columns.Add("Specifications", typeof(string));
        table.Columns.Add("Available", typeof(int));
        table.Columns.Add("Status", typeof(string));

        foreach (Part part in store.Parts)
        {
            if (part.Archived)
            {
                continue;
            }

            if (category.SelectedIndex != 0 && part.Category != category.Text)
            {
                continue;
            }

            int availableStock = store.Available(part);
            string stockStatus = store.StockState(part);
            bool matchesAvailability = availability.SelectedIndex == 0 ||
                stockStatus == availability.Text ||
                (availability.Text == "Available" && availableStock > 0);

            if (!matchesAvailability)
            {
                continue;
            }

            string searchableDetails = $"{part.Name} {part.Brand} {part.Specifications}";
            if (!searchableDetails.Contains(searchText, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            table.Rows.Add(part.Id, part.Name, part.Category, part.Brand,
                Ui.Money(part.Price), part.Specifications, availableStock, stockStatus);
            Action<Part>? select = store.Session == null || store.IsCustomer ? AddToBuild : null;
            products.Controls.Add(new ProductCard(part, store, select));
        }

        Ui.Bind(grid, table);
        emptyProducts.Visible = table.Rows.Count == 0;
        if (emptyProducts.Visible)
        {
            bool emptyStore = !store.Parts.Any(p => !p.Archived);
            emptyProducts.Message = emptyStore && store.IsAdmin ? "Add products to your catalog" : emptyStore ? "The catalog is getting ready" : "No products match these filters";
            emptyProducts.Detail = emptyStore ? "Check back after the store adds its products." : "Try another category or search term.";
            emptyProducts.ActionText = emptyStore && store.IsAdmin ? "Add product" : emptyStore ? "" : "Clear filters";
            emptyProducts.BringToFront();
        }
    }

    private void EmptyProducts_Click(object? sender, EventArgs e)
    {
        if (emptyProducts.ActionText == "Retry") { RefreshButton_Click(sender, e); return; }
        AppStore store = AppStore.Current;
        if (store.IsAdmin && !store.Parts.Any(p => !p.Archived))
        {
            if (Parent?.FindForm() is DashboardForm dashboard) dashboard.OpenScreen("Inventory");
            else { using InventoryForm inventory = new(); inventory.ShowDialog(this); LoadRows(); }
            return;
        }
        search.Clear(); category.SelectedIndex = 0; availability.SelectedIndex = 0;
    }

    private void AddToBuild(Part selected)
    {
        try
        {
            AppStore store = AppStore.Current;
            store.Refresh();
            if (store.Session != null && !store.IsCustomer) throw new InvalidOperationException("Customer access is required.");
            Part current = store.Parts.First(p => p.Id == selected.Id && !p.Archived);
            if (store.Available(current) <= 0) { LoadRows(); return; }
            draft.Select(current.Category, current.Id);
            if (Parent?.FindForm() is DashboardForm dashboard) dashboard.OpenScreen("Builder");
            else { using BuilderForm builder = new(draft); builder.ShowDialog(this); }
        }
        catch (Exception error) { Ui.ShowError(this, error); }
    }
}
