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
    public string Message { get => message.Text; set { message.Text = value; PerformLayout(); } }
    [DefaultValue("")]
    public string Detail { get => detail.Text; set { detail.Text = value; PerformLayout(); } }
    [DefaultValue("")]
    public string ActionText
    {
        get => action.Text;
        set { action.Text = value; action.Visible = !string.IsNullOrWhiteSpace(value); PerformLayout(); }
    }
    public event EventHandler? ActionClicked;
    private void Action_Click(object? sender, EventArgs e) => ActionClicked?.Invoke(this, e);
    protected override void OnLayout(LayoutEventArgs e)
    {
        base.OnLayout(e);
        if (action == null || icon == null || message == null || detail == null) return;
        // Keep one compact group centred, instead of pinning the action to the bottom.
        foreach (Control child in new Control[] { icon, message, detail, action }) child.Dock = DockStyle.None;
        int textWidth = Math.Max(1, Width - 32);
        int messageHeight = Math.Max(40, TextRenderer.MeasureText(message.Text, message.Font,
            new Size(textWidth, int.MaxValue), TextFormatFlags.WordBreak).Height + 8);
        int detailHeight = string.IsNullOrWhiteSpace(detail.Text) ? 0 : Math.Max(32,
            TextRenderer.MeasureText(detail.Text, detail.Font, new Size(textWidth, int.MaxValue), TextFormatFlags.WordBreak).Height + 8);
        int buttonHeight = string.IsNullOrWhiteSpace(action.Text) ? 0 : action.Height + 16;
        int top = Math.Max(8, (Height - 64 - messageHeight - detailHeight - buttonHeight) / 2);
        icon.SetBounds(16, top, textWidth, 64);
        message.SetBounds(16, top + 64, textWidth, messageHeight);
        detail.SetBounds(16, message.Bottom, textWidth, detailHeight);
        action.Location = new Point(Math.Max(0, (Width - action.Width) / 2), detail.Bottom + 16);
    }
}
