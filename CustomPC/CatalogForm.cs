using System.Data;

namespace CustomPC;

public partial class CatalogForm : Form
{
    public CatalogForm()
    {
        InitializeComponent();

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
        grid.CellDoubleClick += PartsGrid_CellDoubleClick;

        LoadRows();
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
            Ui.ShowError(this, exception);
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

            string details = $"{selectedPart.Name}\n" +
                $"{selectedPart.Brand} / {selectedPart.Category}\n" +
                $"{Ui.Money(selectedPart.Price)}\n\n" +
                $"{selectedPart.Specifications}\n" +
                $"Socket: {selectedPart.Socket}\n" +
                $"Memory type: {selectedPart.MemoryType}\n" +
                $"Wattage: {selectedPart.Watts}\n" +
                $"Available: {AppStore.Current.Available(selectedPart)}";

            MessageBox.Show(this, details, "Component details");
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
        }

        Ui.Bind(grid, table);
    }
}
