namespace CustomPC;

public partial class UserEditForm : StoreWindow
{
    private string? userId;

    public UserEditForm()
    {
        InitializeComponent();
        back.Click += BackButton_Click;
        if (Ui.IsDesign)
        {
            return;
        }

        var sheet = new EditorSheet { Name = "editorSheet" };
        sheet.Bind(this, heading, back, save, "Account identity, role and access.", false,
            (fullNameLabel, fullName), (emailLabel, email), (roleLabel, role), (passwordLabel, password));
        sheet.UseCompactSpacing();
        password.MaxLength = 128;
        sheet.AddFieldHint(password, PasswordGuidance.Requirements);
        sheet.AddOption(archived);
        AppStore.Current.Refresh();
        if (!AppStore.Current.IsAdmin)
            throw new InvalidOperationException("Administrator access is required.");

        role.Items.AddRange(AppStore.Roles);
        role.SelectedItem = "Customer";
        ClientSize = new Size(580, 640);
        save.Click += SaveClicked;
    }

    public UserEditForm(string id) : this()
    {
        userId = id;
        heading.Text = "Edit account";
        Text = "CustomPC | Edit account";
        ClientSize = new Size(580, 560);
        password.Clear(); password.Enabled = false; password.TabStop = false;
        Controls.OfType<EditorSheet>().Single().SetFieldVisible(password, false);
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

    private void BackButton_Click(object? sender, EventArgs e)
    {
        // Return to the previous screen without saving changes.
        DialogResult = DialogResult.Cancel;
        Close();
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

            AppStore.Current.SaveUser(user, userId == null ? password.Text : "");
            DialogResult = DialogResult.OK;
        }
        catch (Exception exception)
        {
            Ui.ShowError(this, exception);
        }
    }
}
