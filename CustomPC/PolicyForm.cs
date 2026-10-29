namespace CustomPC;

public partial class PolicyForm : StoreWindow
{
    public PolicyForm() { InitializeComponent(); }
    public PolicyForm(string policy) : this()
    {
        var content = ShopPolicies.Content(policy);
        Text = "CustomPC | " + content.Title;
        heading.Text = content.Title;
        foreach (var section in content.Sections)
        {
            var title = new Label { Text = section.Heading, AutoSize = true, UseMnemonic = false,
                Font = AppFonts.Heading(11), ForeColor = StoreTheme.Ink, Margin = new Padding(0, 0, 0, 8) };
            var body = new Label { Text = section.Body, AutoSize = true, UseMnemonic = false,
                Font = AppFonts.Body(10.5F), ForeColor = StoreTheme.Ink, Margin = new Padding(0, 0, 0, 24) };
            sections.Controls.Add(title); sections.Controls.Add(body);
        }
        sections.SizeChanged += (_, _) => FitText();
        Shown += (_, _) => { FitText(); sections.AutoScrollPosition = Point.Empty; sections.Focus(); };
    }
    private void FitText()
    {
        int width = Math.Max(80, sections.ClientSize.Width - sections.Padding.Horizontal - SystemInformation.VerticalScrollBarWidth - 4);
        foreach (Label label in sections.Controls.OfType<Label>()) label.MaximumSize = new Size(width, 0);
    }
}
