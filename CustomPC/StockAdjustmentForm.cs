namespace CustomPC;

public partial class StockAdjustmentForm : StoreWindow
{
    private string? partId;
    private readonly StoreComboBox adjustmentType = new() { Name = "adjustmentType" };
    private readonly Label stockPreview = new() { Name = "stockPreview", ForeColor = StoreTheme.Action };

    public StockAdjustmentForm()
    {
        InitializeComponent();
        back.Click += BackButton_Click;
        if (Ui.IsDesign)
        {
            return;
        }

        adjustmentType.Items.AddRange(new object[] { "Add stock", "Remove stock" });
        adjustmentType.SelectedIndex = 0;
        quantity.Minimum = 1;
        quantity.Maximum = 10000000;
        quantity.Value = 1;
        BuildAdjustmentLayout();
        adjustmentType.SelectedIndexChanged += (_, _) => UpdatePreview();
        quantity.ValueChanged += (_, _) => UpdatePreview();
        reason.TextChanged += (_, _) => UpdatePreview();
        AppStore.Current.Refresh();
        if (!AppStore.Current.IsStaff)
            throw new InvalidOperationException("Staff access is required.");

        save.Click += SaveClicked;
        UpdatePreview();
    }

    public StockAdjustmentForm(string id) : this()
    {
        partId = id;
        Part? selectedPart = null;
        foreach (Part part in AppStore.Current.Parts)
        {
            if (part.Id == id)
            {
                selectedPart = part;
                break;
            }
        }

        if (selectedPart == null)
        {
            throw new InvalidOperationException("The selected part no longer exists.");
        }

        partLabel.Text = selectedPart.Name;
        productPhoto.Display(selectedPart, AppStore.Current.DatabasePath);
        stockSnapshot.Text = $"On hand  {selectedPart.Stock:N0}     Reserved  {AppStore.Current.Reserved(id):N0}     Available  {AppStore.Current.Available(selectedPart):N0}";
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        bool remove = adjustmentType.SelectedIndex == 1;
        addStock.BackColor = !remove ? StoreTheme.Action : Color.White;
        removeStock.BackColor = remove ? StoreTheme.Action : Color.White;
        addStock.AccessibleDescription = !remove ? "Selected" : "";
        removeStock.AccessibleDescription = remove ? "Selected" : "";
        addStock.Invalidate(); removeStock.Invalidate();
        quantityLabel.Text = remove ? "Quantity to remove" : "Quantity to add";
        save.Text = remove ? "Remove stock" : "Add stock";
        Part? part = AppStore.Current.Parts.FirstOrDefault(part => part.Id == partId);
        if (part == null) { save.Enabled = false; return; }
        long result = part.Stock + (remove ? -(long)quantity.Value : (long)quantity.Value);
        bool valid = result >= AppStore.Current.Reserved(part.Id) && result <= 10000000;
        stockPreview.Text = valid ? $"{part.Stock:N0} {(remove ? "−" : "+")} {quantity.Value:N0} = {result:N0}"
            : result < AppStore.Current.Reserved(part.Id) ? "You cannot remove stock reserved for orders." : "Stock cannot exceed 10,000,000 units.";
        stockPreview.ForeColor = valid ? StoreTheme.Action : Color.Firebrick;
        resultingStock.Text = valid ? $"{result:N0} units" : "Check quantity";
        resultingStock.ForeColor = stockPreview.ForeColor;
        save.Enabled = valid && quantity.Value > 0 && !string.IsNullOrWhiteSpace(reason.Text);
    }

    private void BackButton_Click(object? sender, EventArgs e)
    {
        // Return to the previous screen without saving changes.
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private void SaveClicked(object? sender, EventArgs e)
    {
        try
        {
            if (partId == null)
            {
                throw new InvalidOperationException("Select a part.");
            }

            int stockChange = (int)quantity.Value * (adjustmentType.SelectedIndex == 1 ? -1 : 1);
            AppStore.Current.AdjustStock(partId, stockChange, reason.Text);
            DialogResult = DialogResult.OK;
        }
        catch (Exception exception)
        {
            Ui.ShowError(this, exception);
        }
    }
}
