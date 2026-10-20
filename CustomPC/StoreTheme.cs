using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;

namespace CustomPC;

// Central colours keep shopping pages and operational pages consistent.
public static class StoreTheme
{
    public static readonly Color Background = Color.FromArgb(243, 245, 247);
    public static readonly Color Surface = Color.White;
    public static readonly Color Ink = Color.FromArgb(32, 41, 35);
    public static readonly Color Muted = Color.FromArgb(92, 105, 98);
    public static readonly Color Green = Color.FromArgb(84, 177, 55);
    public static readonly Color Border = Color.FromArgb(222, 228, 223);
    private static readonly ConditionalWeakTable<Button, ButtonState> buttons = new();

    public static void Apply(Control control)
    {
        if (control is Form) control.BackColor = Background;
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
        grid.ColumnHeadersHeight = 44;
        grid.RowTemplate.Height = 46;
        foreach (DataGridViewRow row in grid.Rows) row.Height = 46;
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(239, 243, 241);
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Muted;
        grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(239, 243, 241);
        grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Muted;
        grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(12, 0, 8, 0);
        grid.DefaultCellStyle.Padding = new Padding(12, 4, 8, 4);
        grid.DefaultCellStyle.BackColor = Surface;
        grid.DefaultCellStyle.ForeColor = Ink;
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(229, 243, 222);
        grid.DefaultCellStyle.SelectionForeColor = Ink;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 252, 251);
        grid.RowHeadersVisible = false;
        // Pause fill-size recalculation while applying minimum widths to new columns.
        DataGridViewAutoSizeColumnsMode sizing = grid.AutoSizeColumnsMode;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        foreach (DataGridViewColumn column in grid.Columns)
        {
            if (!column.Visible) continue;
            column.MinimumWidth = column.Name == "Price" ? 128 : column.Name == "Status" ? 110 :
                column.Name is "OnHand" or "Reserved" or "Available" ? 95 :
                column.Name is "Component" or "Part" ? 165 : column is DataGridViewCheckBoxColumn ? 85 : 80;
            column.HeaderText = column.Name switch { "OnHand" => "On hand", "CreatedUtc" => "Created", "DeadlineUtc" => "Pay before", _ => column.HeaderText };
            column.FillWeight = column.Name is "Component" or "Part" or "Customer" or "Details" or "Email" ? 240 :
                column.Name is "Status" or "Action" ? 150 : 90;
        }
        grid.AutoSizeColumnsMode = sizing;
    }

    private static void PaintButton(object? sender, PaintEventArgs e)
    {
        if (sender is not Button button || !buttons.TryGetValue(button, out ButtonState? state)) return;
        bool dark = button.BackColor.GetBrightness() < 0.25F;
        bool primary = button.BackColor.G > button.BackColor.R + 20;
        Color fill = button.BackColor;
        if (!button.Enabled) fill = Color.FromArgb(236, 239, 236);
        else if (state.Pressed) fill = primary ? Color.FromArgb(69, 151, 43) : Color.FromArgb(220, 232, 224);
        else if (state.Hover) fill = dark ? Color.FromArgb(47, 62, 52) : primary ? Color.FromArgb(102, 189, 73) : Color.FromArgb(240, 247, 237);
        e.Graphics.Clear(button.Parent?.BackColor ?? Background);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        Rectangle bounds = new(1, 1, Math.Max(1, button.Width - 3), Math.Max(1, button.Height - 3));
        using GraphicsPath shape = RoundedRectangle(bounds, 8);
        using SolidBrush brush = new(fill);
        e.Graphics.FillPath(brush, shape);
        if (!primary && !dark)
        {
            using Pen outline = new(Border);
            e.Graphics.DrawPath(outline, shape);
        }
        Color text = !button.Enabled ? Muted : dark ? Color.White : Ink;
        string icon = button.Name switch { "back" => "back", "add" => "add", "refresh" => "refresh",
            "catalog" or "inventory" => "grid", "builder" => "build", "orders" or "tools" => "list",
            "users" => "user", "reports" or "home" => "chart", "signout" => "log-out", "backup" => "download", "restore" => "rotate-ccw", _ => "" };
        Rectangle textBounds = bounds;
        if (icon != "")
        {
            IconAssets.Paint(e.Graphics, icon, new Rectangle(14, (button.Height - 18) / 2, 18, 18), text);
            textBounds.X += 40; textBounds.Width -= 48;
        }
        TextRenderer.DrawText(e.Graphics, button.Text, button.Font, textBounds, text,
            (dark || (button.Parent?.Name == "sidebar" && icon != "") ? TextFormatFlags.Left : TextFormatFlags.HorizontalCenter) | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        if (button.Focused)
            ControlPaint.DrawFocusRectangle(e.Graphics, Rectangle.Inflate(bounds, -5, -5), text, fill);
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
