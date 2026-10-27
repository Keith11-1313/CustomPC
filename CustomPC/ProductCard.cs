using System.Drawing.Drawing2D;

namespace CustomPC;

// The layout is editable in ProductCard.Designer.cs. Only this constructor binds saved data.
public partial class ProductCard : UserControl
{
    private bool? compactLayout;
    private bool fittingText;
    private int minimumNameHeight;
    internal int NameRowHeight => (int)cardLayout.RowStyles[compactLayout == true ? 1 : 2].Height;
    internal void SetMinimumNameHeight(int height)
    {
        minimumNameHeight = height;
        FitCardText();
    }
    public void UseCompactLayout(bool compact)
    {
        if (compactLayout == compact) { FitCardText(); return; }
        compactLayout = compact;
        cardLayout.SuspendLayout();
        cardLayout.Controls.Clear(); cardLayout.ColumnStyles.Clear(); cardLayout.RowStyles.Clear();
        cardLayout.ColumnCount = compact ? 2 : 1; cardLayout.RowCount = compact ? 5 : 6;
        if (compact) cardLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 112 * DeviceDpi / 96F));
        cardLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        foreach (int height in compact ? new[] { 20, 44, 28, 32, 44 } : new[] { 160, 25, 56, 30, 40, 44 })
            cardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, height * DeviceDpi / 96F));
        cardLayout.Controls.Add(photograph, 0, 0);
        cardLayout.SetRowSpan(photograph, compact ? 5 : 1);
        Control[] right = { categoryCaption, productName, stock, price, cardActions };
        for (int i = 0; i < right.Length; i++) cardLayout.Controls.Add(right[i], compact ? 1 : 0, compact ? i : i + 1);
        cardLayout.ResumeLayout(true);
        FitCardText();
    }
    private void FitCardText()
    {
        if (fittingText || compactLayout == null) return;
        fittingText = true;
        try
        {
            float scale = DeviceDpi / 96F;
            bool compact = compactLayout.Value;
            int textWidth = Math.Max(40, Width - Padding.Horizontal - cardLayout.Padding.Horizontal -
                (compact ? (int)(112 * scale) : 0) - productName.Margin.Horizontal);
            int nameHeight = TextRenderer.MeasureText(productName.Text, productName.Font,
                new Size(textWidth, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix).Height + productName.Margin.Vertical + (int)(4 * scale);
            cardLayout.RowStyles[compact ? 1 : 2].Height = Math.Max(nameHeight, minimumNameHeight);
            Height = (int)cardLayout.RowStyles.Cast<RowStyle>().Sum(row => row.Height) + cardLayout.Padding.Vertical + Padding.Vertical;
        }
        finally { fittingText = false; }
    }
    public ProductCard()
    {
        InitializeComponent();
        Padding = new Padding(1);
        SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        SizeChanged += (_, _) => FitCardText();
    }

    public ProductCard(Part part, AppStore store, Action<Part>? select) : this()
    {
        productName.Text = part.Name;
        categoryCaption.Text = part.Brand.Length == 0 ? part.Category : part.Brand + " / " + part.Category;
        price.Text = Ui.Money(part.Price);
        stock.Text = store.Available(part) <= 0 ? "Out of stock" : store.StockState(part) == "Low stock"
            ? $"Low stock · {store.Available(part)} available" : $"In stock · {store.Available(part)} available";
        stock.BackColor = Color.White;
        stock.Padding = Padding.Empty;
        photograph.Display(part, store.DatabasePath);
        choose.Text = select == null ? "Specifications" : "Add to build";
        choose.Enabled = select == null || store.Available(part) > 0;
        choose.BackColor = !choose.Enabled ? Color.FromArgb(226, 232, 224) : select == null ? Color.White : StoreTheme.Green;
        choose.Click += (_, _) => { if (select != null) select(part); else ShowSpecifications(part); };
        details.Visible = select != null;
        details.Click += (_, _) => ShowSpecifications(part);
        AccessibleName = part.Name;
        AppFonts.Apply(this);
        StoreTheme.Apply(this);
        stock.ForeColor = store.Available(part) <= 0 ? Color.FromArgb(155, 58, 58) : StoreTheme.Action;
    }

    private void ShowSpecifications(Part part)
    {
        using ProductDetailsForm details = new(part);
        details.ShowDialog(FindForm());
    }

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
