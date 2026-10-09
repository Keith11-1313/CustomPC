using System.Data;

namespace CustomPC;

public partial class OrdersForm : Form
{
    private readonly System.Windows.Forms.Timer refreshTimer = new System.Windows.Forms.Timer
    {
        Interval = 30000
    };

    public OrdersForm()
    {
        InitializeComponent();

        if (Ui.IsDesign)
        {
            return;
        }

        pay.Enabled = AppStore.Current.IsStaff;
        advance.Enabled = AppStore.Current.IsStaff;
        status.Items.Add("All statuses");
        status.Items.AddRange(AppStore.Statuses);
        status.SelectedIndex = 0;

        search.TextChanged += FilterChanged;
        status.SelectedIndexChanged += FilterChanged;
        refresh.Click += RefreshButton_Click;
        receipt.Click += ReceiptButton_Click;
        pay.Click += PayButton_Click;
        advance.Click += AdvanceButton_Click;
        cancel.Click += CancelButton_Click;
        refreshTimer.Tick += RefreshTimer_Tick;
        FormClosed += OrdersForm_FormClosed;

        refreshTimer.Start();
        LoadRows();
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

    private void LoadRows()
    {
        AppStore store = AppStore.Current;
        store.Refresh();
        string searchText = search.Text.Trim();

        DataTable table = new DataTable();
        table.Columns.Add("Id", typeof(string));
        table.Columns.Add("Order", typeof(string));
        table.Columns.Add("Customer", typeof(string));
        table.Columns.Add("Total", typeof(string));
        table.Columns.Add("Status", typeof(string));
        table.Columns.Add("Payment", typeof(string));
        table.Columns.Add("PayBefore", typeof(string));

        // VisibleOrders already limits customers to their own orders.
        foreach (CustomerOrder order in store.VisibleOrders())
        {
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
            table.Rows.Add(order.Id, order.Number, order.CustomerName,
                Ui.Money(order.Total), order.Status, paymentStatus, paymentDeadline);
        }

        Ui.Bind(grid, table);
    }
}
