namespace CustomPC;

partial class PolicyForm
{
    private Label heading = null!;
    private FlowLayoutPanel sections = null!;
    private void InitializeComponent()
    {
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3,
            Padding = new Padding(28), BackColor = Color.White };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
        heading = new Label { Text = "Shop policy", AutoSize = true, UseMnemonic = false,
            Font = new Font("Segoe UI", 22, FontStyle.Bold), Margin = new Padding(0, 0, 0, 24) };
        sections = new FlowLayoutPanel { Name = "policySections", Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown,
            WrapContents = false, AutoScroll = true, TabStop = true, Margin = new Padding(0), Padding = new Padding(0, 0, 8, 0) };
        var footer = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false, Padding = new Padding(0, 16, 0, 0), Margin = new Padding(0) };
        var close = new Button { Name = "close", Text = "Close", Size = new Size(120, 42),
            DialogResult = DialogResult.OK, BackColor = StoreTheme.Action, ForeColor = Color.White };
        footer.Controls.Add(close);
        layout.Controls.Add(heading, 0, 0); layout.Controls.Add(sections, 0, 1); layout.Controls.Add(footer, 0, 2);
        Controls.Add(layout);
        Font = new Font("Segoe UI", 10.5F); AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(700, 700); MinimumSize = new Size(480, 420);
        StartPosition = FormStartPosition.CenterParent; MinimizeBox = false; MaximizeBox = false; ShowInTaskbar = false;
        Text = "CustomPC | Shop policy"; AcceptButton = close; CancelButton = close;
    }
}
