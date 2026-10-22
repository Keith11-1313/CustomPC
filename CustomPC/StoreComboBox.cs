using System.Drawing.Drawing2D;

namespace CustomPC;

// Windows owns the popup, scrolling and keyboard selection; we draw its rows.
public class StoreComboBox : ComboBox
{
    public StoreComboBox()
    {
        DrawMode = DrawMode.OwnerDrawFixed;
        ItemHeight = 30;
        FlatStyle = FlatStyle.Standard;
        BackColor = Color.White;
        DropDownStyle = ComboBoxStyle.DropDownList;
        IntegralHeight = false;
        DropDownHeight = 270;
    }
    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        bool selected = (e.State & DrawItemState.Selected) != 0;
        using SolidBrush background = new(selected ? StoreTheme.Selection : Color.White);
        e.Graphics.FillRectangle(background, e.Bounds);
        string text = e.Index < 0 ? Text : GetItemText(Items[e.Index]) ?? "";
        Rectangle area = Rectangle.Inflate(e.Bounds, -12, 0);
        TextRenderer.DrawText(e.Graphics, text, Font, area, Enabled ? StoreTheme.Ink : StoreTheme.Muted,
            TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        if ((e.State & DrawItemState.Focus) != 0) e.DrawFocusRectangle();
    }
    protected override void OnDropDown(EventArgs e)
    {
        DropDownWidth = Width;
        base.OnDropDown(e);
    }
    protected override void WndProc(ref Message message)
    {
        base.WndProc(ref message);
        if (message.Msg != 0x000F && message.Msg != 0x0318 && message.Msg != 0x0317) return; // Paint, print client, print
        using Graphics graphics = message.Msg is 0x0318 or 0x0317 && message.WParam != IntPtr.Zero
            ? Graphics.FromHdc(message.WParam) : Graphics.FromHwnd(Handle);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        Rectangle visible = ClientRectangle;
        for (Control? ancestor = Parent; ancestor != null; ancestor = ancestor.Parent)
            visible.Intersect(RectangleToClient(ancestor.RectangleToScreen(ancestor.ClientRectangle)));
        graphics.SetClip(visible, CombineMode.Intersect);
        // WM_PRINT shares the form's bitmap DC. Clear only this control's rectangle.
        using SolidBrush backdrop = new(Parent?.BackColor ?? StoreTheme.Background);
        graphics.FillRectangle(backdrop, ClientRectangle);
        using var shape = StoreTheme.RoundedRectangle(new Rectangle(1, 1, Math.Max(1, Width - 3), Math.Max(1, Height - 3)), 8);
        using Pen border = new(Focused ? StoreTheme.Action : StoreTheme.Border, Focused ? 2 : 1);
        using SolidBrush fill = new(Enabled ? Color.White : Color.FromArgb(241, 243, 242));
        graphics.FillPath(fill, shape); graphics.DrawPath(border, shape);
        TextRenderer.DrawText(graphics, Text, Font, new Rectangle(12, 0, Math.Max(1, Width - 46), Height),
            Enabled ? StoreTheme.Ink : StoreTheme.Muted, TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        IconAssets.Paint(graphics, "chevron-down", new Rectangle(Width - 25, (Height - 18) / 2, 18, 18), StoreTheme.Muted);
    }
}
