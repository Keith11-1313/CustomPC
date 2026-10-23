namespace CustomPC;

public partial class RestorePasswordForm : StoreWindow
{
    internal string Password => password.Text;
    public RestorePasswordForm()
    {
        InitializeComponent();
        if (Ui.IsDesign) return;
        confirm.Click += (_, _) =>
        {
            try
            {
                AppStore.Current.VerifyAdministratorPassword(password.Text);
                DialogResult = DialogResult.OK;
            }
            catch (Exception error)
            {
                password.Clear(); password.Focus();
                failure.Text = error.Message;
            }
        };
        password.TextChanged += (_, _) => { confirm.Enabled = password.Text.Length > 0; failure.Text = ""; };
    }
}
