namespace CustomPC;

public partial class UsersForm : Form
{
    public UsersForm()
    {
        InitializeComponent(); if (Ui.IsDesign) return;
        if (!AppStore.Current.IsAdmin) throw new InvalidOperationException("Admin access required.");
        search.TextChanged += (_, _) => LoadRows(); archived.CheckedChanged += (_, _) => LoadRows();
        add.Click += (_, _) => { using var f = new UserEditForm(); f.ShowDialog(this); LoadRows(); };
        edit.Click += (_, _) => Ui.Run(this, () => { using var f = new UserEditForm(Ui.RequireSelection(grid)); f.ShowDialog(this); LoadRows(); }); LoadRows();
    }
    private void LoadRows()
    {
        var s = AppStore.Current; s.Refresh(); Ui.Bind(grid, s.Users.Where(x => (archived.Checked || !x.Archived) && $"{x.Name} {x.Email}".Contains(search.Text.Trim(), StringComparison.OrdinalIgnoreCase)).Select(x => new { x.Id, x.Name, x.Email, x.Role, x.Archived }).ToList());
    }
}
