using System.ComponentModel;

namespace CustomPC;

// Reusable and editable in Designer. It never opens the store or chooses permissions itself.
public partial class EmptyStateControl : UserControl
{
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
