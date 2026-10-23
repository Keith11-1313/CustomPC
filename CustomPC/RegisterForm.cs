namespace CustomPC;

public partial class RegisterForm : StoreWindow
{
    public RegisterForm()
    {
        InitializeComponent();
        back.Click += BackButton_Click;

        if (Ui.IsDesign)
        {
            return;
        }

        AcceptButton = save;
        var sheet = new EditorSheet { Name = "editorSheet" };
        sheet.Bind(this, heading, back, save, "Create your customer account to save and order a PC build.", false,
            (fullNameLabel, fullName), (emailLabel, email), (passwordLabel, password), (confirmLabel, confirm));
        var policies = new FlowLayoutPanel { WrapContents = true };
        policies.Controls.Add(ShopPolicies.Link("Privacy Notice", "privacy", this));
        policies.Controls.Add(ShopPolicies.Link("Terms & Conditions", "terms", this));
        sheet.AddOption(policies);
        save.Click += SaveButton_Click;
    }

    private void BackButton_Click(object? sender, EventArgs e)
    {
        // Return to the previous screen without saving changes.
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        try
        {
            if (password.Text != confirm.Text)
            {
                throw new InvalidOperationException("Passwords do not match.");
            }

            UserAccount newCustomer = new UserAccount
            {
                Name = fullName.Text.Trim(),
                Email = email.Text.Trim()
            };

            // Registration always creates a customer account.
            AppStore.Current.SaveUser(newCustomer, password.Text, true);
            Ui.Message(this, "Account created. You can now sign in.", "CustomPC");
            DialogResult = DialogResult.OK;
        }
        catch (Exception exception)
        {
            Ui.ShowError(this, exception);
        }
    }
}
