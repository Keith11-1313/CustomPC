using System.Drawing.Drawing2D;

namespace CustomPC;

// The layout is editable in ProductCard.Designer.cs. Only this constructor binds saved data.
public partial class ProductCard : UserControl
{
    public ProductCard() { InitializeComponent(); }

    public ProductCard(Part part, AppStore store, Action<Part>? select) : this()
    {
        productName.Text = part.Name;
        categoryCaption.Text = part.Category;
        price.Text = Ui.Money(part.Price);
        stock.Text = store.StockState(part) + $" · {store.Available(part)} units";
        stock.ForeColor = store.Available(part) <= 0 ? Color.FromArgb(155, 58, 58) : StoreTheme.Muted;
        photograph.Display(part, store.DatabasePath);
        choose.Text = select == null ? "Specifications" : "Add to build";
        choose.Enabled = select == null || store.Available(part) > 0;
        choose.BackColor = !choose.Enabled ? Color.FromArgb(226, 232, 224) : select == null ? Color.White : StoreTheme.Green;
        choose.Click += (_, _) => { if (select != null) select(part); else ShowSpecifications(part); };
        details.Visible = select != null;
        details.Click += (_, _) => ShowSpecifications(part);
        AccessibleName = part.Name;
        AppFonts.Apply(this);
    }

    private void ShowSpecifications(Part part) => MessageBox.Show(FindForm(), part.Specifications, part.Name);

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using GraphicsPath outline = new();
        const int diameter = 10;
        outline.AddArc(0, 0, diameter, diameter, 180, 90);
        outline.AddArc(Width - diameter - 1, 0, diameter, diameter, 270, 90);
        outline.AddArc(Width - diameter - 1, Height - diameter - 1, diameter, diameter, 0, 90);
        outline.AddArc(0, Height - diameter - 1, diameter, diameter, 90, 90);
        outline.CloseFigure();
        using Pen pen = new(StoreTheme.Border);
        e.Graphics.DrawPath(pen, outline);
    }
}
