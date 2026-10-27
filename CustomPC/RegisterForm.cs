namespace CustomPC;

public partial class RegisterForm : StoreWindow
{
    private readonly StoreCheckBox policyAgreement = new() { Name = "policyAgreement", WrapText = true,
        Text = "I agree to the Terms & Conditions and acknowledge the Privacy Notice." };
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
        var agreement = new Panel { Name = "agreementRow" };
        var agreementText = new LinkLabel { Name = "agreementText", Text = policyAgreement.Text,
            AutoSize = false, UseMnemonic = false, TextAlign = ContentAlignment.MiddleLeft, Font = AppFonts.Body(10.5F),
            LinkColor = StoreTheme.Action, ActiveLinkColor = StoreTheme.Ink, VisitedLinkColor = StoreTheme.Action };
        agreementText.Links.Clear();
        foreach (var (text, key) in new[] { ("Terms & Conditions", "terms"), ("Privacy Notice", "privacy") })
            agreementText.Links.Add(agreementText.Text.IndexOf(text, StringComparison.Ordinal), text.Length, key);
        agreementText.LinkClicked += (_, args) =>
        {
            using var policy = new PolicyForm((string)args.Link!.LinkData!);
            policy.ShowDialog(this);
        };
        policyAgreement.AccessibleName = policyAgreement.Text;
        policyAgreement.Text = "";
        agreement.Controls.Add(policyAgreement); agreement.Controls.Add(agreementText);
        void FitAgreement()
        {
            int gap = (int)(30 * DeviceDpi / 96F);
            int width = Math.Max(1, agreement.ClientSize.Width - gap);
            int height = Math.Max((int)(44 * DeviceDpi / 96F),
                agreementText.GetPreferredSize(new Size(width, 0)).Height + 8);
            policyAgreement.SetBounds(0, 0, gap - 4, height);
            agreementText.SetBounds(gap, 0, width, height);
            if (agreement.Parent != null && agreement.Parent.Height != height) agreement.Parent.Height = height;
        }
        agreement.SizeChanged += (_, _) => FitAgreement();
        agreementText.FontChanged += (_, _) => FitAgreement();
        sheet.UseCompactSpacing();
        password.MaxLength = confirm.MaxLength = 128;
        sheet.AddFieldHint(password, PasswordGuidance.Requirements);
        sheet.AddOption(agreement);
        FitAgreement();
        ClientSize = new Size(580, 640);
        save.Enabled = false;
        policyAgreement.CheckedChanged += (_, _) => save.Enabled = policyAgreement.Checked;
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
            if (!policyAgreement.Checked)
                throw new InvalidOperationException("Please agree to the Terms & Conditions and acknowledge the Privacy Notice to create your account.");
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
