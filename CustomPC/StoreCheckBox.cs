using System.Drawing.Drawing2D;

namespace CustomPC;

// Keep CheckBox keyboard/accessibility behavior and replace only its drawing.
public class StoreCheckBox : CheckBox
{
    public StoreCheckBox() { AutoSize = false; SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true); }
    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(Parent?.BackColor ?? StoreTheme.Background);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        Rectangle box = new(2, Math.Max(2, (Height - 18) / 2), 18, 18);
        using GraphicsPath path = StoreTheme.RoundedRectangle(box, 4);
        using SolidBrush fill = new(Checked ? Color.FromArgb(61, 137, 39) : Color.White);
        using Pen border = new(Checked ? Color.FromArgb(61, 137, 39) : Color.FromArgb(155, 171, 161), 1.4F);
        e.Graphics.FillPath(fill, path); e.Graphics.DrawPath(border, path);
        if (Checked) IconAssets.Paint(e.Graphics, "check", Rectangle.Inflate(box, -2, -2), Color.White);
        Rectangle text = new(30, 0, Math.Max(1, Width - 32), Height);
        TextRenderer.DrawText(e.Graphics, Text, Font, text, Enabled ? StoreTheme.Ink : StoreTheme.Muted, TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        if (Focused) ControlPaint.DrawFocusRectangle(e.Graphics, text);
    }
}
