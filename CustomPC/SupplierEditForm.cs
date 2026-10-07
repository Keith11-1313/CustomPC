namespace CustomPC;

public partial class SupplierEditForm : Form
{
    private string? supplierId;
    public SupplierEditForm()
    {
        InitializeComponent(); if (Ui.IsDesign) return;
        save.Click += (_, _) => Ui.Run(this, () => { var s = new Supplier { Name = supplierName.Text.Trim(), Contact = contact.Text.Trim(), Email = email.Text.Trim(), Address = address.Text.Trim(), Archived = archived.Checked }; if (supplierId != null) s.Id = supplierId; AppStore.Current.SaveSupplier(s); DialogResult = DialogResult.OK; });
    }
    public SupplierEditForm(string id) : this() { supplierId = id; var s = AppStore.Current.Suppliers.Single(x => x.Id == id); supplierName.Text = s.Name; contact.Text = s.Contact; email.Text = s.Email; address.Text = s.Address; archived.Checked = s.Archived; }
}
