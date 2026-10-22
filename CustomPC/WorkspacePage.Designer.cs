namespace CustomPC;

partial class WorkspacePage
{
    private TableLayoutPanel layout = null!;
    private Panel header = null!;
    private FlowLayoutPanel commands = null!;
    private FlowLayoutPanel filters = null!;
    private Panel content = null!;
    private Label footer = null!;

    private void InitializeComponent()
    {
        layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, Padding = new Padding(28, 16, 28, 12) };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 88));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        header = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0) };
        commands = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true, Margin = new Padding(0, 0, 0, 8) };
        filters = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true, Margin = new Padding(0, 0, 0, 8) };
        content = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(1), Margin = new Padding(0) };
        footer = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = StoreTheme.Muted, Margin = new Padding(0) };
        layout.Controls.Add(header, 0, 0);
        layout.Controls.Add(commands, 0, 1);
        layout.Controls.Add(filters, 0, 2);
        layout.Controls.Add(content, 0, 3);
        layout.Controls.Add(footer, 0, 4);
        Controls.Add(layout);
        Dock = DockStyle.Fill;
        BackColor = StoreTheme.Background;
        Font = new Font("Roboto", 10F);
        Size = new Size(1000, 700);
    }
}
