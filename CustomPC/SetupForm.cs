namespace CustomPC;

public partial class SetupForm : StoreWindow
{
    public SetupForm()
    {
        InitializeComponent();
        if (Ui.IsDesign) return;
        var sheet = new EditorSheet { Name = "editorSheet" };
        sheet.Bind(this, heading, back, save, "Create the first administrator for CustomPC.", false,
            (fullNameLabel, fullName), (emailLabel, email), (passwordLabel, password), (confirmLabel, confirm));
        sheet.UseCompactSpacing();
        password.MaxLength = confirm.MaxLength = 128;
        sheet.AddFieldHint(password, PasswordGuidance.Requirements);
        sheet.AddOption(help);
        ClientSize = new Size(580, 640);
        save.Click += SaveButton_Click;
        back.Click += BackButton_Click;
    }

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        try
        {
            if (password.Text != confirm.Text) throw new InvalidOperationException("Passwords do not match.");
            AppStore.Current.CompleteSetup(fullName.Text, email.Text, password.Text);
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
