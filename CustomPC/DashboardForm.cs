using System.Data;

namespace CustomPC;

public partial class DashboardForm : StoreWindow
{
    public BuildDraft Draft { get; } = new();
    private Form? hostedPage;
    private string dashboardIdentity = "";
    private Action? retryPage;
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
        home.Click += (_, _) => ShowOverview();
        pageError.ActionClicked += (_, _) => retryPage?.Invoke();
        emptyRecent.ActionClicked += (_, _) => OpenScreen("Builder");
        period.SelectedIndexChanged += (_, _) => { if (period.SelectedIndex >= 0) RefreshCharts(); };
        trendChart.ChartActivated += (_, _) => OpenChartOrders(false);
        distributionChart.ChartActivated += (_, _) => OpenChartOrders(true);
        stockChart.ChartActivated += (_, _) => OpenScreen("Inventory");
        trendChart.SelectionOpened += selection => OpenChartSelection(selection);
        distributionChart.SelectionOpened += selection => OpenChartSelection(selection);
        stockChart.SelectionOpened += selection => OpenChartSelection(selection);
        trendChart.EmptyActionRequested += (_, _) => { period.Focus(); period.DroppedDown = true; };
        distributionChart.EmptyActionRequested += (_, _) => OpenScreen("Orders");
        stockChart.EmptyActionRequested += (_, _) => OpenScreen("Inventory");
        viewPeriodOrders.Click += (_, _) => OpenChartOrders(false);
        viewQueueOrders.Click += (_, _) => OpenChartOrders(true);
        viewStock.Click += (_, _) => OpenScreen("Inventory");
        builder.Click += BuilderButton_Click;
        orders.Click += OrdersButton_Click;
        inventory.Click += InventoryButton_Click;

        users.Click += UsersButton_Click;

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

