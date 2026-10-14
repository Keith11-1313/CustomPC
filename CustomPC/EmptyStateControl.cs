using System.ComponentModel;

namespace CustomPC;

// Reusable and editable in Designer. It never opens the store or chooses permissions itself.
public partial class EmptyStateControl : UserControl
{
    private void PaintEmptyIcon(object? sender, PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        int x = icon.Width / 2;
        using SolidBrush background = new(Color.FromArgb(238, 246, 234));
        e.Graphics.FillEllipse(background, x - 25, 5, 50, 50);
        IconAssets.Paint(e.Graphics, "inbox", new Rectangle(x - 14, 16, 28, 28), Color.FromArgb(65, 127, 43));
    }
    public EmptyStateControl() { InitializeComponent(); }
    [DefaultValue("No records found")]
    public string Message { get => message.Text; set => message.Text = value; }
    [DefaultValue("")]
    public string Detail { get => detail.Text; set => detail.Text = value; }
    [DefaultValue("")]
    public string ActionText
    {
        get => action.Text;
        set { action.Text = value; action.Visible = !string.IsNullOrWhiteSpace(value); }
    }
    public event EventHandler? ActionClicked;
    private void Action_Click(object? sender, EventArgs e) => ActionClicked?.Invoke(this, e);
    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (action != null) action.Location = new Point(Math.Max(0, (Width - action.Width) / 2), Math.Max(0, Height - action.Height - 8));
    }
    protected override void OnLayout(LayoutEventArgs e)
    {
        base.OnLayout(e);
        if (action != null) action.Location = new Point(Math.Max(0, (Width - action.Width) / 2), Math.Max(0, Height - action.Height - 8));
    }
}
