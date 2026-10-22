namespace CustomPC;

public partial class DashboardForm
{
    private Panel? overviewViewport;
    private Panel? overviewCanvas;
    private Label? navigationCaption;
    private Label? accountName;
    private Label? accountRole;
    private Panel? navigationList;
    private FlowLayoutPanel? shoppingActions;
    private readonly ToolTip navigationTips = new();
    private bool fittingWorkspace;

    private static void PaintMetric(object? sender, PaintEventArgs e)
    {
        if (sender is not Label label) return;
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        using SolidBrush background = new(Color.White);
        e.Graphics.FillRectangle(background, label.ClientRectangle);
        string[] lines = label.Text.Split('\n', 2);
        Font valueFont = AppFonts.Heading(label.Width < 240 ? 20 : 23);
        while (valueFont.Size > 14 && TextRenderer.MeasureText(lines[0], valueFont).Width > label.Width - 40)
        {
            float smaller = valueFont.Size - 1;
            valueFont.Dispose(); valueFont = AppFonts.Heading(smaller);
        }
        using Font captionFont = AppFonts.Body(10);
        using (valueFont)
            TextRenderer.DrawText(e.Graphics, lines[0], valueFont, new Rectangle(20, 17, Math.Max(1, label.Width - 40), 40), StoreTheme.Ink,
                TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        TextRenderer.DrawText(e.Graphics, lines.Length > 1 ? lines[1] : "", captionFont, new Rectangle(20, 60, Math.Max(1, label.Width - 40), 26), StoreTheme.Muted,
            TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
    }

    private void BuildAdaptiveShell()
    {
        MinimumSize = new Size(720, 600);
        sidebar.BackColor = brand.BackColor = brandCaption.BackColor = logo.BackColor = StoreTheme.Ink;
        overviewViewport = new Panel { Name = "overviewViewport", AutoScroll = true, BackColor = StoreTheme.Background };
        overviewCanvas = new Panel { Name = "overviewCanvas", BackColor = StoreTheme.Background };
        overviewViewport.Controls.Add(overviewCanvas);
        Control[] overviewControls = { heading, identity, period, chartSummary, adminSummary, stockAttention,
            chartArea, chartActions, summarySurface, workflowSurface, recentTitle, recent, emptyRecent };
        foreach (Control control in overviewControls) { control.Parent = overviewCanvas; control.Anchor = AnchorStyles.Top | AnchorStyles.Left; }
        Controls.Add(overviewViewport);
        navigationCaption = new Label { Text = "Workspace", ForeColor = Color.FromArgb(151, 172, 160), BackColor = StoreTheme.Ink, Font = AppFonts.Body(9, FontStyle.Bold) };
        accountName = new Label { ForeColor = Color.White, BackColor = StoreTheme.Ink, Font = AppFonts.Body(11, FontStyle.Bold), AutoEllipsis = true };
        accountRole = new Label { ForeColor = Color.FromArgb(175, 195, 183), BackColor = StoreTheme.Ink, Font = AppFonts.Body(9) };
        sidebar.Controls.Add(navigationCaption); sidebar.Controls.Add(accountName); sidebar.Controls.Add(accountRole);
        navigationList = new Panel { Name = "navigationList", AutoScroll = true, BackColor = StoreTheme.Ink };
        sidebar.Controls.Add(navigationList);
        foreach (Button button in new[] { home, catalog, builder, orders, inventory, users, reports, tools }) button.Parent = navigationList;
        foreach (Button button in new[] { home, catalog, builder, orders, inventory, users, reports, tools, signout })
        {
            button.AccessibleName = button.Text;
            navigationTips.SetToolTip(button, button.Text);
            button.GotFocus += (_, _) =>
            {
                if (sidebar.Width < 100 * DeviceDpi / 96F) navigationTips.Show(button.Text, button, button.Width + 8, 0, 3000);
            };
            button.LostFocus += (_, _) => navigationTips.Hide(button);
        }
        Disposed += (_, _) => navigationTips.Dispose();
        heading.Font = AppFonts.Heading(28);
        viewPeriodOrders.Name = "orders"; viewQueueOrders.Name = "orders"; viewStock.Name = "inventory";
        brand.Font = AppFonts.Brand(19);
        workflowTitle.Font = AppFonts.Heading(24);
        foreach (Label value in new[] { adminSales, adminPaidOrders, adminLowStock, adminOutOfStock })
        {
            value.Font = AppFonts.Body(14, FontStyle.Bold);
            value.Paint += PaintMetric;
        }
        shoppingActions = new FlowLayoutPanel { BackColor = StoreTheme.Ink, WrapContents = true };
        var startBuild = new Button { Name = "builder", Text = "Open PC builder", BackColor = StoreTheme.Green, Size = new Size(210, 44) };
        var browse = new Button { Name = "catalog", Text = "Explore parts", BackColor = Color.White, Size = new Size(190, 44) };
        startBuild.Click += (_, _) => OpenScreen("Builder"); browse.Click += (_, _) => OpenScreen("Catalog");
        shoppingActions.Controls.Add(startBuild); shoppingActions.Controls.Add(browse); workflowSurface.Controls.Add(shoppingActions);
        workflowSurface.BackColor = StoreTheme.Ink;
        workflowTitle.BackColor = guide.BackColor = StoreTheme.Ink;
        workflowTitle.ForeColor = Color.White; guide.ForeColor = Color.FromArgb(211, 228, 217);
        FitWorkspace();
    }

    private void LayoutWorkspace()
    {
        if (overviewViewport == null || overviewCanvas == null || fittingWorkspace) return;
        fittingWorkspace = true;
        try
        {
            float scale = DeviceDpi / 96F;
            int Px(int value) => (int)Math.Round(value * scale);
            bool rail = ClientSize.Width < Px(1180);
            int side = Px(rail ? 76 : 232);
            sidebar.SetBounds(0, 0, side, ClientSize.Height);
            pageHost.SetBounds(side, 0, Math.Max(1, ClientSize.Width - side), ClientSize.Height);
            overviewViewport.Bounds = pageHost.Bounds;
            logo.SetBounds(Px(rail ? 18 : 22), Px(24), Px(40), Px(40));
            brand.Visible = brandCaption.Visible = navigationCaption!.Visible = !rail;
            brand.SetBounds(Px(72), Px(24), Px(146), Px(36));
            brandCaption.SetBounds(Px(24), Px(76), side - Px(40), Px(22));
            navigationCaption.SetBounds(Px(24), Px(118), side - Px(40), Px(24));
            int top = Px(rail ? 94 : 154);
            navigationList!.SetBounds(0, top, side, Math.Max(Px(90), ClientSize.Height - top - Px(rail ? 80 : 142)));
            top = 0;
            foreach (Button button in new[] { home, catalog, builder, orders, inventory, users, reports, tools })
            {
                button.Visible = button == home || button.Enabled;
                if (!button.Visible) continue;
                button.SetBounds(Px(rail ? 10 : 14), top, side - Px(rail ? 20 : 28) - (navigationList.VerticalScroll.Visible ? SystemInformation.VerticalScrollBarWidth : 0), Px(44));
                button.AccessibleName = button.Text; navigationTips.SetToolTip(button, button.Text);
                top += Px(52);
            }
            signout.SetBounds(Px(rail ? 10 : 14), ClientSize.Height - Px(60), side - Px(rail ? 20 : 28), Px(42));
            signout.BackColor = StoreTheme.Ink;
            accountName!.Visible = accountRole!.Visible = !rail;
            accountName.Text = AppStore.Current.Session?.Name ?? "Guest";
            accountRole.Text = AppStore.Current.Session?.Role ?? "Shopping preview";
            accountName.SetBounds(Px(24), signout.Top - Px(62), side - Px(40), Px(26));
            accountRole.SetBounds(Px(24), signout.Top - Px(34), side - Px(40), Px(22));
            int canvasWidth = Math.Max(1, overviewViewport.ClientSize.Width - SystemInformation.VerticalScrollBarWidth);
            int x = Px(canvasWidth < Px(700) ? 20 : 32), width = Math.Max(1, canvasWidth - x * 2);
            overviewCanvas.Width = canvasWidth;
            heading.SetBounds(x, Px(28), width - (period.Visible ? Px(195) : 0), Px(52));
            identity.SetBounds(x, Px(87), width, Px(28));
            period.SetBounds(x + width - Px(180), Px(34), Px(180), Px(40));
            chartSummary.SetBounds(x, Px(124), width, Px(42));
            bool operational = AppStore.Current.IsAdmin || AppStore.Current.IsStaff;
            int y = operational ? Px(180) : Px(136);
            if (operational)
            {
                bool two = width < Px(900);
                adminSummary.SuspendLayout(); adminSummary.ColumnCount = two ? 2 : 4; adminSummary.RowCount = two ? 2 : 1;
                adminSummary.ColumnStyles.Clear(); adminSummary.RowStyles.Clear();
                for (int i = 0; i < adminSummary.ColumnCount; i++) adminSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / adminSummary.ColumnCount));
                for (int i = 0; i < adminSummary.RowCount; i++) adminSummary.RowStyles.Add(new RowStyle(SizeType.Percent, 100F / adminSummary.RowCount));
                int index = 0;
                foreach (Label value in new[] { adminSales, adminPaidOrders, adminLowStock, adminOutOfStock })
                {
                    adminSummary.SetCellPosition(value, new TableLayoutPanelCellPosition(index % adminSummary.ColumnCount, index / adminSummary.ColumnCount));
                    value.TextAlign = ContentAlignment.MiddleLeft; value.Padding = new Padding(Px(20), 0, 0, 0); value.Margin = new Padding(0, 0, Px(12), Px(12)); index++;
                }
                adminSummary.SetBounds(x, y, width + Px(12), Px(two ? 212 : 112)); adminSummary.ResumeLayout(true);
                y = adminSummary.Bottom + Px(12);
                if (AppStore.Current.IsAdmin)
                {
                    bool single = width < Px(850);
                    chartArea.SuspendLayout(); chartArea.ColumnCount = single ? 1 : 2; chartArea.RowCount = single ? 3 : 2;
                    chartArea.ColumnStyles.Clear(); chartArea.RowStyles.Clear();
                    for (int i = 0; i < chartArea.ColumnCount; i++) chartArea.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / chartArea.ColumnCount));
                    for (int i = 0; i < chartArea.RowCount; i++) chartArea.RowStyles.Add(new RowStyle(SizeType.Percent, 100F / chartArea.RowCount));
                    chartArea.SetColumnSpan(trendChart, 1);
                    chartArea.SetCellPosition(trendChart, new TableLayoutPanelCellPosition(0, 0));
                    chartArea.SetCellPosition(distributionChart, new TableLayoutPanelCellPosition(0, 1));
                    chartArea.SetCellPosition(stockChart, new TableLayoutPanelCellPosition(single ? 0 : 1, single ? 2 : 1));
                    chartArea.SetColumnSpan(trendChart, single ? 1 : 2);
                    chartArea.SetBounds(x, y, width, Px(single ? 1020 : 660)); chartArea.ResumeLayout(true);
                    y = chartArea.Bottom + Px(16);
                }
                else
                {
                    chartActions.SetBounds(x, y, width, Px(width < Px(850) ? 108 : 54)); y = chartActions.Bottom + Px(12);
                    recentTitle.SetBounds(x, y, width, Px(36)); y += Px(44);
                    recent.SetBounds(x, y, width, Px(300)); emptyRecent.Bounds = recent.Bounds; y += Px(316);
                }
                stockAttention.WrapContents = true;
                stockAttentionText.AutoSize = false;
                stockAttentionText.Size = new Size(Math.Max(Px(220), width - Px(350)), Px(42));
                stockAttention.SetBounds(x, y, width, Px(width < Px(700) ? 120 : 72)); y = stockAttention.Bottom + Px(28);
            }
            else
            {
                workflowSurface.SetBounds(x, y, width, Px(210));
                workflowTitle.Text = "Build a PC. Make it yours.";
                workflowTitle.SetBounds(Px(24), Px(22), width - Px(48), Px(48));
                guide.Text = "Choose compatible parts, reserve your build, and pay at the store.";
                guide.SetBounds(Px(24), Px(76), width - Px(48), Px(40));
                shoppingActions!.SetBounds(Px(24), Px(128), width - Px(48), Px(72));
                y = workflowSurface.Bottom + Px(20);
                Label[] values = { statParts, statOrders, statStock, statSales }; Label[] captions = { statPartsCaption, statOrdersCaption, statStockCaption, statSalesCaption };
                int[] items = AppStore.Current.IsCustomer ? new[] { 0, 1, 2, 3 } : new[] { 0, 2, 3 };
                int columns = width < Px(700) ? 2 : items.Length;
                int rows = (items.Length + columns - 1) / columns;
                summarySurface.SetBounds(x, y, width, Px(70 + rows * 100)); metrics.SetBounds(Px(24), Px(18), width - Px(48), Px(32));
                for (int i = 0; i < items.Length; i++)
                {
                    int item = items[i];
                    values[item].SetBounds(Px(24) + i % columns * width / columns, Px(64 + i / columns * 100), width / columns - Px(24), Px(42));
                    captions[item].SetBounds(values[item].Left, values[item].Bottom + Px(4), values[item].Width, Px(42));
                }
                y = summarySurface.Bottom + Px(20);
                if (AppStore.Current.IsCustomer) { recentTitle.SetBounds(x, y, width, Px(36)); y += Px(44); recent.SetBounds(x, y, width, Px(300)); emptyRecent.Bounds = recent.Bounds; y += Px(320); }
            }
            overviewCanvas.Height = Math.Max(overviewViewport.ClientSize.Height, y);
        }
        finally { fittingWorkspace = false; }
    }
}
