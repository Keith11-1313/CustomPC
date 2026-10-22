namespace CustomPC;

public partial class StockAdjustmentForm : StoreWindow
{
    private string? partId;

    public StockAdjustmentForm()
    {
        InitializeComponent();
        back.Click += BackButton_Click;
        if (Ui.IsDesign)
        {
            return;
        }

        var sheet = new EditorSheet { Name = "editorSheet" };
        sheet.Bind(this, heading, back, save, "Record a stock receipt or correction with a reason.", false,
            (quantityLabel, quantity), (reasonLabel, reason));
        sheet.AddOption(partLabel);
        AppStore.Current.Refresh();
        if (!AppStore.Current.IsStaff)
            throw new InvalidOperationException("Staff access is required.");

        save.Click += SaveClicked;
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

            int stockChange = (int)quantity.Value;
            AppStore.Current.AdjustStock(partId, stockChange, reason.Text);
            DialogResult = DialogResult.OK;
        }
        catch (Exception exception)
        {
            Ui.ShowError(this, exception);
        }
    }
}
