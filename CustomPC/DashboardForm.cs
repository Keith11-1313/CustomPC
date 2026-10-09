namespace CustomPC;

public partial class DashboardForm : Form
{
    private readonly System.Windows.Forms.Timer expiryTimer = new System.Windows.Forms.Timer
    {
        Interval = 30000
    };
    private readonly bool askForLogin;

    public DashboardForm() : this(true)
    {
    }

    public DashboardForm(bool askForLogin)
    {
        InitializeComponent();

        if (Ui.IsDesign)
        {
            return;
        }

        this.askForLogin = askForLogin;
        catalog.Click += CatalogButton_Click;
        builder.Click += BuilderButton_Click;
        orders.Click += OrdersButton_Click;
        inventory.Click += InventoryButton_Click;
        suppliers.Click += SuppliersButton_Click;
        users.Click += UsersButton_Click;
        rules.Click += RulesButton_Click;
        reports.Click += ReportsButton_Click;
        signout.Click += SignOutButton_Click;
        Shown += DashboardForm_Shown;
        FormClosed += DashboardForm_FormClosed;
        expiryTimer.Tick += ExpiryTimer_Tick;

        expiryTimer.Start();
    }

    private void CatalogButton_Click(object? sender, EventArgs e)
    {
        OpenScreen("Catalog");
    }

    private void BuilderButton_Click(object? sender, EventArgs e)
    {
        OpenScreen("Builder");
    }

    private void OrdersButton_Click(object? sender, EventArgs e)
    {
        OpenScreen("Orders");
    }

    private void InventoryButton_Click(object? sender, EventArgs e)
    {
        OpenScreen("Inventory");
    }

    private void SuppliersButton_Click(object? sender, EventArgs e)
    {
        OpenScreen("Suppliers");
    }

    private void UsersButton_Click(object? sender, EventArgs e)
    {
        OpenScreen("Users");
    }

    private void RulesButton_Click(object? sender, EventArgs e)
    {
        OpenScreen("Compatibility");
    }

    private void ReportsButton_Click(object? sender, EventArgs e)
    {
        OpenScreen("Reports");
    }

    private void SignOutButton_Click(object? sender, EventArgs e)
    {
        SignIn();
    }

    private void DashboardForm_Shown(object? sender, EventArgs e)
    {
        try
        {
            if (askForLogin)
            {
                SignIn();
            }
            else
            {
                UpdateDashboard();
            }
        }
        catch (Exception exception)
        {
            Ui.ShowError(this, exception);
            Close();
        }
    }

    private void ExpiryTimer_Tick(object? sender, EventArgs e)
    {
        try
        {
            // Refresh also cancels unpaid orders whose payment deadline has passed.
            UpdateDashboard();
        }
        catch (Exception exception)
        {
            Ui.ShowError(this, exception);
        }
    }

    private void DashboardForm_FormClosed(object? sender, FormClosedEventArgs e)
    {
        expiryTimer.Dispose();
    }

    private void OpenScreen(string screenName)
    {
        try
        {
            // Construct the screen inside the try block too: loading its data can fail.
            AppStore.Current.Refresh();
            Form form;

            switch (screenName)
            {
                case "Catalog":
                    form = new CatalogForm();
                    break;
                case "Builder":
                    form = new BuilderForm();
                    break;
                case "Orders":
                    form = new OrdersForm();
                    break;
                case "Inventory":
                    form = new InventoryForm();
                    break;
                case "Suppliers":
                    form = new SuppliersForm();
                    break;
                case "Users":
                    form = new UsersForm();
                    break;
                case "Compatibility":
                    form = new CompatibilityForm();
                    break;
                case "Reports":
                    form = new ReportsForm();
                    break;
                default:
                    throw new InvalidOperationException("This screen is not available.");
            }

            using (form)
            {
                form.ShowDialog(this);
            }

            UpdateDashboard();
        }
        catch (Exception exception)
        {
            Ui.ShowError(this, exception);
        }
    }

    private void SignIn()
    {
        expiryTimer.Stop();
        try
        {
            AppStore.Current.Logout();

            using (LoginForm loginForm = new LoginForm())
            {
                if (loginForm.ShowDialog(this) != DialogResult.OK)
                {
                    Close();
                    return;
                }
            }

            UpdateDashboard();
            expiryTimer.Start();
        }
        catch (Exception exception)
        {
            Ui.ShowError(this, exception);
            Close();
        }
    }

    private void UpdateDashboard()
    {
        AppStore store = AppStore.Current;
        store.Refresh();

        if (store.Session == null)
        {
            identity.Text = "Guest workspace";
        }
        else
        {
            identity.Text = $"Welcome, {store.Session.Name}   /   {store.Session.Role}";
        }

        inventory.Enabled = store.IsStaff;
        reports.Enabled = store.IsStaff;
        suppliers.Enabled = store.IsAdmin;
        users.Enabled = store.IsAdmin;
        rules.Enabled = store.IsAdmin;
        orders.Enabled = store.Session != null;

        int activeParts = 0;
        int availableParts = 0;
        int lowStockParts = 0;

        foreach (Part part in store.Parts)
        {
            if (part.Archived)
            {
                continue;
            }

            activeParts++;
            int availableStock = store.Available(part);
            if (availableStock > 0)
            {
                availableParts++;
            }

            if (availableStock <= part.LowStock)
            {
                lowStockParts++;
            }
        }

        if (store.IsStaff)
        {
            int pendingOrders = 0;
            decimal totalSales = 0;

            foreach (CustomerOrder order in store.Orders)
            {
                if (order.Status == "Pending Payment")
                {
                    pendingOrders++;
                }
            }

            foreach (Payment payment in store.Payments)
            {
                totalSales += payment.Amount;
            }

            metrics.Text = $"{activeParts} active parts   /   {pendingOrders} awaiting payment\n" +
                $"{lowStockParts} low / out of stock   /   {Ui.Money(totalSales)} sales";

            guide.Text = "STORE WORKFLOW\n\n" +
                "Review orders and confirm in-store payments.\n" +
                "Move paid orders through processing, assembly and pickup.\n" +
                "Monitor reserved stock and record inventory adjustments.\n" +
                "Use reports to review orders, stock and recorded sales.";
        }
        else
        {
            int customerOrderCount = store.VisibleOrders().Count;
            metrics.Text = $"{availableParts} parts available   /   {customerOrderCount} your orders";
            guide.Text = "01   Browse available components\n\n" +
                "02   Select a compatible build\n\n" +
                "03   Confirm your order and keep the receipt\n\n" +
                "04   Pay at the store within 24 hours\n\n" +
                "05   Track assembly and collect your PC";
        }
    }
}
