using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace CustomPC;

// The Windows calendar remains available inside the same padded field surface.
public class StoreDateField : UserControl
{
    private readonly CalendarInput input = new();
    public StoreDateField()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        BackColor = Color.White;
        Height = 38;
        Controls.Add(input);
        input.Enter += (_, _) => Invalidate();
        input.Leave += (_, _) => Invalidate();
    }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public DateTime Value { get => input.Value; set => input.Value = value; }
    [DefaultValue(DateTimePickerFormat.Long)]
    public DateTimePickerFormat Format { get => input.Format; set => input.Format = value; }
    protected override void OnEnter(EventArgs e) { base.OnEnter(e); input.Focus(); }
    protected override void OnFontChanged(EventArgs e) { base.OnFontChanged(e); if (input != null) { input.Font = Font; PerformLayout(); } }
    protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        => base.SetBoundsCore(x, y, width, Math.Max(38, height), specified);
    protected override void OnLayout(LayoutEventArgs e)
    {
        base.OnLayout(e);
        if (input != null) input.SetBounds(34, Math.Max(3, (Height - input.PreferredHeight) / 2), Math.Max(1, Width - 44), input.PreferredHeight);
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(Parent?.BackColor ?? StoreTheme.Background);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using GraphicsPath path = StoreTheme.RoundedRectangle(new Rectangle(1, 1, Width - 3, Height - 3), 7);
        using SolidBrush fill = new(Color.White);
        using Pen pen = new(ContainsFocus ? Color.FromArgb(56, 132, 37) : Color.FromArgb(194, 204, 198), ContainsFocus ? 2 : 1);
        e.Graphics.FillPath(fill, path); e.Graphics.DrawPath(pen, path);
        IconAssets.Paint(e.Graphics, "calendar-days", new Rectangle(10, (Height - 18) / 2, 18, 18), StoreTheme.Muted);
    }
    private sealed class CalendarInput : DateTimePicker
    {
        protected override CreateParams CreateParams
        {
            get { CreateParams parameters = base.CreateParams; parameters.Style &= ~0x00800000; parameters.ExStyle &= ~0x00000200; return parameters; }
        }
    }
}
