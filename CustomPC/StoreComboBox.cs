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
        using SolidBrush background = new(selected ? Color.FromArgb(230, 244, 224) : Color.White);
        e.Graphics.FillRectangle(background, e.Bounds);
        string text = e.Index < 0 ? Text : GetItemText(Items[e.Index]) ?? "";
        Rectangle area = Rectangle.Inflate(e.Bounds, -12, 0);
        TextRenderer.DrawText(e.Graphics, text, Font, area, Enabled ? StoreTheme.Ink : StoreTheme.Muted,
            TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        if ((e.State & DrawItemState.Focus) != 0) e.DrawFocusRectangle();
    }
    protected override void OnDropDown(EventArgs e)
    {
        DropDownWidth = Math.Min(Math.Max(Width, 400), Screen.FromControl(this).WorkingArea.Width - 40);
        base.OnDropDown(e);
    }
    protected override void WndProc(ref Message message)
    {
        base.WndProc(ref message);
        if (message.Msg != 0x000F && message.Msg != 0x0318) return; // WM_PAINT / WM_PRINTCLIENT
        using Graphics graphics = message.Msg == 0x0318 && message.WParam != IntPtr.Zero
            ? Graphics.FromHdc(message.WParam) : Graphics.FromHwnd(Handle);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using Pen border = new(Focused ? Color.FromArgb(56, 132, 37) : Color.FromArgb(194, 204, 198), Focused ? 2 : 1);
        graphics.DrawRectangle(border, 0, 0, Width - 1, Height - 1);
        using SolidBrush fill = new(Enabled ? Color.White : Color.FromArgb(241, 243, 242));
        graphics.FillRectangle(fill, Width - 30, 2, 28, Height - 4);
        IconAssets.Paint(graphics, "chevron-down", new Rectangle(Width - 25, (Height - 18) / 2, 18, 18), StoreTheme.Muted);
    }
}
