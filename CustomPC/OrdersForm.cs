using System.Data;

namespace CustomPC;

public partial class OrdersForm : StoreWindow
{
    private OrderViewFilter? viewFilter;
    public OrdersForm(OrderViewFilter filter) : this() { viewFilter = filter; LoadRows(); }
    private readonly System.Windows.Forms.Timer refreshTimer = new System.Windows.Forms.Timer
    {
        Interval = 30000
    };

    public OrdersForm()
    {
        InitializeComponent();
        back.Click += BackButton_Click;

        if (Ui.IsDesign)
        {
            return;
        }

        var workspace = new WorkspacePage { Name = "workspace" };
        workspace.Bind(this, heading, back, "Track orders, payment status and pickup progress.", grid,
            refresh, new[] { receipt, pay, advance, cancel }, (searchLabel, search), (statusLabel, status));
        workspace.AddNotice(filterCaption);
        workspace.AddNotice(clearDashboardFilter);

        AppStore.Current.Refresh();
        if (!AppStore.Current.CanViewAllOrders && !AppStore.Current.IsCustomer)
            throw new InvalidOperationException("Sign in with an authorized account to view orders.");

        status.Items.Add("All statuses");
        status.Items.AddRange(AppStore.Statuses);
        status.SelectedIndex = 0;

        search.TextChanged += FilterChanged;
        status.SelectedIndexChanged += FilterChanged;
        refresh.Visible = false;
        _ = new AutomaticViewRefresh(this, () => RefreshButton_Click(this, EventArgs.Empty));
        emptyOrders.ActionClicked += EmptyOrders_Click;
        clearDashboardFilter.Click += (_, _) => { viewFilter = null; LoadRows(); };
        receipt.Click += ReceiptButton_Click;
        pay.Click += PayButton_Click;
        advance.Click += AdvanceButton_Click;
        cancel.Click += CancelButton_Click;
        grid.SelectionChanged += OrderSelectionChanged;
        Shown += OrdersForm_Shown;
        refreshTimer.Tick += RefreshTimer_Tick;
        FormClosed += OrdersForm_FormClosed;
        Disposed += (_, _) => refreshTimer.Dispose();

        refreshTimer.Start();
        LoadRows();
    }

