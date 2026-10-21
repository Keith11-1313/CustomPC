namespace CustomPC;

// Product details are presented as labelled attributes, using only saved product data.
public partial class ProductDetailsForm : StoreWindow
{
    public ProductDetailsForm()
    {
        InitializeComponent();
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
