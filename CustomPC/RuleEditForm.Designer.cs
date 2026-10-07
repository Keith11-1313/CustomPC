#nullable enable
namespace CustomPC;

partial class RuleEditForm
{
    private System.ComponentModel.IContainer? components = null;
    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        SuspendLayout();
        heading = new Label();
        heading.Name = "heading";
        heading.Location = new Point(32, 24);
        heading.Size = new Size(516, 40);
        heading.TabIndex = 0;
        heading.Text = "Compatibility rule";
        heading.UseMnemonic = false;
        heading.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
        Controls.Add(heading);
        subheading = new Label();
        subheading.Name = "subheading";
        subheading.Location = new Point(32, 72);
        subheading.Size = new Size(516, 40);
        subheading.TabIndex = 1;
        subheading.Text = "Enable or disable a stored specification check.";
        subheading.UseMnemonic = false;
        subheading.ForeColor = Color.FromArgb(71, 85, 105);
        Controls.Add(subheading);
        ruleNameLabel = new Label();
        ruleNameLabel.Name = "ruleNameLabel";
        ruleNameLabel.Location = new Point(32, 136);
        ruleNameLabel.Size = new Size(480, 28);
        ruleNameLabel.TabIndex = 2;
        ruleNameLabel.Text = "Rule name";
        ruleNameLabel.UseMnemonic = false;
        Controls.Add(ruleNameLabel);
        ruleName = new TextBox();
        ruleName.Name = "ruleName";
        ruleName.Location = new Point(32, 166);
        ruleName.Size = new Size(480, 32);
        ruleName.TabIndex = 3;
        Controls.Add(ruleName);
        kindLabel = new Label();
        kindLabel.Name = "kindLabel";
        kindLabel.Location = new Point(32, 216);
        kindLabel.Size = new Size(480, 28);
        kindLabel.TabIndex = 4;
        kindLabel.Text = "Check type";
        kindLabel.UseMnemonic = false;
        Controls.Add(kindLabel);
        kind = new ComboBox();
        kind.Name = "kind";
        kind.Location = new Point(32, 246);
        kind.Size = new Size(480, 32);
        kind.TabIndex = 5;
        kind.DropDownStyle = ComboBoxStyle.DropDownList;
        Controls.Add(kind);
        enabled = new CheckBox();
        enabled.Name = "enabled";
        enabled.Location = new Point(32, 316);
        enabled.Size = new Size(480, 32);
        enabled.TabIndex = 6;
        enabled.Text = "Enable this compatibility check";
        Controls.Add(enabled);
        save = new Button();
        save.Name = "save";
        save.Location = new Point(32, 396);
        save.Size = new Size(480, 40);
        save.TabIndex = 7;
        save.Text = "Save rule";
        save.BackColor = Color.FromArgb(15, 98, 84);
        save.ForeColor = Color.White;
        save.FlatStyle = FlatStyle.Flat;
        save.FlatAppearance.BorderSize = 0;
        save.Cursor = Cursors.Hand;
        save.UseMnemonic = false;
        Controls.Add(save);
        AutoScaleDimensions = new SizeF(7F, 17F);
        AutoScaleMode = AutoScaleMode.Font;
        Font = new Font("Segoe UI", 10F);
        ForeColor = Color.FromArgb(15, 23, 42);
        BackColor = Color.FromArgb(248, 250, 252);
        ClientSize = new Size(580, 490);
        MinimumSize = new Size(596, 529);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "CustomPC | Compatibility rule";
        Name = "RuleEditForm";
        ResumeLayout(false);
        PerformLayout();
    }
    private Label heading = null!;
    private Label subheading = null!;
    private Label ruleNameLabel = null!;
    private TextBox ruleName = null!;
    private Label kindLabel = null!;
    private ComboBox kind = null!;
    private CheckBox enabled = null!;
    private Button save = null!;
}
