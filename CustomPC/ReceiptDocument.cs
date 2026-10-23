using System.Drawing.Drawing2D;

namespace CustomPC;

// One document renderer serves both the paper preview and the printer.
internal sealed class ReceiptDocument
{
    private readonly CustomerOrder order;
    private readonly Payment? payment;
    private const float Width = 760, Height = 1000;
    private sealed record Block(float Height, Action<Graphics, float> Draw);
    public ReceiptDocument(CustomerOrder order, Payment? payment) { this.order = order; this.payment = payment; }
    private static Font Font(float size, bool bold = false) => new("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular, GraphicsUnit.Pixel);
    private static void Text(Graphics g, string text, float x, float y, float width, float height, float size = 14, bool bold = false, Color? color = null, bool right = false)
    {
        using var font = Font(size, bold);
        using var brush = new SolidBrush(color ?? StoreTheme.Ink);
        using var format = new StringFormat { Alignment = right ? StringAlignment.Far : StringAlignment.Near, Trimming = StringTrimming.None };
        g.DrawString(text, font, brush, new RectangleF(x, y, width, height), format);
    }
    private static float Measure(Graphics g, string text, float width, float size = 14, bool bold = false)
    {
        using var font = Font(size, bold);
        return (float)Math.Ceiling(g.MeasureString(text, font, (int)width).Height);
    }
    private float TableTop(Graphics g) => 238 + Math.Max(0, Measure(g, order.CustomerName, 320, 17, true) - 48);
    private List<List<Block>> Pages(Graphics g)
    {
        var blocks = new List<Block>();
        foreach (var line in order.Lines)
        {
            float h = Math.Max(56, Math.Max(Measure(g, line.Name, 310, bold: true),
                Math.Max(Measure(g, Ui.Money(line.Price), 140), Measure(g, Ui.Money(line.Price * line.Quantity), 158, bold: true))) + 28);
            blocks.Add(new Block(h, (canvas, y) => {
                Text(canvas, line.Name, 40, y + 14, 310, h - 20, bold: true);
                Text(canvas, line.Quantity.ToString("N0"), 354, y + 14, 44, h - 20, right: true);
                Text(canvas, Ui.Money(line.Price), 410, y + 14, 140, h - 20, right: true);
                Text(canvas, Ui.Money(line.Price * line.Quantity), 562, y + 14, 158, h - 20, bold: true, right: true);
                using var pen = new Pen(StoreTheme.Border); canvas.DrawLine(pen, 40, y + h, 720, y + h);
            }));
        }
        if (blocks.Count == 0) blocks.Add(new Block(56, (canvas, y) => Text(canvas, "No items recorded.", 40, y + 14, 680, 30)));
        string note = payment != null ? "Payment confirmed at the store. Keep this receipt for collection." : order.Status == "Pending Payment" ? "Pay at the store before the deadline to keep your reservation. Unpaid orders are cancelled after the deadline." : "No confirmed payment is recorded for this order.";
        string paidDetail = payment == null ? "Unpaid" : $"Paid {payment.PaidUtc.ToLocalTime():MMM d, yyyy · h:mm tt}";
        blocks.Add(new Block(198, (canvas, y) => {
            using var tint = new SolidBrush(StoreTheme.Selection); canvas.FillRectangle(tint, 40, y + 22, 680, 72);
            Text(canvas, "ORDER TOTAL", 58, y + 46, 210, 26, 13, true, StoreTheme.Action);
            Text(canvas, Ui.Money(order.Total), 274, y + 37, 428, 46, 30, true, StoreTheme.Action, true);
            Text(canvas, paidDetail, 40, y + 108, 680, 25, 13, true);
            Text(canvas, note, 40, y + 139, 680, 50, 13, color: StoreTheme.Muted);
        }));
        var pages = new List<List<Block>> { new() };
        float used = 0;
        foreach (var block in blocks)
        {
            if (used + block.Height > 926 - (TableTop(g) + 38) && pages[^1].Count > 0) { pages.Add(new()); used = 0; }
            pages[^1].Add(block); used += block.Height;
        }
        return pages;
    }
    public int PageCount(Graphics g) => Pages(g).Count;
    public bool DrawPage(Graphics g, RectangleF bounds, int index)
    {
        var state = g.Save();
        try
        {
            g.PageUnit = GraphicsUnit.Pixel;
            g.TranslateTransform(bounds.X, bounds.Y);
            g.ScaleTransform(bounds.Width / Width, bounds.Height / Height);
            g.FillRectangle(Brushes.White, 0, 0, Width, Height);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            var pages = Pages(g);
            if (index >= pages.Count) return false;
            using var green = new SolidBrush(StoreTheme.Action); g.FillRectangle(green, 40, 34, 42, 5);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            BrandLogo.Draw(g, new RectangleF(40, 53, 54, 48));
            Text(g, "CustomPC", 106, 53, 280, 48, 32, true);
            Text(g, "ORDER RECEIPT", 415, 61, 305, 26, 14, true, StoreTheme.Action, true);
            Text(g, order.Number, 340, 91, 380, 25, 14, true, right: true);
            Text(g, "CUSTOMER", 40, 127, 320, 20, 11, true, StoreTheme.Muted);
            float tableTop = TableTop(g);
            Text(g, order.CustomerName, 40, 149, 320, tableTop - 190, 17, true);
            Text(g, $"Ordered  {order.CreatedUtc.ToLocalTime():MMM d, yyyy · h:mm tt}", 390, 128, 330, 24, 13, right: true);
            Text(g, order.Status, 390, 155, 330, 24, 14, true, right: true);
            if (payment == null && order.Status == "Pending Payment") Text(g, $"Pay before  {order.DeadlineUtc.ToLocalTime():MMM d, yyyy · h:mm tt}", 40, tableTop - 36, 680, 24, 13, true, StoreTheme.Action);
            using var shade = new SolidBrush(StoreTheme.Background); g.FillRectangle(shade, 40, tableTop, 680, 38);
            Text(g, "COMPONENT", 50, tableTop + 12, 300, 20, 11, true, StoreTheme.Muted);
            Text(g, "QTY", 354, tableTop + 12, 44, 20, 11, true, StoreTheme.Muted, true);
            Text(g, "UNIT PRICE", 410, tableTop + 12, 140, 20, 11, true, StoreTheme.Muted, true);
            Text(g, "AMOUNT", 562, tableTop + 12, 148, 20, 11, true, StoreTheme.Muted, true);
            float y = tableTop + 38;
            foreach (var block in pages[index]) { block.Draw(g, y); y += block.Height; }
            using var border = new Pen(StoreTheme.Border); g.DrawLine(border, 40, 947, 720, 947);
            Text(g, "Thank you for choosing CustomPC. · In-store payment and pickup", 40, 961, 580, 22, 11, color: StoreTheme.Muted);
            Text(g, $"{index + 1} / {pages.Count}", 640, 961, 80, 22, 11, color: StoreTheme.Muted, right: true);
            return index + 1 < pages.Count;
        }
        finally { g.Restore(state); }
    }
}

internal sealed class ReceiptPreview : Panel
{
    private readonly Paper paper = new();
    public ReceiptPreview()
    {
        AutoScroll = true; TabStop = true; BackColor = StoreTheme.Background; Controls.Add(paper);
        SizeChanged += (_, _) => Reflow();
        DpiChangedAfterParent += (_, _) => Reflow();
    }
    public void SetDocument(ReceiptDocument? document) { paper.Document = document; Reflow(); }
    private void Reflow()
    {
        AutoScrollPosition = Point.Empty;
        float dpi = DeviceDpi / 96F;
        int width = Math.Max(300, Math.Min((int)(760 * dpi), ClientSize.Width - (int)(48 * dpi)));
        using var bitmap = new Bitmap(760, 1000); using var graphics = Graphics.FromImage(bitmap);
        int count = paper.Document?.PageCount(graphics) ?? 1;
        paper.SetBounds(Math.Max(12, (ClientSize.Width - SystemInformation.VerticalScrollBarWidth - width) / 2), 16, width, (int)(width * 1000F / 760 * count) + 20 * (count - 1));
        AutoScrollMinSize = new Size(0, paper.Bottom + 16);
        paper.Invalidate();
    }
    private sealed class Paper : Control
    {
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public ReceiptDocument? Document { get; set; }
        public Paper() { DoubleBuffered = true; AccessibleName = "Order receipt preview"; }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e); if (Document == null) return;
            using var bitmap = new Bitmap(760, 1000); using var graphics = Graphics.FromImage(bitmap);
            int count = Document.PageCount(graphics); float height = Width * 1000F / 760;
            for (int i = 0; i < count; i++) Document.DrawPage(e.Graphics, new RectangleF(0, i * (height + 20), Width, height), i);
        }
    }
}
