namespace CustomPC;

public partial class LoginForm : Form
{
    public LoginForm()
    {
        InitializeComponent();

        if (Ui.IsDesign)
        {
            return;
        }

        AcceptButton = login;
        login.Click += LoginButton_Click;
        register.Click += RegisterButton_Click;
        guest.Click += GuestButton_Click;
    }

    private void LoginButton_Click(object? sender, EventArgs e)
    {
        try
        {
            AppStore.Current.Login(email.Text, password.Text);
            DialogResult = DialogResult.OK;
        }
        catch (Exception exception)
        {
            Ui.ShowError(this, exception);
        }
    }

    private void RegisterButton_Click(object? sender, EventArgs e)
    {
        using (RegisterForm registrationForm = new RegisterForm())
        {
            registrationForm.ShowDialog(this);
        }
    }

    private void GuestButton_Click(object? sender, EventArgs e)
    {
        AppStore.Current.Logout();
        DialogResult = DialogResult.OK;
    }
}