    private void BackButton_Click(object? sender, EventArgs e)
    {
        // Closing this screen returns to the window that opened it.
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private void FilterChanged(object? sender, EventArgs e)
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

    private void RefreshButton_Click(object? sender, EventArgs e)
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

    private void RefreshTimer_Tick(object? sender, EventArgs e)
    {
        if (!Visible || Form.ActiveForm is { Modal: true }) return;
        try
        {
            LoadRows();
        }
        catch (Exception exception)
        {
            Ui.ShowError(this, exception);
        }
    }

    private void OrdersForm_FormClosed(object? sender, FormClosedEventArgs e)
    {
        refreshTimer.Dispose();
    }

    private void ReceiptButton_Click(object? sender, EventArgs e)
    {
        try
        {
            string orderId = Ui.RequireSelection(grid);
            using (ReceiptForm receiptForm = new ReceiptForm(orderId))
            {
                receiptForm.ShowDialog(this);
            }
        }
        catch (Exception exception)
        {
            Ui.ShowError(this, exception);
        }
    }

    private void PayButton_Click(object? sender, EventArgs e)
    {
        try
        {
            string orderId = Ui.RequireSelection(grid);
            bool confirmed = Ui.Confirm(this,
                "Confirm the customer paid the full amount at the physical store?");

            if (confirmed)
            {
                AppStore.Current.ConfirmPayment(orderId);
                LoadRows();
            }
        }
        catch (Exception exception)
        {
            Ui.ShowError(this, exception);
        }
    }

    private void AdvanceButton_Click(object? sender, EventArgs e)
    {
        try
        {
            string orderId = Ui.RequireSelection(grid);
            bool confirmed = Ui.Confirm(this,
                "Move this order to the next stage? Completion records pickup and deducts stock.");

            if (confirmed)
            {
                AppStore.Current.AdvanceOrder(orderId);
                LoadRows();
            }
        }
        catch (Exception exception)
        {
            Ui.ShowError(this, exception);
        }
    }

    private void CancelButton_Click(object? sender, EventArgs e)
    {
        try
        {
            string orderId = Ui.RequireSelection(grid);
            bool confirmed = Ui.Confirm(this,
                "Cancel this unpaid order and release its reserved parts?");

            if (confirmed)
            {
                AppStore.Current.CancelOrder(orderId);
                LoadRows();
            }
        }
        catch (Exception exception)
        {
            Ui.ShowError(this, exception);
        }
    }

    private void OrdersGrid_SizeChanged(object? sender, EventArgs e)
    {
        if (emptyOrders == null) return;
        int top = grid.ColumnHeadersVisible ? grid.ColumnHeadersHeight : 0;
        emptyOrders.SetBounds(0, top, grid.ClientSize.Width, Math.Max(0, grid.ClientSize.Height - top));
    }

    private void LoadRows()
    {
        string? selectedOrderId = Ui.Selected(grid);
        AppStore store = AppStore.Current;
        store.Refresh();
        // A role change in another running copy must also update these buttons.
        pay.Visible = store.IsStaff;
        advance.Visible = store.IsStaff;
        cancel.Visible = store.IsCustomer;
        heading.Text = store.IsAdmin ? "Order monitoring" : store.IsStaff ? "Orders & payments" : "My orders";
        Text = "CustomPC | " + heading.Text;
        string searchText = search.Text.Trim();
        filterCaption.Visible = viewFilter != null;
        clearDashboardFilter.Visible = viewFilter != null;
        filterCaption.Text = viewFilter?.Description ?? "";

        DataTable table = new DataTable();
        table.Columns.Add("Id", typeof(string));
        table.Columns.Add("Order", typeof(string));
        if (store.CanViewAllOrders) table.Columns.Add("Customer", typeof(string));
        searchLabel.Text = store.CanViewAllOrders ? "Order number or customer" : "Order number";
        table.Columns.Add("Total", typeof(string));
        table.Columns.Add("Status", typeof(string));
        table.Columns.Add("Payment", typeof(string));
        table.Columns.Add("PayBefore", typeof(string));

        // VisibleOrders already limits customers to their own orders.
        foreach (CustomerOrder order in store.VisibleOrders())
        {
            if (viewFilter != null && !viewFilter.Matches(order, store)) continue;
            if (status.SelectedIndex != 0 && order.Status != status.Text)
            {
                continue;
            }

            string searchableDetails = $"{order.Number} {order.CustomerName}";
            if (!searchableDetails.Contains(searchText, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string paymentStatus = "Unpaid";
            foreach (Payment payment in store.Payments)
            {
                if (payment.OrderId == order.Id)
                {
                    paymentStatus = "Paid";
                    break;
                }
            }

            string paymentDeadline = order.DeadlineUtc.ToLocalTime().ToString("MMM dd, hh:mm tt");
            if (store.CanViewAllOrders)
                table.Rows.Add(order.Id, order.Number, order.CustomerName,
                    Ui.Money(order.Total), order.Status, paymentStatus, paymentDeadline);
            else
                table.Rows.Add(order.Id, order.Number,
                    Ui.Money(order.Total), order.Status, paymentStatus, paymentDeadline);
        }

        Ui.Bind(grid, table);
        emptyOrders.Visible = table.Rows.Count == 0;
        OrdersGrid_SizeChanged(this, EventArgs.Empty);
        grid.Invalidate();
        if (table.Rows.Count == 0)
        {
            bool firstBuild = store.IsCustomer && store.VisibleOrders().Count == 0;
            emptyOrders.Message = firstBuild ? "Your first build starts here" : store.IsStaff ? "No orders need this action right now" : "No orders match this view";
            emptyOrders.Detail = firstBuild ? "Choose your parts and save your build with an order." : "Try another status or view all orders.";
            emptyOrders.ActionText = firstBuild ? "Open PC builder" : "View all orders";
            emptyOrders.BringToFront();
        }
        // Keep the same order selected after payment/progress, if it still matches the filters.
        foreach (DataGridViewRow row in grid.Rows)
        {
            if (row.Cells["Id"].Value?.ToString() == selectedOrderId)
            {
                grid.CurrentCell = row.Cells["Order"];
                row.Selected = true;
                break;
            }
        }
        UpdateOrderActions();
    }

    private void EmptyOrders_Click(object? sender, EventArgs e)
    {
        AppStore store = AppStore.Current;
        store.Refresh();
        if (store.IsCustomer && store.VisibleOrders().Count == 0)
        {
            if (Parent?.FindForm() is DashboardForm dashboard) dashboard.OpenScreen("Builder");
            else { using BuilderForm builder = new(); builder.ShowDialog(this); }
            return;
        }
        viewFilter = null;
        search.Clear(); status.SelectedIndex = 0; LoadRows();
    }

    private void OrderSelectionChanged(object? sender, EventArgs e)
    {
        UpdateOrderActions();
    }

    private void OrdersForm_Shown(object? sender, EventArgs e)
    {
        // WinForms may select its first bound row while showing the grid. Require an explicit choice.
        grid.ClearSelection();
        UpdateOrderActions();
    }

    private void UpdateOrderActions()
    {
        AppStore store = AppStore.Current;
        CustomerOrder? selectedOrder = null;
        string? selectedId = grid.Columns.Contains("Id") ? Ui.Selected(grid) : null;
        foreach (CustomerOrder order in store.VisibleOrders())
        {
            if (order.Id == selectedId)
            {
                selectedOrder = order;
                break;
            }
        }

        // Permission decides who sees an action; selection and status decide when it works.
        bool pending = selectedOrder != null && selectedOrder.Status == "Pending Payment"
            && selectedOrder.DeadlineUtc > DateTime.UtcNow;
        bool inProgress = selectedOrder != null && (selectedOrder.Status == "Paid"
            || selectedOrder.Status == "Processing" || selectedOrder.Status == "Assembly"
            || selectedOrder.Status == "Ready for Pickup");

        receipt.Enabled = selectedOrder != null;
        pay.Enabled = store.IsStaff && pending;
        advance.Enabled = store.IsStaff && inProgress;
        cancel.Enabled = store.IsCustomer && pending;

        // Muted buttons show that this order cannot use the action yet.
        Color inactive = Color.FromArgb(226, 232, 224);
        Color green = Color.FromArgb(84, 177, 55);
        pay.BackColor = pay.Enabled ? green : inactive;
        advance.BackColor = advance.Enabled ? green : inactive;
        cancel.BackColor = cancel.Enabled ? Color.FromArgb(180, 35, 56) : inactive;
    }
}
