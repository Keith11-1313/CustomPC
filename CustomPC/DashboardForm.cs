namespace CustomPC;

public partial class DashboardForm : Form
{
    private readonly System.Windows.Forms.Timer expiryTimer = new() { Interval = 30000 };
    public DashboardForm() : this(true) { }
    public DashboardForm(bool askForLogin)
    {
        InitializeComponent(); if (Ui.IsDesign) return;
        catalog.Click += (_, _) => Open(new CatalogForm()); builder.Click += (_, _) => Open(new BuilderForm());
        orders.Click += (_, _) => Open(new OrdersForm()); inventory.Click += (_, _) => Open(new InventoryForm());
        suppliers.Click += (_, _) => Open(new SuppliersForm()); users.Click += (_, _) => Open(new UsersForm());
        rules.Click += (_, _) => Open(new CompatibilityForm()); reports.Click += (_, _) => Open(new ReportsForm());
        signout.Click += (_, _) => SignIn(); Shown += (_, _) => { if (askForLogin) SignIn(); else UpdateDashboard(); };
        expiryTimer.Tick += (_, _) => Ui.Run(this, UpdateDashboard); expiryTimer.Start();
        FormClosed += (_, _) => expiryTimer.Dispose();
    }
    private void Open(Form form)
    {
        using (form) Ui.Run(this, () => { form.ShowDialog(this); UpdateDashboard(); });
    }
    private void SignIn()
    {
        expiryTimer.Stop(); AppStore.Current.Logout();
        using var f = new LoginForm(); if (f.ShowDialog(this) != DialogResult.OK) { Close(); return; }
        UpdateDashboard(); expiryTimer.Start();
    }
    private void UpdateDashboard()
    {
        var s = AppStore.Current; s.Refresh();
        identity.Text = s.Session == null ? "Guest workspace" : $"Welcome, {s.Session.Name}   /   {s.Session.Role}";
        inventory.Enabled = reports.Enabled = s.IsStaff; suppliers.Enabled = users.Enabled = rules.Enabled = s.IsAdmin;
        orders.Enabled = s.Session != null;
        var visible = s.VisibleOrders();
        metrics.Text = s.IsStaff ? $"{s.Parts.Count(x => !x.Archived)} active parts   /   {s.Orders.Count(x => x.Status == "Pending Payment")} awaiting payment\n{s.Parts.Count(x => !x.Archived && s.Available(x) <= x.LowStock)} low / out of stock   /   {Ui.Money(s.Payments.Sum(x => x.Amount))} sales" : $"{s.Parts.Count(x => !x.Archived && s.Available(x) > 0)} parts available   /   {visible.Count} your orders";
        guide.Text = s.IsStaff ? "STORE WORKFLOW\n\nReview orders and confirm in-store payments.\nMove paid orders through processing, assembly and pickup.\nMonitor reserved stock and record inventory adjustments.\nUse reports to review orders, stock and recorded sales." : "01   Browse available components\n\n02   Select a compatible build\n\n03   Confirm your order and keep the receipt\n\n04   Pay at the store within 24 hours\n\n05   Track assembly and collect your PC";
    }
}
