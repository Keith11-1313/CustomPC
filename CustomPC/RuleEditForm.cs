namespace CustomPC;

public partial class RuleEditForm : Form
{
    private string? ruleId;

    public RuleEditForm()
    {
        InitializeComponent();
        if (Ui.IsDesign)
        {
            return;
        }

        kind.Items.AddRange(new object[] { "Socket", "Memory", "Power" });
        kind.SelectedIndex = 0;
        enabled.Checked = true;
        save.Click += SaveClicked;
    }

    public RuleEditForm(string id) : this()
    {
        ruleId = id;
        CompatibilityRule? rule = null;
        foreach (CompatibilityRule savedRule in AppStore.Current.Rules)
        {
            if (savedRule.Id == id)
            {
                rule = savedRule;
                break;
            }
        }

        if (rule == null)
        {
            throw new InvalidOperationException("The selected compatibility rule no longer exists.");
        }

        ruleName.Text = rule.Name;
        kind.SelectedItem = rule.Kind;
        enabled.Checked = rule.Enabled;
    }

    private void SaveClicked(object? sender, EventArgs e)
    {
        try
        {
            CompatibilityRule rule = new CompatibilityRule();
            rule.Name = ruleName.Text.Trim();
            rule.Kind = kind.Text;
            rule.Enabled = enabled.Checked;

            if (ruleId != null)
            {
                rule.Id = ruleId;
            }

            AppStore.Current.SaveRule(rule);
            DialogResult = DialogResult.OK;
        }
        catch (Exception exception)
        {
            Ui.ShowError(this, exception);
        }
    }
}
