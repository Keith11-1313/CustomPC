using System.Data;

namespace CustomPC;

public partial class ReportsForm : Form
{
    public ReportsForm()
    {
        InitializeComponent();
        if (Ui.IsDesign)
        {
            return;
        }

        AppStore store = AppStore.Current;
        if (!store.IsStaff)
        {
            throw new InvalidOperationException("Staff access required.");
        }

        report.Items.AddRange(new object[] { "Inventory", "Orders", "Stock movements" });
        if (store.IsAdmin)
        {
            report.Items.Add("Sales");
        }

        from.Value = DateTime.Today.AddDays(-30);
        to.Value = DateTime.Today;
        report.SelectedIndex = 0;
        report.SelectedIndexChanged += ReportChanged;
        refresh.Click += RefreshClicked;
        export.Click += ExportClicked;
        LoadReport();
    }

    public ReportsForm(string kind) : this()
    {
        report.SelectedItem = kind;
        LoadReport();
    }

    private void ReportChanged(object? sender, EventArgs e)
    {
        try
        {
            LoadReport();
        }
        catch (Exception exception)
        {
            Ui.ShowError(this, exception);
        }
    }

    private void RefreshClicked(object? sender, EventArgs e)
    {
        try
        {
            LoadReport();
        }
        catch (Exception exception)
        {
            Ui.ShowError(this, exception);
        }
    }

    private void ExportClicked(object? sender, EventArgs e)
    {
        try
        {
            Ui.Export(this, grid);
        }
        catch (Exception exception)
        {
            Ui.ShowError(this, exception);
        }
    }

    private void LoadReport()
    {
        AppStore store = AppStore.Current;
        store.Refresh();
        if (from.Value.Date > to.Value.Date)
        {
            throw new InvalidOperationException("Start date must not be after end date.");
        }

        from.Enabled = report.Text != "Inventory";
        to.Enabled = report.Text != "Inventory";

        switch (report.Text)
        {
            case "Inventory":
                LoadInventoryReport(store);
                break;
            case "Orders":
                LoadOrdersReport(store);
                break;
            case "Sales":
                LoadSalesReport(store);
                break;
            default:
                LoadStockMovementsReport(store);
                break;
        }
    }

    private bool IsWithinDateRange(DateTime utcDate)
    {
        DateTime localDate = utcDate.ToLocalTime().Date;
        return localDate >= from.Value.Date && localDate <= to.Value.Date;
    }

    private void LoadInventoryReport(AppStore store)
    {
        DataTable table = new DataTable();
        table.Columns.Add("Id");
        table.Columns.Add("Part");
        table.Columns.Add("Category");
        table.Columns.Add("OnHand", typeof(int));
        table.Columns.Add("Reserved", typeof(int));
        table.Columns.Add("Available", typeof(int));
        table.Columns.Add("Status");
        table.Columns.Add("Price");

        int partCount = 0;
        int availableUnits = 0;
        int reservedUnits = 0;
        foreach (Part part in store.Parts)
        {
            if (part.Archived)
            {
                continue;
            }

            int reservedStock = store.Reserved(part.Id);
            int availableStock = store.Available(part);
            table.Rows.Add(part.Id, part.Name, part.Category, part.Stock, reservedStock,
                availableStock, store.StockState(part), Ui.Money(part.Price));
            partCount++;
            availableUnits += availableStock;
            reservedUnits += reservedStock;
        }

        Ui.Bind(grid, table);
        summary.Text = $"Current inventory: {partCount} parts / {availableUnits} available units / {reservedUnits} reserved units";
    }

