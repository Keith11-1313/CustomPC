using System.Data;

namespace CustomPC;

public partial class CompatibilityForm : Form
{
    public CompatibilityForm()
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

        edit.Click += EditClicked;
        LoadRows();
    }

    private void EditClicked(object? sender, EventArgs e)
    {
        try
        {
            AppStore.Current.Refresh();
            ValidateAccess(AppStore.Current);
            string selectedRuleId = Ui.RequireSelection(grid);
            using RuleEditForm ruleForm = new RuleEditForm(selectedRuleId);
            ruleForm.ShowDialog(this);
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

        DataTable table = new DataTable();
        table.Columns.Add("Id");
        table.Columns.Add("Rule");
        table.Columns.Add("Type");
        table.Columns.Add("Enabled", typeof(bool));

        foreach (CompatibilityRule rule in store.Rules)
        {
            table.Rows.Add(rule.Id, rule.Name, rule.Kind, rule.Enabled);
        }

        Ui.Bind(grid, table);
    }

    private void ValidateAccess(AppStore store)
    {
        edit.Enabled = store.IsAdmin;

        if (!store.IsAdmin)
        {
            grid.DataSource = null;
            throw new InvalidOperationException("Your account no longer has admin access. Close this screen and sign in with an administrator account.");
        }
    }
}
