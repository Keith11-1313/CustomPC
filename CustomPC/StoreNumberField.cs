using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace CustomPC;

public class StoreNumberField : UserControl
{
    private readonly NumericUpDown input = new() { BorderStyle = BorderStyle.None };
    public StoreNumberField()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        Height = 38;
        BackColor = Color.White;
        Controls.Add(input);
        input.Enter += (_, _) => Invalidate();
        input.Leave += (_, _) => Invalidate();
        Click += (_, _) => input.Focus();
    }
    [DefaultValue(typeof(decimal), "0")]
    public decimal Value { get => input.Value; set => input.Value = value; }
    [DefaultValue(typeof(decimal), "0")]
    public decimal Minimum { get => input.Minimum; set => input.Minimum = value; }
    [DefaultValue(typeof(decimal), "100")]
    public decimal Maximum { get => input.Maximum; set => input.Maximum = value; }
    [DefaultValue(0)]
    public int DecimalPlaces { get => input.DecimalPlaces; set => input.DecimalPlaces = value; }
    [DefaultValue(false)]
    public bool ThousandsSeparator { get => input.ThousandsSeparator; set => input.ThousandsSeparator = value; }
    protected override void OnEnter(EventArgs e) { base.OnEnter(e); input.Focus(); }
    protected override void OnFontChanged(EventArgs e) { base.OnFontChanged(e); if (input != null) { input.Font = Font; PerformLayout(); } }
    protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        => base.SetBoundsCore(x, y, width, Math.Max(38, height), specified);
    protected override void OnLayout(LayoutEventArgs e)
    {
        base.OnLayout(e);
        if (input != null) input.SetBounds(12, Math.Max(3, (Height - input.PreferredHeight) / 2), Math.Max(1, Width - 24), input.PreferredHeight);
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(Parent?.BackColor ?? StoreTheme.Background);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using GraphicsPath path = StoreTheme.RoundedRectangle(new Rectangle(1, 1, Width - 3, Height - 3), 7);
        using SolidBrush fill = new(Color.White);
        using Pen pen = new(ContainsFocus ? StoreTheme.Action : StoreTheme.Border, ContainsFocus ? 2 : 1);
        e.Graphics.FillPath(fill, path); e.Graphics.DrawPath(pen, path);
    }
}