    private void LoadOrdersReport(AppStore store)
    {
        DataTable table = new DataTable();
        table.Columns.Add("Id");
        table.Columns.Add("Order");
        table.Columns.Add("Customer");
        table.Columns.Add("Status");
        table.Columns.Add("Total");
        table.Columns.Add("Created");

        int orderCount = 0;
        int cancelledCount = 0;
        decimal orderValue = 0;
        foreach (CustomerOrder order in store.Orders)
        {
            if (!IsWithinDateRange(order.CreatedUtc))
            {
                continue;
            }

            table.Rows.Add(order.Id, order.Number, order.CustomerName, order.Status,
                Ui.Money(order.Total), order.CreatedUtc.ToLocalTime().ToString("g"));
            orderCount++;
            orderValue += order.Total;
            if (order.Status == "Cancelled")
            {
                cancelledCount++;
            }
        }

        Ui.Bind(grid, table);
        summary.Text = $"{orderCount} orders / {cancelledCount} cancelled / order value {Ui.Money(orderValue)}";
    }

    private void LoadSalesReport(AppStore store)
    {
        if (!store.IsAdmin)
        {
            throw new InvalidOperationException("Sales reports require admin access.");
        }

        DataTable table = new DataTable();
        table.Columns.Add("Id");
        table.Columns.Add("Order");
        table.Columns.Add("Paid");
        table.Columns.Add("Amount");
        table.Columns.Add("ConfirmedBy");

        int paymentCount = 0;
        decimal salesTotal = 0;
        foreach (Payment payment in store.Payments)
        {
            if (!IsWithinDateRange(payment.PaidUtc))
            {
                continue;
            }

            CustomerOrder? paidOrder = null;
            foreach (CustomerOrder order in store.Orders)
            {
                if (order.Id == payment.OrderId)
                {
                    paidOrder = order;
                    break;
                }
            }

            if (paidOrder == null)
            {
                throw new InvalidOperationException("A payment's order could not be found.");
            }

            string staffName = GetUserName(store, payment.StaffId);
            table.Rows.Add(payment.Id, paidOrder.Number, payment.PaidUtc.ToLocalTime().ToString("g"),
                Ui.Money(payment.Amount), staffName);
            paymentCount++;
            salesTotal += payment.Amount;
        }

        Ui.Bind(grid, table);
        summary.Text = $"{paymentCount} confirmed store payments / sales {Ui.Money(salesTotal)}";
    }

    private void LoadStockMovementsReport(AppStore store)
    {
        DataTable table = new DataTable();
        table.Columns.Add("Id");
        table.Columns.Add("Part");
        table.Columns.Add("Quantity", typeof(int));
        table.Columns.Add("Reason");
        table.Columns.Add("RecordedBy");
        table.Columns.Add("Date");

        int movementCount = 0;
        int netQuantity = 0;
        List<InventoryMovement> sortedMovements = new List<InventoryMovement>(store.Movements);
        sortedMovements.Sort(CompareMovementsByDate);
        foreach (InventoryMovement movement in sortedMovements)
        {
            if (!IsWithinDateRange(movement.CreatedUtc))
            {
                continue;
            }

            string partName = "";
            foreach (Part part in store.Parts)
            {
                if (part.Id == movement.PartId)
                {
                    partName = part.Name;
                    break;
                }
            }

            string userName = GetUserName(store, movement.UserId);
            table.Rows.Add(movement.Id, partName, movement.Quantity, movement.Reason,
                userName, movement.CreatedUtc.ToLocalTime().ToString("g"));
            movementCount++;
            netQuantity += movement.Quantity;
        }

        Ui.Bind(grid, table);
        summary.Text = $"{movementCount} inventory adjustments / net quantity {netQuantity}";
    }

    private int CompareMovementsByDate(InventoryMovement first, InventoryMovement second)
    {
        int dateComparison = second.CreatedUtc.CompareTo(first.CreatedUtc);
        if (dateComparison != 0)
        {
            return dateComparison;
        }

        // Keep the original order when two entries have the same timestamp.
        List<InventoryMovement> originalMovements = AppStore.Current.Movements;
        int firstPosition = originalMovements.IndexOf(first);
        int secondPosition = originalMovements.IndexOf(second);
        return firstPosition.CompareTo(secondPosition);
    }

    private string GetUserName(AppStore store, string userId)
    {
        foreach (UserAccount user in store.Users)
        {
            if (user.Id == userId)
            {
                return user.Name;
            }
        }

        return "";
    }
}
