namespace CustomPC;

public partial class RegisterForm : Form
{
    public RegisterForm()
    {
        InitializeComponent(); if (Ui.IsDesign) return; AcceptButton = save;
        save.Click += (_, _) => Ui.Run(this, () => {
            if (password.Text != confirm.Text) throw new InvalidOperationException("Passwords do not match.");
            AppStore.Current.SaveUser(new() { Name = fullName.Text.Trim(), Email = email.Text.Trim() }, password.Text, true);
            MessageBox.Show(this, "Account created. You can now sign in.", "CustomPC"); DialogResult = DialogResult.OK;
        });
    }
}
