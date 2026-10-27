namespace CustomPC;

public partial class ResetPasswordForm : StoreWindow
{
    private string? userId;
    public ResetPasswordForm()
    {
        InitializeComponent();
        if (Ui.IsDesign) return;
        back.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        password.TextChanged += (_, _) => UpdateAction();
        confirmPassword.TextChanged += (_, _) => UpdateAction();
        identityVerified.CheckedChanged += (_, _) => UpdateAction();
        save.Click += SaveClicked;
        AcceptButton = save; CancelButton = back;
        AppStore.Current.Refresh();
        if (!AppStore.Current.IsAdmin) throw new InvalidOperationException("Administrator access is required.");
    }
    public ResetPasswordForm(string id) : this()
    {
        var account = AppStore.Current.Users.FirstOrDefault(user => user.Id == id)
            ?? throw new InvalidOperationException("The selected account no longer exists.");
        if (account.Archived) throw new InvalidOperationException("Restore this account before resetting its password.");
        userId = id;
        accountSummary.Text = account.Name + "\n" + account.Email;
    }
    private void UpdateAction() => save.Enabled = identityVerified.Checked && password.Text.Length > 0 && confirmPassword.Text.Length > 0;
    private void SaveClicked(object? sender, EventArgs e)
    {
        try
        {
            if (!identityVerified.Checked) throw new InvalidOperationException("Verify the account holder's identity before resetting the password.");
            if (password.Text != confirmPassword.Text) throw new InvalidOperationException("Passwords do not match.");
            if (userId == null) throw new InvalidOperationException("Choose an account first.");
            AppStore.Current.ResetUserPassword(userId, password.Text);
            password.Clear(); confirmPassword.Clear();
            Ui.Message(this, "Password reset. The account holder can now sign in with the new password.", "Password reset");
            DialogResult = DialogResult.OK;
        }
        catch (Exception error) { Ui.ShowError(this, error); }
    }
}
