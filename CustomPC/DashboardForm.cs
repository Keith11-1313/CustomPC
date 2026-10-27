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
        SizeChanged += (_, _) => FitWorkspace();
        Shown += (_, _) => FitWorkspace();

        if (Ui.IsDesign)
        {
            return;
        }

        BuildAdaptiveShell();
        lowStockAction.Click += (_, _) => OpenScreen("Inventory", inventoryFilter: new InventoryViewFilter(StockState: "Low stock"));
        outOfStockAction.Click += (_, _) => OpenScreen("Inventory", inventoryFilter: new InventoryViewFilter(StockState: "Out of stock"));
        catalog.Click += CatalogButton_Click;
        home.Click += (_, _) => ShowOverview();
        pageError.ActionClicked += (_, _) => retryPage?.Invoke();
        emptyRecent.ActionClicked += (_, _) => OpenScreen(AppStore.Current.IsStaff ? "Orders" : "Builder");
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
        if (!Visible || Form.ActiveForm is { Modal: true }) return;
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
                    if (AppStore.Current.IsAdmin || AppStore.Current.IsStaff)
                        throw new InvalidOperationException("Use Parts & inventory to manage products.");
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
            form.AutoScroll = screenName is not "Catalog" and not "Builder";
            form.AutoScrollMinSize = Size.Empty;
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
        catalog.Enabled = !store.IsAdmin && !store.IsStaff;
        reports.Enabled = store.IsAdmin;

        users.Enabled = store.IsAdmin;

        tools.Enabled = store.IsAdmin;
        orders.Enabled = store.CanViewAllOrders || store.IsCustomer;
        builder.Enabled = store.IsCustomer || store.Session == null;
        builder.Text = store.Session == null ? "PC builder preview" : "PC builder";
        signout.Text = store.Session == null ? "Sign in" : "Sign out";
        orders.Text = store.IsAdmin ? "View orders" : store.IsStaff ? "Orders & payments" : "My orders";
        ArrangeRoleMenu();

        // Refresh still checks expiry and account permissions. Hosted pages do not need
        // an invisible overview (including its charts) rebuilt behind them.
        if (pageHost.Visible) { FitWorkspace(); return; }

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
        foreach (CustomerOrder order in store.VisibleOrders()
            .Where(o => !store.IsStaff || o.Status is not ("Completed" or "Cancelled"))
            .OrderByDescending(o => o.CreatedUtc).Take(6))
        {
            if (store.CanViewAllOrders) recentOrders.Rows.Add(order.Id, order.Number, order.CustomerName, order.Status);
            else recentOrders.Rows.Add(order.Id, order.Number, order.Status);
        }
        Ui.Bind(recent, recentOrders);
        bool operational = store.IsAdmin || store.IsStaff;
        chartArea.Visible = store.IsAdmin;
        period.Visible = operational;
        summarySurface.Visible = !operational;
        workflowSurface.Visible = !operational;
        recent.Visible = store.IsStaff || store.IsCustomer;
        recentTitle.Visible = store.IsStaff || store.IsCustomer;
        recentTitle.Text = store.IsStaff ? "Open orders" : "Your recent orders";
        emptyRecent.Message = store.IsStaff ? "No open orders right now" : "Your first build starts here";
        emptyRecent.Detail = store.IsStaff
            ? "Orders awaiting payment, preparation or pickup will appear here."
            : "Choose your components to create your first order.";
        emptyRecent.ActionText = store.IsStaff ? "View all orders" : "Open PC builder";
        emptyRecent.Visible = (store.IsCustomer || store.IsStaff) && recentOrders.Rows.Count == 0;
        if (emptyRecent.Visible) { recent.Visible = false; emptyRecent.BringToFront(); }
        statOrders.Visible = store.Session != null;
        statOrdersCaption.Visible = store.Session != null;
        if (operational) RefreshCharts();
        chartActions.Visible = store.IsStaff;
        adminSummary.Visible = stockAttention.Visible = operational;
        FitWorkspace();
        chartSummary.Visible = false;
        viewQueueOrders.Text = store.IsAdmin ? "View paid parts orders" : "View open orders";
        viewPeriodOrders.Text = store.IsStaff ? "View received orders" : "View period orders";
        viewStock.Text = store.IsStaff ? "View inventory" : "View current inventory";
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
        FitWorkspace();
    }

    private void DrawCharts()
    {
        AppStore store = AppStore.Current;
        int days = period.SelectedIndex == 0 ? 7 : period.SelectedIndex == 2 ? 90 : 30;
        chartArea.SuspendLayout();
        chartArea.ColumnStyles.Clear(); chartArea.RowStyles.Clear();
        chartArea.ColumnCount = 3;
        chartArea.RowCount = 1;
        for (int i = 0; i < 3; i++) chartArea.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / 3));
        chartArea.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        chartArea.SetColumnSpan(trendChart, 1);
        chartArea.SetCellPosition(trendChart, new TableLayoutPanelCellPosition(0, 0));
        chartArea.SetCellPosition(distributionChart, new TableLayoutPanelCellPosition(1, 0));
        chartArea.SetCellPosition(stockChart, new TableLayoutPanelCellPosition(2, 0));
        chartArea.ResumeLayout();
        if (store.IsAdmin)
        {
            var data = store.ReadAdminDashboard(days);
            adminSales.Text = $"PHP {data.Sales.Sum(v => v.Value):N2}\nConfirmed payments";
            int paidOrders = store.Payments.Where(p => p.PaidUtc.ToLocalTime().Date >= data.FirstDate && p.PaidUtc.ToLocalTime().Date <= data.LastDate).Select(p => p.OrderId).Distinct().Count();
            adminPaidOrders.Text = $"{paidOrders}\nPaid orders";
            int low = (int)data.Inventory.First(v => v.Name == "Low stock").Value;
            int unavailable = (int)data.Inventory.First(v => v.Name == "Out of stock").Value;
            adminLowStock.Text = $"{low}\nLow-stock products";
            adminOutOfStock.Text = $"{unavailable}\nOut-of-stock products";
            stockAttentionText.Text = low + unavailable == 0 ? "No low-stock or out-of-stock products." : $"Products needing stock: {low + unavailable:N0}";
            lowStockAction.Text = $"Low stock ({low})";
            outOfStockAction.Text = $"Out of stock ({unavailable})";
            chartSummary.Text = "";
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
            adminSales.Text = $"{data.Incoming.Sum(v => v.Value):N0}\nOrders received";
            adminPaidOrders.Text = $"{data.Queue.Sum(v => v.Value):N0}\nOpen orders";
            adminLowStock.Text = $"{data.Queue.Where(v => v.Name == "Ready for Pickup").Sum(v => v.Value):N0}\nReady for pickup";
            int stockAlerts = data.Stock.Sum(v => v.LowStock + v.OutOfStock);
            adminOutOfStock.Text = $"{stockAlerts}\nProducts needing stock";
            int low = data.Stock.Sum(v => v.LowStock), unavailable = data.Stock.Sum(v => v.OutOfStock);
            stockAttentionText.Text = stockAlerts == 0 ? "No low-stock or out-of-stock products." : $"Products needing stock: {stockAlerts:N0}";
            lowStockAction.Text = $"Low stock ({low})";
            outOfStockAction.Text = $"Out of stock ({unavailable})";
            chartSummary.Text = "";
            // Staff use operational counts and stock links without loading chart controls.
            trendChart.ClearRuntimeData();
            distributionChart.ClearRuntimeData();
            stockChart.ClearRuntimeData();
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
            item.Key.BackColor = selected ? StoreTheme.Selection : StoreTheme.Ink;
            item.Key.ForeColor = selected ? StoreTheme.Ink : Color.White;
            item.Key.FlatAppearance.BorderSize = 0;
            item.Key.FlatAppearance.MouseOverBackColor = selected ? Color.FromArgb(123, 195, 92) : Color.FromArgb(54, 68, 58);
            item.Key.AccessibleDescription = selected ? "Current page" : "Open " + item.Key.Text;
        }
    }

    private void ArrangeRoleMenu()
    {
        // The menu is inside navigationList now; sidebar coordinates add a
        // second header offset and leave the menu floating halfway down.
        LayoutWorkspace();
    }

    private void FitWorkspace() => LayoutWorkspace();
}
