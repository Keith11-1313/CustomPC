namespace CustomPC;

public partial class StockAdjustmentForm : Form
{
    private string? partId;
    public StockAdjustmentForm()
    {
        InitializeComponent(); if (Ui.IsDesign) return;
        save.Click += (_, _) => Ui.Run(this, () => { if (partId == null) throw new InvalidOperationException("Select a part."); AppStore.Current.AdjustStock(partId, (int)quantity.Value, reason.Text); DialogResult = DialogResult.OK; });
    }
    public StockAdjustmentForm(string id) : this() { partId = id; partLabel.Text = AppStore.Current.Parts.Single(x => x.Id == id).Name; }
}
