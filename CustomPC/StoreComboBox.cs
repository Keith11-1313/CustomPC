using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace CustomPC;

// Windows owns the popup, scrolling and keyboard selection; we draw its rows.
public class StoreComboBox : ComboBox
{
    [System.ComponentModel.Browsable(false), System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public Func<object, string?>? WarningText { get; set; }
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
        if (e.Index >= 0 && Items[e.Index] is object item && !string.IsNullOrEmpty(WarningText?.Invoke(item)))
        {
            IconAssets.Paint(e.Graphics, "triangle-alert", new Rectangle(area.Left, area.Top + (area.Height - 18) / 2, 18, 18), Color.FromArgb(160, 90, 10));
            area.X += 26; area.Width = Math.Max(1, area.Width - 26);
            text += " · Incompatible";
        }
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
        if (message.Msg == 0x0014) // WM_ERASEBKGND: the buffered surface includes its background.
        {
            message.Result = (IntPtr)1;
            return;
        }
        if (message.Msg == 0x000F) // WM_PAINT: do not flash the native hover surface first.
        {
            IntPtr dc = BeginPaint(Handle, out PaintState state);
            try
            {
                if (dc != IntPtr.Zero && Width > 0 && Height > 0)
                {
                    using Graphics target = Graphics.FromHdc(dc);
                    using BufferedGraphics buffer = BufferedGraphicsManager.Current.Allocate(target, ClientRectangle);
                    DrawSurface(buffer.Graphics);
                    buffer.Render(target);
                }
            }
            finally { EndPaint(Handle, ref state); }
            message.Result = IntPtr.Zero;
            return;
        }
        if (message.Msg is 0x0318 or 0x0317 && message.WParam != IntPtr.Zero)
        {
            using Graphics graphics = Graphics.FromHdc(message.WParam);
            DrawSurface(graphics);
            message.Result = IntPtr.Zero;
            return;
        }
        base.WndProc(ref message);
    }

    private void DrawSurface(Graphics graphics)
    {
        var saved = graphics.Save();
        try
        {
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
        bool warning = SelectedItem != null && !string.IsNullOrEmpty(WarningText?.Invoke(SelectedItem));
        if (warning) IconAssets.Paint(graphics, "triangle-alert", new Rectangle(12, (Height - 18) / 2, 18, 18), Color.FromArgb(160, 90, 10));
        TextRenderer.DrawText(graphics, Text, Font, new Rectangle(warning ? 38 : 12, 0, Math.Max(1, Width - (warning ? 72 : 46)), Height),
            Enabled ? StoreTheme.Ink : StoreTheme.Muted, TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        IconAssets.Paint(graphics, "chevron-down", new Rectangle(Width - 25, (Height - 18) / 2, 18, 18), StoreTheme.Muted);
        }
        finally { graphics.Restore(saved); }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PaintState
    {
        public IntPtr Dc;
        public int Erase, Left, Top, Right, Bottom, Restore, Incremental;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] Reserved;
    }
    [DllImport("user32.dll")]
    private static extern IntPtr BeginPaint(IntPtr window, out PaintState state);
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EndPaint(IntPtr window, ref PaintState state);
}
