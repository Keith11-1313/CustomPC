namespace CustomPC;

partial class EmptyStateControl
{
    private Label icon = null!;
    private Label message = null!;
    private Label detail = null!;
    private Button action = null!;
    private void InitializeComponent()
    {
        icon = new Label();
        message = new Label();
        detail = new Label();
        action = new Button();
        SuspendLayout();
        icon.Text = "◇";
        icon.Font = new Font("Segoe UI", 28F);
        icon.Dock = DockStyle.Top;
        icon.Height = 64;
        icon.TextAlign = ContentAlignment.MiddleCenter;
        icon.ForeColor = StoreTheme.Green;
        message.Text = "No records found";
        message.Font = new Font("Roboto", 13F, FontStyle.Bold);
        message.Dock = DockStyle.Top;
        message.Height = 40;
        message.TextAlign = ContentAlignment.MiddleCenter;
        detail.Dock = DockStyle.Top;
        detail.Height = 56;
        detail.TextAlign = ContentAlignment.MiddleCenter;
        detail.ForeColor = StoreTheme.Muted;
        action.Location = new Point(70, 172);
        action.Size = new Size(220, 40);
        action.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        action.FlatStyle = FlatStyle.Flat;
        action.BackColor = StoreTheme.Green;
        action.Visible = false;
        action.Click += Action_Click;
        Controls.Add(detail);
        Controls.Add(message);
        Controls.Add(icon);
        Controls.Add(action);
        BackColor = Color.White;
        Font = new Font("Roboto", 10F);
        Size = new Size(360, 220);
        ResumeLayout(false);
    }
}
