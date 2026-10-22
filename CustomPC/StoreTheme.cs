using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;

namespace CustomPC;

// Central colours keep shopping pages and operational pages consistent.
public static class StoreTheme
{
    public static readonly Color Background = Color.FromArgb(247, 248, 250);
    public static readonly Color Surface = Color.White;
    public static readonly Color Ink = Color.FromArgb(27, 39, 37);
    public static readonly Color Muted = Color.FromArgb(100, 112, 111);
    public static readonly Color Green = Color.FromArgb(84, 177, 55);
    public static readonly Color Border = Color.FromArgb(222, 228, 223);
    public static readonly Color Action = Color.FromArgb(20, 119, 80);
    public static readonly Color Selection = Color.FromArgb(230, 243, 237);
    private static readonly Font body = AppFonts.Body(10.5F);
    private static readonly Font emphasis = AppFonts.Body(10.5F, FontStyle.Bold);
    private static readonly ConditionalWeakTable<Button, ButtonState> buttons = new();

    public static void Apply(Control control)
    {
        if (control is Form) control.BackColor = Background;
        // Display type belongs to headings. Working controls use the readable body face.
        if (control is Button or StoreTextField or ComboBox or NumericUpDown or CheckBox or DataGridView)
            control.Font = control is Button && control.Parent?.Name is not ("navigationList" or "sidebar") ? emphasis : body;
        if (control is Label label && label.Font.Size <= 10.5F)
        {
            label.Font = label.Font.Bold ? emphasis : body;
        }
        if (control is DataGridView grid) ApplyGrid(grid);
        if (control is ComboBox combo && control is not StoreComboBox) combo.FlatStyle = FlatStyle.Standard;
        if (control is Button button && button is not PasswordEyeButton && !buttons.TryGetValue(button, out _))
        {
            ButtonState state = new();
            buttons.Add(button, state);
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Cursor = Cursors.Hand;
            button.Paint += PaintButton;
            button.MouseEnter += (_, _) => { state.Hover = true; button.Invalidate(); };
            button.MouseLeave += (_, _) => { state.Hover = false; state.Pressed = false; button.Invalidate(); };
            button.MouseDown += (_, _) => { state.Pressed = true; button.Invalidate(); };
            button.MouseUp += (_, _) => { state.Pressed = false; button.Invalidate(); };
            button.GotFocus += (_, _) => button.Invalidate();
            button.LostFocus += (_, _) => button.Invalidate();
        }
        foreach (Control child in control.Controls) Apply(child);
    }

    // Reapply after binding so refreshed tables retain the same appearance.
    public static void ApplyGrid(DataGridView grid)
    {
        grid.BorderStyle = BorderStyle.None;
        grid.BackgroundColor = Surface;
        grid.EnableHeadersVisualStyles = false;
        grid.GridColor = Border;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        int scaleHeight = (int)(48 * grid.DeviceDpi / 96F);
        grid.ColumnHeadersHeight = scaleHeight;
        grid.RowTemplate.Height = scaleHeight;
        foreach (DataGridViewRow row in grid.Rows) row.Height = scaleHeight;
        grid.DefaultCellStyle.Font = body;
        grid.ColumnHeadersDefaultCellStyle.Font = emphasis;
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(247, 249, 248);
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Muted;
        grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(239, 243, 241);
        grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Muted;
        grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(12, 0, 8, 0);
        grid.DefaultCellStyle.Padding = new Padding(12, 4, 8, 4);
        grid.DefaultCellStyle.BackColor = Surface;
        grid.DefaultCellStyle.ForeColor = Ink;
        grid.DefaultCellStyle.SelectionBackColor = Selection;
        grid.DefaultCellStyle.SelectionForeColor = Ink;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Surface;
        grid.RowHeadersVisible = false;
        // Pause fill-size recalculation while applying minimum widths to new columns.
        DataGridViewAutoSizeColumnsMode sizing = grid.AutoSizeColumnsMode;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        foreach (DataGridViewColumn column in grid.Columns)
        {
            if (!column.Visible) continue;
            column.MinimumWidth = column.Name is "Price" or "Total" or "Amount" ? 140 : column.Name == "Status" ? 150 :
                column.Name == "Category" ? 125 : column.Name == "Order" ? 220 :
                column.Name is "PayBefore" or "DeadlineUtc" ? 170 :
                column.Name is "OnHand" or "Reserved" or "Available" ? 95 :
                column.Name is "Component" or "Part" ? 165 : column is DataGridViewCheckBoxColumn ? 85 : 80;
            column.HeaderText = column.Name switch { "OnHand" => "On hand", "CreatedUtc" => "Created", "PayBefore" or "DeadlineUtc" => "Pay before", _ => column.HeaderText };
            int headerWidth = TextRenderer.MeasureText(column.HeaderText,
                grid.ColumnHeadersDefaultCellStyle.Font ?? grid.Font).Width + 36;
            column.MinimumWidth = Math.Max(column.MinimumWidth, headerWidth);
            column.FillWeight = column.Name is "Component" or "Part" or "Customer" or "Details" or "Email" ? 240 :
                column.Name is "Status" or "Action" ? 150 : 90;
            if (column.Name is "Price" or "Total" or "Amount" or "OnHand" or "Reserved" or "Available")
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }
        grid.AutoSizeColumnsMode = sizing;
    }

