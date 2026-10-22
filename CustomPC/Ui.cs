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
        string message = exception.Message;
        if (exception is not InvalidOperationException)
        {
            string reference = AppLog.Write(exception);
            message = "The operation could not be completed. Check available disk space and file access, then try again.\n\n" +
                "If the problem continues, contact support with reference " + reference + ". Your diagnostic log is in " + AppLog.Folder;
        }
        Message(owner, message, "Please check");
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
        StoreTheme.ApplyGrid(grid);

        grid.ClearSelection();
        grid.Paint -= EmptyGrid_Paint;
        grid.Paint += EmptyGrid_Paint;
        grid.CellFormatting -= StatusCell_Formatting;
        grid.CellFormatting += StatusCell_Formatting;
        grid.CellPainting -= CheckboxCell_Paint;
        grid.CellPainting += CheckboxCell_Paint;
        grid.CellPainting -= StatusCell_Paint;
        grid.CellPainting += StatusCell_Paint;
    }

    private static void StatusCell_Paint(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (sender is not DataGridView grid || e.RowIndex < 0 || e.ColumnIndex < 0 || e.Graphics == null) return;
        if (grid.Columns[e.ColumnIndex].Name is not ("Status" or "Availability" or "StockState")) return;
        string text = e.FormattedValue?.ToString() ?? "";
        if (text.Length == 0) return;
        e.PaintBackground(e.ClipBounds, true);
        Color ink = text is "Cancelled" or "Out of stock" ? Color.FromArgb(157, 48, 54) :
            text is "Pending Payment" or "Low stock" ? Color.FromArgb(132, 83, 18) : StoreTheme.Action;
        Color fill = text is "Cancelled" or "Out of stock" ? Color.FromArgb(253, 237, 238) :
            text is "Pending Payment" or "Low stock" ? Color.FromArgb(252, 244, 223) : StoreTheme.Selection;
        int width = Math.Min(e.CellBounds.Width - 16, TextRenderer.MeasureText(text, grid.Font).Width + 22);
        Rectangle badge = new(e.CellBounds.Left + 8, e.CellBounds.Top + (e.CellBounds.Height - 28) / 2, Math.Max(1, width), 28);
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        using var shape = StoreTheme.RoundedRectangle(badge, 7);
        using SolidBrush brush = new(fill);
        e.Graphics.FillPath(brush, shape);
        TextRenderer.DrawText(e.Graphics, text, grid.Font, Rectangle.Inflate(badge, -8, 0), ink,
            TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        if ((e.State & DataGridViewElementStates.Selected) != 0 && grid.Focused)
            ControlPaint.DrawFocusRectangle(e.Graphics, Rectangle.Inflate(e.CellBounds, -2, -2));
        e.Handled = true;
    }

    private static void CheckboxCell_Paint(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (sender is not DataGridView grid || e.RowIndex < 0 || e.ColumnIndex < 0 || e.Graphics == null ||
            grid.Columns[e.ColumnIndex] is not DataGridViewCheckBoxColumn) return;
        e.PaintBackground(e.ClipBounds, true);
        bool selected = e.Value is bool value && value;
        Rectangle box = new(e.CellBounds.Left + (e.CellBounds.Width - 18) / 2,
            e.CellBounds.Top + (e.CellBounds.Height - 18) / 2, 18, 18);
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        using var outline = StoreTheme.RoundedRectangle(box, 4);
        using SolidBrush fill = new(selected ? Color.FromArgb(61, 137, 39) : Color.White);
        using Pen border = new(Color.FromArgb(155, 171, 161), 1.4F);
        e.Graphics.FillPath(fill, outline); e.Graphics.DrawPath(border, outline);
        if (selected)
        {
            IconAssets.Paint(e.Graphics, "check", Rectangle.Inflate(box, -2, -2), Color.White);
        }
        e.Handled = true;
    }

    private static void EmptyGrid_Paint(object? sender, PaintEventArgs e)
    {
        if (sender is not DataGridView grid || grid.Rows.Count != 0) return;
        // A dedicated child empty state owns the message for this table.
        if (grid.Controls.OfType<EmptyStateControl>().Any(state => state.Visible)) return;
        Rectangle area = grid.ClientRectangle;
        area.Y += grid.ColumnHeadersHeight;
        area.Height -= grid.ColumnHeadersHeight;
        TextRenderer.DrawText(e.Graphics, "No records found.", grid.Font,
            area, Color.FromArgb(76, 92, 78), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
    }

    private static void StatusCell_Formatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (sender is not DataGridView grid || e.ColumnIndex < 0 || e.RowIndex < 0) return;
        string name = grid.Columns[e.ColumnIndex].Name;
        if (name != "Status" && name != "Availability" && name != "StockState") return;
        string value = e.Value?.ToString() ?? "";
        Color color = value switch
        {
            "Completed" or "Available" or "Paid" => Color.FromArgb(18, 103, 70),
            "Cancelled" or "Out of stock" => Color.FromArgb(170, 30, 50),
            "Pending Payment" or "Low stock" => Color.FromArgb(139, 82, 5),
            _ => Color.FromArgb(46, 123, 31)
        };
        e.CellStyle.ForeColor = color;
        e.CellStyle.SelectionForeColor = color;
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
        using StoreDialog dialog = new("Confirm action", message, true);
        return dialog.ShowDialog(owner) == DialogResult.Yes;
    }

    public static void Message(IWin32Window? owner, string message, string title)
    {
        using StoreDialog dialog = new(title, message);
        dialog.ShowDialog(owner);
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
            Message(owner, "Report exported.", "CustomPC");
        }
    }

    private static string EscapeCsvValue(object? value)
    {
        string text = Convert.ToString(value, CultureInfo.InvariantCulture) ?? "";

        // Prevent spreadsheet programs from treating exported text as a formula.
        string formulaCheck = text.TrimStart();
        if ((formulaCheck.Length > 0 && "=+-@".Contains(formulaCheck[0])) ||
            (text.Length > 0 && "\t\r\n".Contains(text[0])))
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
        receipt.AppendLine("CustomPC");

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
