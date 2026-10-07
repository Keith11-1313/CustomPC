namespace CustomPC;

public partial class LoginForm : Form
{
    public LoginForm()
    {
        InitializeComponent();
        if (Ui.IsDesign) return;
        AcceptButton = login;
        login.Click += (_, _) => Ui.Run(this, () => { AppStore.Current.Login(email.Text, password.Text); DialogResult = DialogResult.OK; });
        register.Click += (_, _) => { using var f = new RegisterForm(); f.ShowDialog(this); };
        guest.Click += (_, _) => { AppStore.Current.Logout(); DialogResult = DialogResult.OK; };
    }
}
