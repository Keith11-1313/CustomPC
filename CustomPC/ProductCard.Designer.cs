namespace CustomPC;

partial class ProductCard
{
    private ProductThumbnail photograph = null!;
    private Label categoryCaption = null!;
    private Label productName = null!;
    private Label price = null!;
    private Label stock = null!;
    private Button choose = null!;
    private Button details = null!;
    private void InitializeComponent()
    {
        photograph = new ProductThumbnail();
        categoryCaption = new Label();
        productName = new Label();
        price = new Label();
        stock = new Label();
        choose = new Button();
        details = new Button();
        SuspendLayout();
        photograph.Location = new Point(16, 12);
        photograph.Size = new Size(226, 170);
        categoryCaption.Location = new Point(16, 190);
        categoryCaption.Size = new Size(226, 22);
        categoryCaption.Text = "Category";
        categoryCaption.ForeColor = Color.FromArgb(92, 105, 98);
        categoryCaption.Font = new Font("Roboto", 9F);
        productName.Location = new Point(16, 218);
        productName.Size = new Size(226, 60);
        productName.Text = "Product name";
        productName.Font = new Font("Roboto", 11F, FontStyle.Bold);
        price.Location = new Point(16, 284);
        price.Size = new Size(226, 30);
        price.Text = "PHP 0.00";
        price.Font = new Font("Roboto", 15F, FontStyle.Bold);
        price.ForeColor = Color.FromArgb(46, 124, 33);
        stock.Location = new Point(16, 321);
        stock.Size = new Size(226, 24);
        stock.Text = "Stock status";
        stock.BackColor = Color.FromArgb(241, 246, 239);
        stock.Padding = new Padding(6, 0, 0, 0);
        stock.TextAlign = ContentAlignment.MiddleLeft;
        choose.Location = new Point(16, 356);
        choose.Size = new Size(226, 42);
        choose.Text = "Add to build";
        choose.BackColor = Color.FromArgb(84, 177, 55);
        choose.FlatStyle = FlatStyle.Flat;
        choose.FlatAppearance.BorderSize = 0;
        details.Location = new Point(16, 405);
        details.Size = new Size(226, 28);
        details.Text = "Specifications";
        details.FlatStyle = FlatStyle.Flat;
        details.FlatAppearance.BorderSize = 0;
        details.BackColor = Color.White;
        Controls.Add(photograph);
        Controls.Add(categoryCaption);
        Controls.Add(productName);
        Controls.Add(price);
        Controls.Add(stock);
        Controls.Add(choose);
        Controls.Add(details);
        BackColor = Color.White;
        Font = new Font("Roboto", 10F);
        Size = new Size(258, 445);
        Margin = new Padding(10);
        ResumeLayout(false);
    }
}
