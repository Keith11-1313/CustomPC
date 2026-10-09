namespace CustomPC;

public partial class StockAdjustmentForm : Form
{
    private string? partId;

    public StockAdjustmentForm()
    {
        InitializeComponent();
        if (Ui.IsDesign)
        {
            return;
        }

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
