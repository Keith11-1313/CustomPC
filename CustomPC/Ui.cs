using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace CustomPC;

internal static class Ui
{
    public static bool IsDesign
    {
        get
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime;
        }
    }

    public static string Money(decimal amount)
    {
        return "PHP " + amount.ToString("N2", CultureInfo.InvariantCulture);
    }

    public static void ShowError(IWin32Window owner, Exception exception)
    {
        MessageBox.Show(owner, exception.Message, "Please check",
            MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    public static void Bind(DataGridView grid, object data)
    {
        grid.DataSource = data;

        // Id identifies the saved record, but does not need to appear on screen.
        DataGridViewColumn? idColumn = grid.Columns["Id"];
        if (idColumn != null)
        {
            idColumn.Visible = false;
        }

        grid.ClearSelection();
    }

    public static string? Selected(DataGridView grid)
    {
        if (grid.SelectedRows.Count == 0)
        {
            return null;
        }

        object? selectedId = grid.SelectedRows[0].Cells["Id"].Value;
        if (selectedId == null)
        {
            return null;
        }

        return selectedId.ToString();
    }

    public static string RequireSelection(DataGridView grid)
    {
        string? selectedId = Selected(grid);
        if (selectedId == null)
        {
            throw new InvalidOperationException("Select a row first.");
        }

        return selectedId;
    }

    public static bool Confirm(IWin32Window owner, string message)
    {
        DialogResult answer = MessageBox.Show(owner, message, "Confirm",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        return answer == DialogResult.Yes;
    }

    public static void Export(IWin32Window owner, DataGridView grid)
    {
        using (SaveFileDialog saveDialog = new SaveFileDialog())
        {
            saveDialog.Filter = "CSV report (*.csv)|*.csv";
            saveDialog.FileName = "CustomPC-report.csv";

            if (saveDialog.ShowDialog(owner) != DialogResult.OK)
            {
                return;
            }

            List<DataGridViewColumn> visibleColumns = new List<DataGridViewColumn>();
            List<string> headings = new List<string>();

            foreach (DataGridViewColumn column in grid.Columns)
            {
                if (column.Visible)
                {
                    visibleColumns.Add(column);
                    headings.Add(EscapeCsvValue(column.HeaderText));
                }
            }

            List<string> lines = new List<string>();
            lines.Add(string.Join(",", headings));

            foreach (DataGridViewRow row in grid.Rows)
            {
                if (row.IsNewRow)
                {
                    continue;
                }

                List<string> values = new List<string>();
                foreach (DataGridViewColumn column in visibleColumns)
                {
                    object? cellValue = row.Cells[column.Index].Value;
                    values.Add(EscapeCsvValue(cellValue));
                }

                lines.Add(string.Join(",", values));
            }

            File.WriteAllLines(saveDialog.FileName, lines, new UTF8Encoding(true));
            MessageBox.Show(owner, "Report exported.", "CustomPC");
        }
    }

    private static string EscapeCsvValue(object? value)
    {
        string text = Convert.ToString(value, CultureInfo.InvariantCulture) ?? "";

        // Prevent spreadsheet programs from treating exported text as a formula.
        if (text.Length > 0 && "=+-@".Contains(text[0]))
        {
            text = "'" + text;
        }

        // CSV uses two quotes to represent a quote inside a value.
        string escapedText = text.Replace("\"", "\"\"");
        return "\"" + escapedText + "\"";
    }

    public static string Receipt(CustomerOrder order, AppStore store)
    {
        string paymentStatus = "Unpaid";
        foreach (Payment payment in store.Payments)
        {
            if (payment.OrderId == order.Id)
            {
                paymentStatus = "Confirmed at store";
                break;
            }
        }

        string headingSeparator = new string('=', 62);
        string sectionSeparator = new string('-', 62);
        string createdDate = order.CreatedUtc.ToLocalTime().ToString("MMM dd, yyyy hh:mm tt");
        string paymentDeadline = order.DeadlineUtc.ToLocalTime().ToString("MMM dd, yyyy hh:mm tt");

        StringBuilder receipt = new StringBuilder();
        receipt.AppendLine("CUSTOMPC");
        receipt.AppendLine("PC BUILD ORDER RECEIPT");
        receipt.AppendLine(headingSeparator);
        receipt.AppendLine("Order: " + order.Number);
        receipt.AppendLine("Customer: " + order.CustomerName);
        receipt.AppendLine("Created: " + createdDate);
        receipt.AppendLine("Status: " + order.Status);
        receipt.AppendLine("Payment: " + paymentStatus);
        receipt.AppendLine("Pay before: " + paymentDeadline);
        receipt.AppendLine(sectionSeparator);

        foreach (OrderLine line in order.Lines)
        {
            decimal lineTotal = line.Quantity * line.Price;
            receipt.AppendLine(line.Name);
            receipt.AppendLine($"  {line.Quantity} x {Money(line.Price)} = {Money(lineTotal)}");
        }

        if (order.Lines.Count == 0)
        {
            receipt.AppendLine();
        }

        receipt.AppendLine(sectionSeparator);
        receipt.AppendLine("TOTAL: " + Money(order.Total));
        receipt.AppendLine();
        receipt.AppendLine("Present this receipt and pay at the physical store within 24 hours.");
        receipt.AppendLine("Unpaid orders are cancelled when their deadline expires.");
        receipt.AppendLine("Pickup only. No online payment or delivery.");
        receipt.AppendLine();
        receipt.AppendLine("Compatibility covers stored socket, memory and power rules only.");
        receipt.Append("Staff must verify physical dimensions and clearance before assembly.");

        return receipt.ToString();
    }
}
