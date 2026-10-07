using System.ComponentModel;
using System.Data;
using System.Globalization;

namespace CustomPC;

internal static class Ui
{
    public static bool IsDesign => LicenseManager.UsageMode == LicenseUsageMode.Designtime;
    public static string Money(decimal value) => "PHP " + value.ToString("N2", CultureInfo.InvariantCulture);
    public static void Run(IWin32Window owner, Action action)
    {
        try { action(); }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or Microsoft.Data.Sqlite.SqliteException or IOException)
        { MessageBox.Show(owner, ex.Message, "Please check", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }
    public static void Bind(DataGridView grid, object data)
    {
        grid.DataSource = data;
        if (grid.Columns["Id"] is DataGridViewColumn idColumn) idColumn.Visible = false;
        grid.ClearSelection();
    }
    public static string? Selected(DataGridView grid) => grid.SelectedRows.Count == 0 ? null : grid.SelectedRows[0].Cells["Id"].Value?.ToString();
    public static string RequireSelection(DataGridView grid) => Selected(grid) ?? throw new InvalidOperationException("Select a row first.");
    public static bool Confirm(IWin32Window owner, string message) => MessageBox.Show(owner, message, "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
    public static void Export(IWin32Window owner, DataGridView grid)
    {
        using var dialog = new SaveFileDialog { Filter = "CSV report (*.csv)|*.csv", FileName = "CustomPC-report.csv" };
        if (dialog.ShowDialog(owner) != DialogResult.OK) return;
        static string Escape(object? value)
        {
            var s = Convert.ToString(value, CultureInfo.InvariantCulture) ?? "";
            if (s.Length > 0 && "=+-@".Contains(s[0])) s = "'" + s;
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        }
        var columns = grid.Columns.Cast<DataGridViewColumn>().Where(x => x.Visible).ToList();
        var lines = new List<string> { string.Join(",", columns.Select(x => Escape(x.HeaderText))) };
        lines.AddRange(grid.Rows.Cast<DataGridViewRow>().Where(x => !x.IsNewRow).Select(r => string.Join(",", columns.Select(c => Escape(r.Cells[c.Index].Value)))));
        File.WriteAllLines(dialog.FileName, lines, new System.Text.UTF8Encoding(true));
        MessageBox.Show(owner, "Report exported.", "CustomPC");
    }
    public static string Receipt(CustomerOrder order, AppStore store)
    {
        var paid = store.Payments.FirstOrDefault(x => x.OrderId == order.Id);
        return $"CUSTOMPC\r\nPC BUILD ORDER RECEIPT\r\n{new string('=', 62)}\r\nOrder: {order.Number}\r\nCustomer: {order.CustomerName}\r\nCreated: {order.CreatedUtc.ToLocalTime():MMM dd, yyyy hh:mm tt}\r\nStatus: {order.Status}\r\nPayment: {(paid == null ? "Unpaid" : "Confirmed at store")}\r\nPay before: {order.DeadlineUtc.ToLocalTime():MMM dd, yyyy hh:mm tt}\r\n{new string('-', 62)}\r\n" +
            string.Join("\r\n", order.Lines.Select(x => $"{x.Name}\r\n  {x.Quantity} x {Money(x.Price)} = {Money(x.Quantity * x.Price)}")) +
            $"\r\n{new string('-', 62)}\r\nTOTAL: {Money(order.Total)}\r\n\r\nPresent this receipt and pay at the physical store within 24 hours.\r\nUnpaid orders are cancelled when their deadline expires.\r\nPickup only. No online payment or delivery.\r\n\r\nCompatibility covers stored socket, memory and power rules only.\r\nStaff must verify physical dimensions and clearance before assembly.";
    }
}
