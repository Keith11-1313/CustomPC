namespace CustomPC;

public partial class RegisterForm : Form
{
    public RegisterForm()
    {
        InitializeComponent();

        if (Ui.IsDesign)
        {
            return;
        }

        AcceptButton = save;
        save.Click += SaveButton_Click;
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
            MessageBox.Show(this, "Account created. You can now sign in.", "CustomPC");
            DialogResult = DialogResult.OK;
        }
        catch (Exception exception)
        {
            Ui.ShowError(this, exception);
        }
    }
}
