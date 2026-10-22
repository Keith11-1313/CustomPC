namespace CustomPC;

partial class ProductCard
{
    private TableLayoutPanel cardLayout = null!;
    private TableLayoutPanel cardActions = null!;
    private ProductThumbnail photograph = null!;
    private Label categoryCaption = null!;
    private Label productName = null!;
    private Label price = null!;
    private Label stock = null!;
    private Button choose = null!;
    private Button details = null!;
    private void InitializeComponent()
    {
        SuspendLayout();
        cardLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 6, Padding = new Padding(16), BackColor = Color.White, Margin = new Padding(0) };
        cardLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        foreach (int height in new[] { 160, 25, 56, 30, 40, 44 }) cardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
        photograph = new ProductThumbnail { Dock = DockStyle.Fill, Margin = new Padding(12, 0, 12, 12) };
        categoryCaption = new Label { Dock = DockStyle.Fill, Text = "Category", Font = new Font("Roboto", 9F), ForeColor = StoreTheme.Muted, TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(0) };
        productName = new Label { Dock = DockStyle.Fill, Text = "Product name", Font = new Font("Roboto", 12F, FontStyle.Bold), AutoEllipsis = true, Margin = new Padding(0, 2, 0, 4) };
        stock = new Label { Dock = DockStyle.Fill, Text = "In stock", Font = new Font("Roboto", 9F), BackColor = StoreTheme.Selection, Padding = new Padding(8, 0, 0, 0), TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(0, 2, 0, 2) };
        price = new Label { Dock = DockStyle.Fill, Text = "PHP 0.00", Font = new Font("Roboto", 16F, FontStyle.Bold), ForeColor = StoreTheme.Ink, TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(0) };
        cardActions = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = new Padding(0) };
        cardActions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64)); cardActions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36));
        choose = new Button { Name = "choose", Text = "Add to build", Dock = DockStyle.Fill, BackColor = StoreTheme.Green, Margin = new Padding(0, 0, 6, 0) };
        details = new Button { Name = "details", Text = "Details", Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(0) };
        cardActions.Controls.Add(choose, 0, 0); cardActions.Controls.Add(details, 1, 0);
        cardLayout.Controls.Add(photograph, 0, 0); cardLayout.Controls.Add(categoryCaption, 0, 1); cardLayout.Controls.Add(productName, 0, 2);
        cardLayout.Controls.Add(stock, 0, 3); cardLayout.Controls.Add(price, 0, 4); cardLayout.Controls.Add(cardActions, 0, 5);
        Controls.Add(cardLayout);
        BackColor = Color.White; Font = new Font("Roboto", 10F);
        Size = new Size(286, 391); Margin = new Padding(8);
        ResumeLayout(true);
    }
}