    private static void PaintButton(object? sender, PaintEventArgs e)
    {
        if (sender is not Button button || !buttons.TryGetValue(button, out ButtonState? state)) return;
        bool dark = button.BackColor.GetBrightness() < 0.25F;
        bool primary = button.BackColor.G > button.BackColor.R + 20;
        bool navigation = button.Parent?.Name is "sidebar" or "navigationList";
        bool categoryFilter = button.Name == "categoryFilter";
        bool compactNavigation = navigation && button.Width < 80 * button.DeviceDpi / 96F;
        bool selected = navigation && button.AccessibleDescription == "Current page";
        Color fill = primary && !navigation ? Action : button.BackColor;
        if (categoryFilter) fill = button.AccessibleDescription == "Selected category" ? Selection : button.Parent?.BackColor ?? Background;
        if (!button.Enabled) fill = Color.FromArgb(236, 239, 236);
        else if (state.Pressed) fill = primary ? Color.FromArgb(14, 87, 58) : Color.FromArgb(224, 233, 229);
        else if (state.Hover) fill = dark ? Color.FromArgb(40, 57, 51) : primary ? Color.FromArgb(16, 103, 68) : Color.FromArgb(241, 245, 243);
        e.Graphics.Clear(button.Parent?.BackColor ?? Background);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        Rectangle bounds = new(1, 1, Math.Max(1, button.Width - 3), Math.Max(1, button.Height - 3));
        using GraphicsPath shape = RoundedRectangle(bounds, 8);
        using SolidBrush brush = new(fill);
        e.Graphics.FillPath(brush, shape);
        if (!primary && !dark && !navigation && !categoryFilter && button.Name != "details")
        {
            using Pen outline = new(Border);
            e.Graphics.DrawPath(outline, shape);
        }
        Color text = !button.Enabled ? Muted : (dark || primary && !navigation) ? Color.White : Ink;
        if (selected)
        {
            using Pen marker = new(Action, 3);
            e.Graphics.DrawLine(marker, 5, 12, 5, button.Height - 12);
        }
        string icon = button.Name switch { "back" => "back", "add" => "add", "refresh" => "refresh",
            "catalog" or "inventory" => "grid", "builder" => "build", "orders" or "tools" => "list",
            "users" => "user", "reports" or "home" => "chart", "signout" => "log-out", "adjust" => "plus", "pay" => "check", "advance" => "arrow-right", "save" => "check", "receipt" => "clipboard-list", "edit" => "pencil", "history" => "rotate-ccw", "export" => "download", "backup" => "download", "restore" => "rotate-ccw", _ => "" };
        if (button.Text == "Cancel") icon = "";
        if (button.Name == "details") text = Action;
        Rectangle textBounds = bounds;
        if (icon != "")
        {
            int textWidth = Math.Min(Math.Max(1, button.Width - 50), TextRenderer.MeasureText(button.Text, button.Font).Width);
            int iconLeft = compactNavigation ? (button.Width - 20) / 2 : navigation ? 14 : Math.Max(12, (button.Width - textWidth - 26) / 2);
            IconAssets.Paint(e.Graphics, icon, new Rectangle(iconLeft, (button.Height - 20) / 2, 20, 20), text);
            textBounds.X = iconLeft + 26; textBounds.Width = Math.Max(1, button.Width - textBounds.X - 12);
        }
        if (categoryFilter) { textBounds.X += 14; textBounds.Width -= 24; }
        TextRenderer.DrawText(e.Graphics, compactNavigation ? "" : button.Text, selected ? emphasis : button.Font, textBounds, text,
            (navigation || categoryFilter ? TextFormatFlags.Left : TextFormatFlags.HorizontalCenter) | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        if (button.Focused)
        {
            using var focus = RoundedRectangle(Rectangle.Inflate(bounds, -4, -4), 5);
            using Pen ring = new(primary || dark ? Color.White : Action, 1.5F);
            e.Graphics.DrawPath(ring, focus);
        }
    }

    internal static GraphicsPath RoundedRectangle(Rectangle rectangle, int radius)
    {
        int diameter = Math.Min(radius * 2, Math.Min(rectangle.Width, rectangle.Height));
        GraphicsPath path = new();
        path.AddArc(rectangle.Left, rectangle.Top, diameter, diameter, 180, 90);
        path.AddArc(rectangle.Right - diameter, rectangle.Top, diameter, diameter, 270, 90);
        path.AddArc(rectangle.Right - diameter, rectangle.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rectangle.Left, rectangle.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }

    private sealed class ButtonState { public bool Hover; public bool Pressed; }
}
