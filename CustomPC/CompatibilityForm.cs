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
}
