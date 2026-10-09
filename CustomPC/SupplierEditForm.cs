namespace CustomPC;

public partial class SupplierEditForm : Form
{
    private string? supplierId;

    public SupplierEditForm()
    {
        InitializeComponent();
        if (Ui.IsDesign)
        {
            return;
        }

        save.Click += SaveClicked;
    }

    public SupplierEditForm(string id) : this()
    {
        supplierId = id;
        Supplier? supplier = null;
        foreach (Supplier savedSupplier in AppStore.Current.Suppliers)
        {
            if (savedSupplier.Id == id)
            {
                supplier = savedSupplier;
                break;
            }
        }

        if (supplier == null)
        {
            throw new InvalidOperationException("The selected supplier no longer exists.");
        }

        supplierName.Text = supplier.Name;
        contact.Text = supplier.Contact;
        email.Text = supplier.Email;
        address.Text = supplier.Address;
        archived.Checked = supplier.Archived;
    }

    private void SaveClicked(object? sender, EventArgs e)
    {
        try
        {
            Supplier supplier = new Supplier();
            supplier.Name = supplierName.Text.Trim();
            supplier.Contact = contact.Text.Trim();
            supplier.Email = email.Text.Trim();
            supplier.Address = address.Text.Trim();
            supplier.Archived = archived.Checked;

            if (supplierId != null)
            {
                supplier.Id = supplierId;
            }

            AppStore.Current.SaveSupplier(supplier);
            DialogResult = DialogResult.OK;
        }
        catch (Exception exception)
        {
            Ui.ShowError(this, exception);
        }
    }
}
