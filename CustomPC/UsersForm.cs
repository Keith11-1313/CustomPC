using System.Data;

namespace CustomPC;

public partial class UsersForm : StoreWindow
{
    private readonly Button resetPassword = new() { Name = "resetPassword", Text = "Reset password", BackColor = Color.White };
    public UsersForm()
    {
        InitializeComponent();
        back.Click += BackButton_Click;
        if (Ui.IsDesign)
        {
            return;
        }

        var workspace = new WorkspacePage { Name = "workspace" };
        workspace.Bind(this, heading, back, "Manage store access and account roles.", grid,
            null, new[] { add, edit, resetPassword }, (searchLabel, search));
        workspace.AddFilterOption(archived);

        if (!AppStore.Current.IsAdmin)
        {
            throw new InvalidOperationException("Admin access required.");
        }

        _ = new AutomaticViewRefresh(this, () => FiltersChanged(this, EventArgs.Empty));
        search.TextChanged += FiltersChanged;
        archived.CheckedChanged += FiltersChanged;
        add.Click += AddClicked;
        edit.Click += EditClicked;
        resetPassword.Click += (_, _) =>
        {
            try
            {
                AppStore.Current.Refresh(); ValidateAccess(AppStore.Current);
                using var dialog = new ResetPasswordForm(Ui.RequireSelection(grid));
                dialog.ShowDialog(this);
                LoadRows();
            }
            catch (Exception error) { Ui.ShowError(this, error); }
        };
        LoadRows();
    }

    private void BackButton_Click(object? sender, EventArgs e)
    {
        // Closing this screen returns to the window that opened it.
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private void FiltersChanged(object? sender, EventArgs e)
    {
        try
        {
            LoadRows();
        }
        catch (Exception exception)
        {
            Ui.ShowError(this, exception);
        }
    }

    private void AddClicked(object? sender, EventArgs e)
    {
        try
        {
            AppStore.Current.Refresh();
            ValidateAccess(AppStore.Current);
            using UserEditForm userForm = new UserEditForm();
            userForm.ShowDialog(this);
            LoadRows();
        }
        catch (Exception exception)
        {
            Ui.ShowError(this, exception);
        }
    }

    private void EditClicked(object? sender, EventArgs e)
    {
        try
        {
            AppStore.Current.Refresh();
            ValidateAccess(AppStore.Current);
            string selectedUserId = Ui.RequireSelection(grid);
            using UserEditForm userForm = new UserEditForm(selectedUserId);
            userForm.ShowDialog(this);
            LoadRows();
        }
        catch (Exception exception)
        {
            Ui.ShowError(this, exception);
        }
    }

    private void LoadRows()
    {
        AppStore store = AppStore.Current;
        store.Refresh();
        ValidateAccess(store);
        string searchText = search.Text.Trim();

        DataTable table = new DataTable();
        table.Columns.Add("Id");
        table.Columns.Add("Name");
        table.Columns.Add("Email");
        table.Columns.Add("Role");
        table.Columns.Add("Archived", typeof(bool));

        foreach (UserAccount user in store.Users)
        {
            if (user.Archived && !archived.Checked)
            {
                continue;
            }

            string searchableText = user.Name + " " + user.Email;
            if (!searchableText.Contains(searchText, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            table.Rows.Add(user.Id, user.Name, user.Email, user.Role, user.Archived);
        }

        Ui.Bind(grid, table);
    }

    private void ValidateAccess(AppStore store)
    {
        add.Enabled = store.IsAdmin;
        edit.Enabled = store.IsAdmin;
        resetPassword.Enabled = store.IsAdmin;

        if (!store.IsAdmin)
        {
            grid.DataSource = null;
            throw new InvalidOperationException("Your account no longer has admin access. Close this screen and sign in with an administrator account.");
        }
    }
}
