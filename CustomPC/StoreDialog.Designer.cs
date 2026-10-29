namespace CustomPC;

partial class StoreDialog
{
    private Label heading = null!;
    private Label body = null!;
    private Button accept = null!;
    private Button cancel = null!;
    private Panel bodyPanel = null!;
    private FlowLayoutPanel actions = null!;
    private TableLayoutPanel layout = null!;
    private void InitializeComponent()
    {
        SuspendLayout();
        heading = new Label();
        heading.Text = "Message";
        heading.Font = AppFonts.Heading(18F);
        heading.Dock = DockStyle.Fill;
        heading.TextAlign = ContentAlignment.MiddleLeft;
        body = new Label();
        body.Name = "body";
        body.AutoSize = true;
        body.MaximumSize = new Size(520, 0);
        body.Location = new Point(8, 8);
        body.ForeColor = StoreTheme.Muted;
        bodyPanel = new Panel();
        bodyPanel.Dock = DockStyle.Fill;
        bodyPanel.AutoScroll = true;
        bodyPanel.Controls.Add(body);
        accept = new Button();
        accept.Name = "accept";
        accept.Text = "OK";
        accept.Size = new Size(128, 44);
        accept.BackColor = StoreTheme.Green;
        accept.Margin = new Padding(8, 12, 0, 0);
        cancel = new Button();
        cancel.Name = "cancel";
        cancel.Text = "Cancel";
        cancel.Size = new Size(128, 44);
        cancel.BackColor = Color.White;
        cancel.Margin = new Padding(8, 12, 0, 0);
        actions = new FlowLayoutPanel();
        actions.Dock = DockStyle.Fill;
        actions.FlowDirection = FlowDirection.RightToLeft;
        actions.WrapContents = false;
        actions.Controls.Add(accept); actions.Controls.Add(cancel);
        layout = new TableLayoutPanel();
        layout.Dock = DockStyle.Fill;
        layout.Padding = new Padding(24);
        layout.ColumnCount = 1;
        layout.RowCount = 3;
        layout.BackColor = Color.White;
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 68));
        layout.Controls.Add(heading, 0, 0); layout.Controls.Add(bodyPanel, 0, 1); layout.Controls.Add(actions, 0, 2);
        Controls.Add(layout);
        Font = new Font("Roboto", 11F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(600, 410);
        MinimumSize = new Size(480, 280);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false; MinimizeBox = false; ShowInTaskbar = false;
        Text = "CustomPC | Message";
        ResumeLayout(true);
    }
}
