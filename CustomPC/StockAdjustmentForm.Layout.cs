namespace CustomPC;

public partial class StockAdjustmentForm
{
    private readonly ProductThumbnail productPhoto = new();
    private readonly Label stockSnapshot = new() { AutoSize = true, ForeColor = StoreTheme.Muted };
    private readonly Label resultingStock = new() { AutoSize = true, ForeColor = StoreTheme.Action };
    private readonly Button addStock = new() { Text = "Add stock", BackColor = StoreTheme.Action };
    private readonly Button removeStock = new() { Text = "Remove stock", BackColor = Color.White };

    private void BuildAdjustmentLayout()
    {
        SuspendLayout();
        Controls.Clear();
        AutoScroll = false;
        ClientSize = new Size(600, 740);
        MinimumSize = new Size(480, 560);
        MinimizeBox = MaximizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(24, 16, 24, 0), BackColor = Color.White };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
        heading.Text = "Adjust stock";
        heading.Font = AppFonts.Heading(23);
        heading.AutoSize = true;
        heading.BackColor = Color.White;
        heading.Margin = new Padding(0, 0, 0, 20);
        root.Controls.Add(heading, 0, 0);

        var viewport = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Margin = Padding.Empty };
        var body = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, Padding = new Padding(0, 0, 8, 0), Margin = Padding.Empty };
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        void Row(Control control, int height = 0)
        {
            int row = body.RowCount++;
            body.RowStyles.Add(new RowStyle(height == 0 ? SizeType.AutoSize : SizeType.Absolute, height));
            control.Dock = DockStyle.Fill;
            body.Controls.Add(control, 0, row);
        }
        body.RowCount = 0;
        var product = new TableLayoutPanel { ColumnCount = 2, RowCount = 2, Padding = new Padding(12), BackColor = StoreTheme.Background, Margin = new Padding(0, 0, 0, 20) };
        product.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88));
        product.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        product.RowStyles.Add(new RowStyle(SizeType.Percent, 60)); product.RowStyles.Add(new RowStyle(SizeType.Percent, 40));
        productPhoto.Dock = DockStyle.Fill; productPhoto.Margin = new Padding(0, 0, 12, 0);
        product.Controls.Add(productPhoto, 0, 0); product.SetRowSpan(productPhoto, 2);
        partLabel.Dock = DockStyle.Fill; partLabel.Font = AppFonts.Body(12, FontStyle.Bold); partLabel.BackColor = Color.Transparent;
        stockSnapshot.Dock = DockStyle.Fill; stockSnapshot.AutoSize = false; stockSnapshot.Font = AppFonts.Body(9);
        product.Controls.Add(partLabel, 1, 0); product.Controls.Add(stockSnapshot, 1, 1);
        Row(product, 112);
        void FitProduct()
        {
            int width = Math.Max(1, product.ClientSize.Width - product.Padding.Horizontal - 100);
            int nameHeight = TextRenderer.MeasureText(partLabel.Text, partLabel.Font, new Size(width, int.MaxValue), TextFormatFlags.WordBreak).Height + 8;
            int snapshotHeight = TextRenderer.MeasureText(stockSnapshot.Text, stockSnapshot.Font, new Size(width, int.MaxValue), TextFormatFlags.WordBreak).Height + 8;
            product.RowStyles[0].SizeType = product.RowStyles[1].SizeType = SizeType.Absolute;
            product.RowStyles[0].Height = nameHeight;
            product.RowStyles[1].Height = snapshotHeight;
            body.RowStyles[0].Height = Math.Max(112, nameHeight + snapshotHeight + product.Padding.Vertical + product.Margin.Vertical);
        }
        product.SizeChanged += (_, _) => FitProduct();
        partLabel.TextChanged += (_, _) => FitProduct();
        stockSnapshot.TextChanged += (_, _) => FitProduct();

        var modes = new TableLayoutPanel { ColumnCount = 2, RowCount = 1, Margin = new Padding(0, 0, 0, 18) };
        modes.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50)); modes.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        addStock.Dock = removeStock.Dock = DockStyle.Fill;
        addStock.Margin = new Padding(0, 0, 6, 0); removeStock.Margin = new Padding(6, 0, 0, 0);
        modes.Controls.Add(addStock, 0, 0); modes.Controls.Add(removeStock, 1, 0);
        addStock.Click += (_, _) => adjustmentType.SelectedIndex = 0;
        removeStock.Click += (_, _) => adjustmentType.SelectedIndex = 1;
        Row(modes, 64);
        quantityLabel.AutoSize = true; quantityLabel.BackColor = Color.White; quantityLabel.Margin = new Padding(0, 0, 0, 6);
        Row(quantityLabel);
        quantity.Margin = new Padding(0, 0, 0, 18); Row(quantity, 62);
        reasonLabel.Text = "Reason / reference"; reasonLabel.AutoSize = true; reasonLabel.BackColor = Color.White; reasonLabel.Margin = new Padding(0, 0, 0, 6);
        Row(reasonLabel);
        reason.PlaceholderText = "e.g. Delivery received or damaged item";
        reason.Margin = new Padding(0, 0, 0, 20); Row(reason, 64);

        var preview = new TableLayoutPanel { ColumnCount = 1, RowCount = 3, AutoSize = true, BackColor = StoreTheme.Selection, Padding = new Padding(16), Margin = new Padding(0, 0, 0, 12) };
        preview.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        preview.Controls.Add(new Label { Text = "Stock after adjustment", AutoSize = true, ForeColor = StoreTheme.Muted }, 0, 0);
        resultingStock.Font = AppFonts.Heading(22); preview.Controls.Add(resultingStock, 0, 1);
        stockPreview.AutoSize = true; stockPreview.Font = AppFonts.Body(9); preview.Controls.Add(stockPreview, 0, 2);
        preview.Dock = DockStyle.Fill;
        root.Controls.Add(preview, 0, 2);
        viewport.Controls.Add(body); root.Controls.Add(viewport, 0, 1);

        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, Padding = new Padding(0, 16, 0, 0), Margin = Padding.Empty };
        back.Text = "Cancel"; back.Margin = new Padding(0, 0, 12, 0); save.Margin = Padding.Empty;
        actions.Controls.Add(save); actions.Controls.Add(back);
        void FitActions()
        {
            foreach (Button button in new[] { back, save })
            {
                button.Width = TextRenderer.MeasureText(button.Text, button.Font).Width + (int)(64 * DeviceDpi / 96F);
                button.Height = (int)(42 * DeviceDpi / 96F);
            }
        }
        save.TextChanged += (_, _) => FitActions();
        Load += (_, _) => FitActions();
        root.Controls.Add(actions, 0, 3); Controls.Add(root);
        viewport.SizeChanged += (_, _) => body.Width = Math.Max(1, viewport.ClientSize.Width - SystemInformation.VerticalScrollBarWidth);
        ResumeLayout(true);
        AcceptButton = save;
    }
}
