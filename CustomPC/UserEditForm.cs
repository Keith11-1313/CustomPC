namespace CustomPC;

public partial class UserEditForm : Form
{
    private string? userId;

    public UserEditForm()
    {
        InitializeComponent();
        if (Ui.IsDesign)
        {
            return;
        }

        role.Items.AddRange(AppStore.Roles);
        role.SelectedItem = "Customer";
        save.Click += SaveClicked;
    }

    public UserEditForm(string id) : this()
    {
        userId = id;
        UserAccount? user = null;
        foreach (UserAccount savedUser in AppStore.Current.Users)
        {
            if (savedUser.Id == id)
            {
                user = savedUser;
                break;
            }
        }

        if (user == null)
        {
            throw new InvalidOperationException("The selected user no longer exists.");
        }

        fullName.Text = user.Name;
        email.Text = user.Email;
        role.SelectedItem = user.Role;
        archived.Checked = user.Archived;
    }

    private void SaveClicked(object? sender, EventArgs e)
    {
        try
        {
            UserAccount user = new UserAccount();
            user.Name = fullName.Text.Trim();
            user.Email = email.Text.Trim();
            user.Role = role.Text;
            user.Archived = archived.Checked;

            if (userId != null)
            {
                user.Id = userId;
            }

            AppStore.Current.SaveUser(user, password.Text);
            DialogResult = DialogResult.OK;
        }
        catch (Exception exception)
        {
            Ui.ShowError(this, exception);
        }
    }
}
