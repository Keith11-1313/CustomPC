using System.Data;

namespace CustomPC;

public partial class UsersForm : Form
{
    public UsersForm()
    {
        InitializeComponent();
        if (Ui.IsDesign)
        {
            return;
        }

        if (!AppStore.Current.IsAdmin)
        {
            throw new InvalidOperationException("Admin access required.");
        }

        search.TextChanged += FiltersChanged;
        archived.CheckedChanged += FiltersChanged;
        add.Click += AddClicked;
        edit.Click += EditClicked;
        LoadRows();
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
}
