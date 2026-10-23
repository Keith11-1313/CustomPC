namespace CustomPC;

// Product details are presented as labelled attributes, using only saved product data.
public partial class ProductDetailsForm : StoreWindow
{
    private bool? compactSpecifications;
    private readonly FlowLayoutPanel detailActions = new() { Dock = DockStyle.Fill,
        FlowDirection = FlowDirection.RightToLeft, WrapContents = true, Margin = new Padding(0) };
    public ProductDetailsForm()
    {
        InitializeComponent();
        layout.Controls.Remove(close);
        close.Anchor = AnchorStyles.None; close.Margin = new Padding(8, 0, 0, 0);
        detailActions.Controls.Add(close);
        detailActions.Controls.Add(ShopPolicies.Link("Warranty & Returns", "warranty", this));
        layout.Controls.Add(detailActions, 1, 3);
        MaximizeBox = false; MinimizeBox = false; ShowInTaskbar = false;
        MinimumSize = new Size(480, 560);
        SizeChanged += (_, _) => FitSpecifications();
        Shown += (_, _) => FitSpecifications();
        Disposed += (_, _) => photo.Image?.Dispose();
        Load += (_, _) =>
        {
            specifications.Columns[0].FillWeight = 45;
            specifications.Columns[1].FillWeight = 55;
            specifications.Columns[0].DefaultCellStyle.Font = new Font("Roboto", 10F, FontStyle.Bold);
            specifications.DefaultCellStyle.Padding = new Padding(12, 10, 12, 10);
            specifications.DefaultCellStyle.SelectionBackColor = Color.FromArgb(239, 245, 236);
            specifications.DefaultCellStyle.SelectionForeColor = StoreTheme.Ink;
            specifications.ClearSelection();
        };
    }

    private void FitSpecifications()
    {
        bool compact = ClientSize.Width < 760 * DeviceDpi / 96F;
        if (compactSpecifications == compact) return;
        compactSpecifications = compact;
        layout.SuspendLayout();
        foreach (Control control in new Control[] { heading, photo, summary, specifications, detailActions })
        {
            layout.SetColumnSpan(control, 1); layout.SetRowSpan(control, 1);
        }
        layout.RowStyles.Clear(); layout.RowCount = compact ? 5 : 4;
        layout.ColumnStyles[0].Width = compact ? 100 : 36;
        layout.ColumnStyles[1].Width = compact ? 0 : 64;
        layout.Controls.Remove(heading); layout.Controls.Remove(photo); layout.Controls.Remove(summary); layout.Controls.Remove(specifications); layout.Controls.Remove(detailActions);
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 96));
        if (compact)
        {
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 150)); layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
            layout.Controls.Add(heading, 0, 0); layout.Controls.Add(photo, 0, 1); layout.Controls.Add(summary, 0, 2); layout.Controls.Add(specifications, 0, 3); layout.Controls.Add(detailActions, 0, 4);
        }
        else
        {
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56)); layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 88));
            layout.Controls.Add(heading, 0, 0); layout.SetColumnSpan(heading, 2); layout.Controls.Add(photo, 0, 1); layout.SetRowSpan(photo, 2);
            layout.Controls.Add(specifications, 1, 1); layout.SetRowSpan(specifications, 2); layout.Controls.Add(summary, 0, 3); layout.Controls.Add(detailActions, 1, 3);
        }
        layout.ResumeLayout(true);
    }

    public ProductDetailsForm(Part part) : this()
    {
        heading.Text = part.Name;
        int available = AppStore.Current.Available(part);
        summary.Text = $"{Ui.Money(part.Price)}\n" + (available > 0 ? $"In stock - {available} units" : "Out of stock");
        photo.Image = ProductPhotos.Read(AppStore.Current.DatabasePath, part.PhotoFileName);
        photo.AccessibleName = photo.Image == null ? "Product photo unavailable" : part.Name;
        foreach (var row in ProductSpecifications.Rows(part)) specifications.Rows.Add(row.Key, DisplayValue(row.Key, row.Value));
    }
    private static string DisplayValue(string label, string value)
    {
        if (label == "Recommended power supply")
            return value.Replace("recommended PSU", "", StringComparison.OrdinalIgnoreCase).Trim();
        if (label is "Base frequency" or "Maximum boost frequency" or "Card length" or "Maximum turbo power")
            return System.Text.RegularExpressions.Regex.Replace(value,
                @"\s+(base|boost|turbo|length|maximum turbo power)$", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        return value;
    }

}
