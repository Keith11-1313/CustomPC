namespace CustomPC;

public partial class RuleEditForm : Form
{
    private string? ruleId;
    public RuleEditForm()
    {
        InitializeComponent(); if (Ui.IsDesign) return; kind.Items.AddRange(new object[] { "Socket", "Memory", "Power" }); kind.SelectedIndex = 0; enabled.Checked = true;
        save.Click += (_, _) => Ui.Run(this, () => { var r = new CompatibilityRule { Name = ruleName.Text.Trim(), Kind = kind.Text, Enabled = enabled.Checked }; if (ruleId != null) r.Id = ruleId; AppStore.Current.SaveRule(r); DialogResult = DialogResult.OK; });
    }
    public RuleEditForm(string id) : this() { ruleId = id; var r = AppStore.Current.Rules.Single(x => x.Id == id); ruleName.Text = r.Name; kind.SelectedItem = r.Kind; enabled.Checked = r.Enabled; }
}
