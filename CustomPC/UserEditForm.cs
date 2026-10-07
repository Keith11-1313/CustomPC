namespace CustomPC;

public partial class UserEditForm : Form
{
    private string? userId;
    public UserEditForm()
    {
        InitializeComponent(); if (Ui.IsDesign) return;
        role.Items.AddRange(AppStore.Roles); role.SelectedItem = "Customer";
        save.Click += (_, _) => Ui.Run(this, () => { var u = new UserAccount { Name = fullName.Text.Trim(), Email = email.Text.Trim(), Role = role.Text, Archived = archived.Checked }; if (userId != null) u.Id = userId; AppStore.Current.SaveUser(u, password.Text); DialogResult = DialogResult.OK; });
    }
    public UserEditForm(string id) : this() { userId = id; var u = AppStore.Current.Users.Single(x => x.Id == id); fullName.Text = u.Name; email.Text = u.Email; role.SelectedItem = u.Role; archived.Checked = u.Archived; }
}
