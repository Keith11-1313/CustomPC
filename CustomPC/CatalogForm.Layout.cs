namespace CustomPC;

public partial class CatalogForm
{
    private TableLayoutPanel? browseLayout;
    private FlowLayoutPanel? categoryRail;
    private WorkspacePage? catalogWorkspace;
    private readonly List<Button> categoryButtons = new();

    private void BuildBrowseWorkspace()
    {
        browseLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = new Padding(0) };
        browseLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
        browseLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        browseLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        categoryRail = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Padding = new Padding(0, 8, 12, 0), BackColor = StoreTheme.Background };
        for (int i = 0; i < category.Items.Count; i++)
        {
            int index = i;
            Button button = new() { Name = "categoryFilter", Text = category.Items[i]?.ToString(), Width = 160, Height = 42, Margin = new Padding(0, 0, 0, 6), BackColor = Color.White, AccessibleName = "Filter " + category.Items[i] };
            button.Click += (_, _) => category.SelectedIndex = index;
            categoryButtons.Add(button); categoryRail.Controls.Add(button);
        }
        Panel body = new() { Dock = DockStyle.Fill, BackColor = StoreTheme.Background };
        products.Parent = body; products.Dock = DockStyle.Fill; products.Padding = new Padding(4, 0, 4, 16);
        emptyProducts.Parent = body; emptyProducts.Dock = DockStyle.Fill;
        browseLayout.Controls.Add(categoryRail, 0, 0); browseLayout.Controls.Add(body, 1, 0);
        var workspace = new WorkspacePage { Name = "workspace" };
        catalogWorkspace = workspace;
        workspace.Bind(this, heading, back, "Find the components for your next build. Explore categories or search exact models.", browseLayout,
            refresh, Array.Empty<Button>(), (searchLabel, search), (availabilityLabel, availability));
        browseLayout.SizeChanged += (_, _) => FitBrowseWorkspace();
        FitBrowseWorkspace();
    }

    private void FitBrowseWorkspace()
    {
        if (browseLayout == null || categoryRail == null) return;
        bool compact = browseLayout.ClientSize.Width < 760 * DeviceDpi / 96F;
        browseLayout.SuspendLayout();
        Control body = products.Parent!;
        browseLayout.SetColumnSpan(body, 1);
        browseLayout.ColumnStyles[0].Width = compact ? 0 : 180 * DeviceDpi / 96F;
        browseLayout.RowCount = compact ? 2 : 1;
        browseLayout.RowStyles.Clear();
        float scale = DeviceDpi / 96F;
        int tabWidth = (int)(160 * scale);
        int columns = Math.Max(1, (browseLayout.ClientSize.Width - categoryRail.Padding.Horizontal) / tabWidth);
        int rows = (int)Math.Ceiling(categoryButtons.Count / (double)columns);
        if (compact) browseLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, rows * 48 * scale + categoryRail.Padding.Vertical));
        browseLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        browseLayout.SetCellPosition(body, new TableLayoutPanelCellPosition(1, compact ? 1 : 0));
        browseLayout.SetColumnSpan(categoryRail, compact ? 2 : 1);
        categoryRail.FlowDirection = compact ? FlowDirection.LeftToRight : FlowDirection.TopDown;
        categoryRail.WrapContents = compact;
        foreach (Button button in categoryButtons) button.Width = tabWidth;
        browseLayout.ResumeLayout(true);
        FitProductCards();
    }

    private void UpdateCategorySelection()
    {
        for (int i = 0; i < categoryButtons.Count; i++)
        {
            categoryButtons[i].BackColor = i == category.SelectedIndex ? StoreTheme.Selection : Color.White;
            categoryButtons[i].AccessibleDescription = i == category.SelectedIndex ? "Selected category" : "";
            categoryButtons[i].Invalidate();
        }
    }
}
