using System.Data;

namespace CustomPC;

public partial class DashboardForm : StoreWindow
{
    private readonly System.Windows.Forms.Timer expiryTimer = new System.Windows.Forms.Timer
    {
        Interval = 30000
    };

    public DashboardForm()
    {
        InitializeComponent();

        if (Ui.IsDesign)
        {
            return;
        }

        catalog.Click += CatalogButton_Click;
        builder.Click += BuilderButton_Click;
        orders.Click += OrdersButton_Click;
        inventory.Click += InventoryButton_Click;
        suppliers.Click += SuppliersButton_Click;
        users.Click += UsersButton_Click;
        rules.Click += RulesButton_Click;
        reports.Click += ReportsButton_Click;
        tools.Click += ToolsButton_Click;
        recent.CellDoubleClick += RecentOrder_DoubleClick;
        signout.Click += SignOutButton_Click;
        Shown += DashboardForm_Shown;
        FormClosed += DashboardForm_FormClosed;
        expiryTimer.Tick += ExpiryTimer_Tick;
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

    private void ToolsButton_Click(object? sender, EventArgs e)
    {
        OpenScreen("Store tools");
    }

    private void RecentOrder_DoubleClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;
        try
        {
            string id = recent.Rows[e.RowIndex].Cells["Id"].Value?.ToString()
                ?? throw new InvalidOperationException("Select an order first.");
            using ReceiptForm receipt = new ReceiptForm(id);
            receipt.ShowDialog(this);
        }
        catch (Exception exception) { Ui.ShowError(this, exception); }
    }

    private void SignOutButton_Click(object? sender, EventArgs e)
    {
        SignIn();
    }

    private void DashboardForm_Shown(object? sender, EventArgs e)
    {
        try
        {
            UpdateDashboard();
            expiryTimer.Start();
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
                case "Store tools":
                    form = new StoreToolsForm();
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
        // Switching accounts should show only login, just like a fresh app launch.
        Hide();
        try
        {
            AppStore.Current.Logout();

            using (LoginForm loginForm = new LoginForm())
            {
                if (loginForm.ShowDialog() != DialogResult.OK)
                {
                    Close();
                    return;
                }
            }

            UpdateDashboard();
            Show();
            Activate();
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

        heading.Text = store.CanViewAllOrders ? "Store overview" : store.Session == null ? "Welcome to CustomPC" : "My dashboard";
        Text = "CustomPC | " + heading.Text;

        if (store.Session == null)
        {
            identity.Text = "Guest workspace";
        }
        else
        {
            identity.Text = $"Welcome, {store.Session.Name}";
        }

        inventory.Enabled = store.CanViewInventory;
        reports.Enabled = store.IsAdmin;
        suppliers.Enabled = store.IsAdmin;
        users.Enabled = store.IsAdmin;
        rules.Enabled = store.IsAdmin;
        tools.Enabled = store.IsAdmin;
        orders.Enabled = store.CanViewAllOrders || store.IsCustomer;
        builder.Enabled = store.IsCustomer || store.Session == null;
        builder.Text = store.Session == null ? "PC builder preview" : "PC builder";
        orders.Text = store.IsAdmin ? "View orders" : store.IsStaff ? "Orders & payments" : "My orders";
        ArrangeRoleMenu();

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

        if (store.CanViewAllOrders)
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

            metrics.Text = "Inventory and order summary";
            statParts.Text = activeParts.ToString();
            statOrders.Text = pendingOrders.ToString();
            statStock.Text = lowStockParts.ToString();
            statSales.Text = store.IsAdmin
                ? totalSales.ToString("N0")
                : store.Orders.Count(o => o.Status == "Ready for Pickup").ToString();
            statSalesCaption.Text = store.IsAdmin ? "Recorded sales (PHP)" : "Ready for pickup";

            guide.Text = store.IsAdmin
                ? "Manage accounts and assign roles in User accounts.\n\nMaintain parts, suppliers and rules.\n\nMonitor orders and reports."
                : "Confirm payment after receiving it at the store.\n\nUpdate assembly and pickup in Orders.\n\nAdjust stock in Inventory.";
        }
        else
        {
            int customerOrderCount = store.VisibleOrders().Count;
            metrics.Text = "Your next PC starts here";
            statParts.Text = availableParts.ToString();
            statPartsCaption.Text = "Parts available";
            statOrders.Text = customerOrderCount.ToString();
            statOrdersCaption.Text = "Your orders";
            statStock.Text = "24h";
            statStockCaption.Text = "Payment deadline";
            statSales.Text = "In store";
            statSalesCaption.Text = "Payment & pickup";
            guide.Text = "Choose your parts in PC builder.\n\nConfirm the order and keep your receipt.\n\nPay at the store within 24 hours.";
        }
        if (store.CanViewAllOrders)
        {
            statPartsCaption.Text = "Active parts";
            statOrdersCaption.Text = "Awaiting payment";
            statStockCaption.Text = "Low / out of stock";
        }
        DataTable recentOrders = new DataTable();
        recentOrders.Columns.Add("Id"); recentOrders.Columns.Add("Order");
        if (store.CanViewAllOrders) recentOrders.Columns.Add("Customer");
        recentOrders.Columns.Add("Status");
        foreach (CustomerOrder order in store.VisibleOrders().OrderByDescending(o => o.CreatedUtc).Take(6))
        {
            if (store.CanViewAllOrders) recentOrders.Rows.Add(order.Id, order.Number, order.CustomerName, order.Status);
            else recentOrders.Rows.Add(order.Id, order.Number, order.Status);
        }
        Ui.Bind(recent, recentOrders);
    }

    private void ArrangeRoleMenu()
    {
        // Show only this role's screens, without leaving gaps in the sidebar.
        int nextTop = catalog.Top;
        int left = catalog.Left;
        int spacing = catalog.Height + 6;
        Button[] menu = { catalog, builder, orders, inventory, suppliers, users, rules, reports, tools };
        foreach (Button button in menu)
        {
            button.Visible = button.Enabled;
            if (button.Enabled)
            {
                button.Location = new Point(left, nextTop);
                nextTop += spacing;
            }
        }
    }
}
