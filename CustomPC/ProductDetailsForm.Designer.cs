namespace CustomPC;

partial class ProductDetailsForm
{
    private TableLayoutPanel layout = null!;
    private Label heading = null!;
    private Label summary = null!;
    private PictureBox photo = null!;
    private DataGridView specifications = null!;
    private Button close = null!;
    private void InitializeComponent()
    {
        layout = new TableLayoutPanel();
        heading = new Label();
        summary = new Label();
        photo = new PictureBox();
        specifications = new DataGridView();
        close = new Button();
        SuspendLayout();
        layout.Dock = DockStyle.Fill;
        layout.Padding = new Padding(24);
        layout.ColumnCount = 2;
        layout.RowCount = 4;
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 96F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F));
        heading.Text = "Product specifications";
        heading.Font = new Font("Righteous", 18F);
        heading.Dock = DockStyle.Fill;
        layout.Controls.Add(heading, 0, 0);
        layout.SetColumnSpan(heading, 2);
        summary.Dock = DockStyle.Fill;
        summary.Font = new Font("Roboto", 11F, FontStyle.Bold);
        layout.Controls.Add(summary, 0, 2);
        summary.TextAlign = ContentAlignment.TopLeft;

        photo.Dock = DockStyle.Fill;
        photo.SizeMode = PictureBoxSizeMode.Zoom;
        photo.Margin = new Padding(0, 0, 24, 16);
        photo.AccessibleName = "Product photograph";
        layout.Controls.Add(photo, 0, 1);
        layout.SetRowSpan(photo, 2);
        layout.Controls.Remove(summary);
        layout.Controls.Add(summary, 0, 3);
        summary.Margin = new Padding(0, 8, 16, 0);
        specifications.Dock = DockStyle.Fill;
        specifications.Name = "specifications";
        specifications.ReadOnly = true;
        specifications.AllowUserToAddRows = false;
        specifications.AllowUserToDeleteRows = false;
        specifications.AllowUserToResizeRows = false;
        specifications.RowHeadersVisible = false;
        specifications.ColumnHeadersVisible = false;
        specifications.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        specifications.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        specifications.BorderStyle = BorderStyle.None;
        specifications.BackgroundColor = Color.White;
        specifications.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        specifications.DefaultCellStyle.Padding = new Padding(12, 10, 12, 10);
        specifications.Columns.Add(new DataGridViewTextBoxColumn { Name = "Attribute", FillWeight = 45F, MinimumWidth = 130 });
        specifications.Columns.Add(new DataGridViewTextBoxColumn { Name = "Value", FillWeight = 55F, MinimumWidth = 150 });
        layout.Controls.Add(specifications, 1, 1);
        layout.SetRowSpan(specifications, 2);
        close.Name = "close";
        close.Text = "Close";
        close.Size = new Size(120, 40);
        close.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
        close.DialogResult = DialogResult.OK;
        layout.Controls.Add(close, 1, 3);
        Controls.Add(layout);
        Font = new Font("Roboto", 10F);
        BackColor = Color.White;
        ClientSize = new Size(900, 650);
        MinimumSize = new Size(720, 500);
        StartPosition = FormStartPosition.CenterParent;
        Text = "CustomPC | Product specifications";
        AcceptButton = close;
        CancelButton = close;
        ResumeLayout(false);
    }
}