    private void UsersButton_Click(object? sender, EventArgs e)
    {
        OpenScreen("Users");
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
            ShowDashboardFailure(exception);
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
            ShowDashboardFailure(exception);
        }
    }

    private void DashboardForm_FormClosed(object? sender, FormClosedEventArgs e)
    {
        expiryTimer.Dispose();
        pageError.Dispose();
    }

    private void ShowDashboardFailure(Exception error)
    {
        AppLog.Write(error);
        trendChart.ClearRuntimeData(); distributionChart.ClearRuntimeData(); stockChart.ClearRuntimeData();
        hostedPage?.Dispose(); hostedPage = null;
        pageHost.Controls.Clear();
        pageError.Message = "The dashboard could not be loaded";
        pageError.Detail = "Check file access and available disk space, then retry.";
        pageError.ActionText = "Retry";
        pageError.Visible = true;
        pageHost.Controls.Add(pageError);
        retryPage = () =>
        {
            try { ShowOverview(); expiryTimer.Start(); }
            catch (Exception retryError) { ShowDashboardFailure(retryError); }
        };
        foreach (Control control in Controls)
            if (control != sidebar && control != pageHost) control.Visible = false;
        pageHost.Visible = true; pageHost.BringToFront();
    }

    public void OpenScreen(string screenName, OrderViewFilter? orderFilter = null, InventoryViewFilter? inventoryFilter = null)
    {
        UseWaitCursor = true;
        try
        {
            // Construct the screen inside the try block too: loading its data can fail.
            AppStore.Current.Refresh();
            Form form;

            switch (screenName)
            {
                case "Catalog":
                    form = new CatalogForm(Draft);
                    break;
                case "Builder":
                    form = new BuilderForm(Draft);
                    break;
                case "Orders":
                    form = orderFilter == null ? new OrdersForm() : new OrdersForm(orderFilter);
                    break;
                case "Inventory":
                    form = inventoryFilter == null ? new InventoryForm() : new InventoryForm(inventoryFilter);
                    break;
                case "Users":
                    form = new UsersForm();
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

            hostedPage?.Dispose();
            hostedPage = form;
            foreach (Control control in Controls)
                if (control != sidebar && control != pageHost) control.Visible = false;
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.MinimumSize = Size.Empty;
            form.Dock = DockStyle.Fill;
            form.AutoScroll = true;
            form.AutoScrollMinSize = new Size(screenName is "Catalog" or "Builder" ? 820 : 1100, 650);
            pageHost.Controls.Clear();
            pageError.Visible = false;
            pageHost.Controls.Add(form);
            pageHost.Visible = true;
            pageHost.BringToFront();
            form.FormClosed += (_, _) => { if (!IsDisposed) ShowOverview(); };
            form.Show();
            Text = form.Text;
            StyleMenu(screenName);
        }
        catch (Exception exception)
        {
            hostedPage?.Dispose(); hostedPage = null;
            foreach (Control control in Controls)
                if (control != sidebar && control != pageHost) control.Visible = false;
            pageHost.Controls.Clear();
            pageHost.Controls.Add(pageError);
            AppLog.Write(exception);
            pageError.Message = "This page could not be loaded";
            pageError.ActionText = "Retry";
            pageError.Detail = exception is InvalidOperationException ? exception.Message : "Check store file access, then retry.";
            pageError.Visible = true;
            pageHost.Visible = true;
            pageHost.BringToFront();
            retryPage = () => OpenScreen(screenName, orderFilter, inventoryFilter);
        }
        finally { UseWaitCursor = false; }
    }

    public void ShowOverview()
    {
        pageHost.Visible = false;
        hostedPage?.Dispose();
        hostedPage = null;
        foreach (Control control in Controls)
            if (control != pageHost) control.Visible = true;
        UpdateDashboard();
        StyleMenu("Overview");
    }

    public void PauseForCheckoutLogin()
    {
        expiryTimer.Stop();
        Hide();
    }

    public void ResumeAfterCheckoutLogin()
    {
        if (IsDisposed) return;
        try { UpdateDashboard(); }
        finally
        {
            if (!IsDisposed) { Show(); Activate(); expiryTimer.Start(); }
        }
    }

    private void SignIn()
    {
        bool wasGuest = AppStore.Current.Session == null;
        expiryTimer.Stop();
        // Switching accounts should show only login, just like a fresh app launch.
        Hide();
        try
        {
            AppStore.Current.Logout();
            Draft.Clear();
            hostedPage?.Dispose();
            hostedPage = null;
            pageHost.Visible = false;

            using (LoginForm loginForm = new LoginForm())
            {
                if (loginForm.ShowDialog() != DialogResult.OK)
                {
                    if (wasGuest)
                    {
                        UpdateDashboard();
                        Show();
                        Activate();
                        expiryTimer.Start();
                    }
                    else Close();
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
        string currentIdentity = (store.Session?.Id ?? "Guest") + ":" + store.Session?.Role;
        if (dashboardIdentity != currentIdentity)
        {
            trendChart.ClearRuntimeData(); distributionChart.ClearRuntimeData(); stockChart.ClearRuntimeData();
            bool guestContinuation = dashboardIdentity.StartsWith("Guest:") && store.IsCustomer;
            if (dashboardIdentity.Length > 0 && !guestContinuation)
            {
                Draft.Clear();
                hostedPage?.Dispose(); hostedPage = null; pageHost.Visible = false;
                foreach (Control control in Controls) if (control != pageHost) control.Visible = true;
            }
            dashboardIdentity = currentIdentity;
            period.SelectedIndex = store.IsAdmin ? 1 : 0;
        }

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

        users.Enabled = store.IsAdmin;

        tools.Enabled = store.IsAdmin;
        orders.Enabled = store.CanViewAllOrders || store.IsCustomer;
        builder.Enabled = store.IsCustomer || store.Session == null;
        builder.Text = store.Session == null ? "PC builder preview" : "PC builder";
        signout.Text = store.Session == null ? "Sign in" : "Sign out";
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
                ? "Manage accounts and assign roles in User accounts.\n\nMaintain parts.\n\nMonitor orders and reports."
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
        bool operational = store.IsAdmin || store.IsStaff;
        chartArea.Visible = operational;
        period.Visible = operational;
        summarySurface.Visible = !operational;
        workflowSurface.Visible = !operational;
        recent.Visible = !operational && store.IsCustomer;
        recentTitle.Visible = !operational && store.IsCustomer;
        emptyRecent.Visible = store.IsCustomer && recentOrders.Rows.Count == 0;
        if (emptyRecent.Visible) { recent.Visible = false; emptyRecent.BringToFront(); }
        statOrders.Visible = store.Session != null;
        statOrdersCaption.Visible = store.Session != null;
        if (operational) RefreshCharts();
        chartActions.Visible = operational;
        chartSummary.Visible = operational;
        viewQueueOrders.Text = store.IsAdmin ? "View paid parts orders" : "View current work queue";
        if (pageHost.Visible)
        {
            foreach (Control control in Controls)
                if (control != sidebar && control != pageHost) control.Visible = false;
        }
        else StyleMenu("Overview");
    }

    private void OpenChartOrders(bool distribution)
    {
        AppStore store = AppStore.Current;
        store.Refresh();
        if (!store.CanViewAllOrders) return;
        int days = period.SelectedIndex == 0 ? 7 : period.SelectedIndex == 2 ? 90 : 30;
        DateTime last = DateTime.Today;
        OrderViewFilter filter = store.IsStaff && distribution
            ? new(WorkQueue: true)
            : new(last.AddDays(1 - days), last, PaidDate: store.IsAdmin);
        OpenScreen("Orders", filter);
    }

    private void OpenChartSelection(ChartSelection selection)
    {
        AppStore store = AppStore.Current;
        store.Refresh();
        if (!store.CanViewAllOrders) return;
        if (selection.StockState != null || selection.Category != null)
        {
            OpenScreen("Inventory", inventoryFilter: new(selection.Category, selection.StockState));
            return;
        }
        int days = period.SelectedIndex == 0 ? 7 : period.SelectedIndex == 2 ? 90 : 30;
        if (store.IsStaff && selection.Status != null)
        {
            OpenScreen("Orders", new(Status: selection.Status, WorkQueue: true));
            return;
        }
        DateTime last = selection.Date ?? DateTime.Today;
        DateTime first = selection.Date ?? last.AddDays(1 - days);
        OpenScreen("Orders", new(first, last, PaidDate: store.IsAdmin, Status: selection.Status,
            PartId: selection.PartId, WorkQueue: store.IsStaff && selection.Status != null));
    }

    private void RefreshCharts()
    {
        try { DrawCharts(); }
        catch (Exception error) { ShowDashboardFailure(error); }
    }

    private void DrawCharts()
    {
        AppStore store = AppStore.Current;
        int days = period.SelectedIndex == 0 ? 7 : period.SelectedIndex == 2 ? 90 : 30;
        if (store.IsAdmin)
        {
            var data = store.ReadAdminDashboard(days);
            chartSummary.Text = $"{data.FirstDate:MMM d}–{data.LastDate:MMM d}: PHP {data.Sales.Sum(value => value.Value):N2} confirmed payments. Inventory is a current snapshot.";
            trendChart.EmptyActionText = "Change period";
            distributionChart.EmptyActionText = "View orders";
            stockChart.EmptyActionText = "Add products";
            trendChart.Line("Recorded sales", data.Sales, "PHP", "No confirmed payments in this period");
            distributionChart.Bars("Top five selling parts", data.TopParts, true, "No confirmed payments in this period");
            stockChart.Donut(data.Inventory);
        }
        else if (store.IsStaff)
        {
            var data = store.ReadStaffDashboard(days);
            chartSummary.Text = $"{data.Incoming.Sum(value => value.Value):N0} incoming orders in {days} days. {data.Queue.Sum(value => value.Value):N0} orders in the current work queue.";
            trendChart.EmptyActionText = "Change period";
            distributionChart.EmptyActionText = "View all orders";
            stockChart.EmptyActionText = "View inventory";
            trendChart.Line("Incoming orders", data.Incoming, "Orders", "No incoming orders in this period");
            distributionChart.Bars("Work queue · current", data.Queue, false, "No orders need this action right now");
            stockChart.StackedStock(data.Stock);
        }
    }

    private void StyleMenu(string screen)
    {
        var menu = new Dictionary<Button, string> { [home] = "Overview", [catalog] = "Catalog", [builder] = "Builder",
            [orders] = "Orders", [inventory] = "Inventory", [users] = "Users",
            [reports] = "Reports", [tools] = "Store tools" };
        foreach (var item in menu)
        {
            bool selected = item.Value == screen;
            item.Key.BackColor = selected ? Color.FromArgb(222, 241, 212) : StoreTheme.Ink;
            item.Key.ForeColor = selected ? StoreTheme.Ink : Color.White;
            item.Key.FlatAppearance.BorderSize = 0;
            item.Key.FlatAppearance.MouseOverBackColor = selected ? Color.FromArgb(123, 195, 92) : Color.FromArgb(54, 68, 58);
            item.Key.AccessibleDescription = selected ? "Current page" : "Open " + item.Key.Text;
        }
    }

    private void ArrangeRoleMenu()
    {
        // Show only this role's screens, without leaving gaps in the sidebar.
        int nextTop = catalog.Top;
        int left = catalog.Left;
        int spacing = catalog.Height + 6;
        Button[] menu = { catalog, builder, orders, inventory, users, reports, tools };
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
