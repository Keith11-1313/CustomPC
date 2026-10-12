namespace CustomPC;

public partial class SetupForm : StoreWindow
{
    public SetupForm()
    {
        InitializeComponent();
        if (Ui.IsDesign) return;
        save.Click += SaveButton_Click;
        back.Click += BackButton_Click;
    }

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        try
        {
            if (password.Text != confirm.Text) throw new InvalidOperationException("Passwords do not match.");
            AppStore.Current.CompleteSetup(storeName.Text, fullName.Text, email.Text, password.Text);
            DialogResult = DialogResult.OK;
        }
        catch (Exception exception) { Ui.ShowError(this, exception); }
    }

    private void BackButton_Click(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }
}
