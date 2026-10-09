using System.Data;

namespace CustomPC;

public partial class SuppliersForm : Form
{
    public SuppliersForm()
    {
        InitializeComponent();
        if (Ui.IsDesign)
        {
            return;
        }

        if (!AppStore.Current.IsAdmin)
        {
            throw new InvalidOperationException("Admin access required.");
        }

        search.TextChanged += FiltersChanged;
        archived.CheckedChanged += FiltersChanged;
        add.Click += AddClicked;
        edit.Click += EditClicked;
        parts.Click += SuppliedPartsClicked;
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
            AppStore.Current.Refresh();
            ValidateAccess(AppStore.Current);
            using SupplierEditForm supplierForm = new SupplierEditForm();
            supplierForm.ShowDialog(this);
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
            string selectedSupplierId = Ui.RequireSelection(grid);
            using SupplierEditForm supplierForm = new SupplierEditForm(selectedSupplierId);
            supplierForm.ShowDialog(this);
            LoadRows();
        }
        catch (Exception exception)
        {
            Ui.ShowError(this, exception);
        }
    }

    private void SuppliedPartsClicked(object? sender, EventArgs e)
    {
        try
        {
            AppStore.Current.Refresh();
            ValidateAccess(AppStore.Current);
            string selectedSupplierId = Ui.RequireSelection(grid);
            List<string> partDescriptions = new List<string>();
            foreach (Part part in AppStore.Current.Parts)
            {
                if (part.SupplierId == selectedSupplierId)
                {
                    partDescriptions.Add($"{part.Name} | {part.Category} | stock {part.Stock}");
                }
            }

            if (partDescriptions.Count == 0)
            {
                partDescriptions.Add("No components assigned.");
            }

            MessageBox.Show(this, string.Join(Environment.NewLine, partDescriptions), "Supplied components");
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

        DataTable table = new DataTable();
        table.Columns.Add("Id");
        table.Columns.Add("Supplier");
        table.Columns.Add("Contact");
        table.Columns.Add("Email");
        table.Columns.Add("Address");
        table.Columns.Add("Components", typeof(int));
        table.Columns.Add("Archived", typeof(bool));

        foreach (Supplier supplier in store.Suppliers)
        {
            if (supplier.Archived && !archived.Checked)
            {
                continue;
            }

            string searchableText = supplier.Name + " " + supplier.Contact;
            if (!searchableText.Contains(searchText, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            int componentCount = 0;
            foreach (Part part in store.Parts)
            {
                if (part.SupplierId == supplier.Id)
                {
                    componentCount++;
                }
            }

            table.Rows.Add(supplier.Id, supplier.Name, supplier.Contact, supplier.Email,
                supplier.Address, componentCount, supplier.Archived);
        }

        Ui.Bind(grid, table);
    }

    private void ValidateAccess(AppStore store)
    {
        add.Enabled = store.IsAdmin;
        edit.Enabled = store.IsAdmin;
        parts.Enabled = store.IsAdmin;

        if (!store.IsAdmin)
        {
            grid.DataSource = null;
            throw new InvalidOperationException("Your account no longer has admin access. Close this screen and sign in with an administrator account.");
        }
    }
}
