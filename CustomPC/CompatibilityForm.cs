namespace CustomPC;

public partial class CompatibilityForm : Form
{
    public CompatibilityForm()
    {
        InitializeComponent(); if (Ui.IsDesign) return;
        if (!AppStore.Current.IsAdmin) throw new InvalidOperationException("Admin access required.");
        edit.Click += (_, _) => Ui.Run(this, () => { using var f = new RuleEditForm(Ui.RequireSelection(grid)); f.ShowDialog(this); LoadRows(); }); LoadRows();
    }
    private void LoadRows() { AppStore.Current.Refresh(); Ui.Bind(grid, AppStore.Current.Rules.Select(x => new { x.Id, Rule = x.Name, Type = x.Kind, x.Enabled }).ToList()); }
}
