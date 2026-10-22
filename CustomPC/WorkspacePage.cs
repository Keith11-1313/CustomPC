namespace CustomPC;

// Shared page structure: title, commands, wrapping filters, and one scrolling content area.
// Existing controls and event handlers are retained when a form adopts this layout.
public partial class WorkspacePage : UserControl
{
    private Label? title;
    private Button? back;
    private Button? refresh;
    private readonly List<Panel> fields = new();
    private int options;
    public void SetResults(int count, string unit) => footer.Text = $"{count:N0} {unit}";
    public WorkspacePage() { InitializeComponent(); header.SizeChanged += (_, _) => Reflow(); }

    public void Bind(Form owner, Label heading, Button returnButton, string detail, Control body,
        Button? refreshButton, IEnumerable<Button> actions, params (Label Label, Control Input)[] inputs)
    {
        owner.SuspendLayout();
        title = heading; back = returnButton; refresh = refreshButton;
        foreach (Control old in owner.Controls) old.Visible = false;
        title.Parent = header; title.Dock = DockStyle.None; title.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        title.Font = AppFonts.Heading(23); title.BackColor = BackColor; title.Visible = true;
        back.Parent = header; back.Dock = DockStyle.None; back.Anchor = AnchorStyles.Top | AnchorStyles.Right; back.Visible = true;
        back.Size = new Size(92, 38);
        description.Text = detail;
        if (refresh != null)
        {
            refresh.Parent = header; refresh.Dock = DockStyle.None; refresh.Size = new Size(120, 38);
            refresh.Anchor = AnchorStyles.Top | AnchorStyles.Right; refresh.Visible = true;
        }
        foreach (Button button in actions)
        {
            button.Dock = DockStyle.None; button.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            button.Height = 40; button.Width = Math.Clamp(button.Width, 140, 220);
            button.Margin = new Padding(0, 0, 8, 8); commands.Controls.Add(button);
            button.Visible = true;
        }
        foreach (var pair in inputs)
        {
            Panel field = new() { Height = 78, Width = 220, Margin = new Padding(0, 0, 12, 0) };
            pair.Label.Parent = field; pair.Label.Dock = DockStyle.None; pair.Label.SetBounds(0, 0, 220, 25);
            pair.Label.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right; pair.Label.Visible = true;
            pair.Input.Parent = field; pair.Input.Dock = DockStyle.None; pair.Input.SetBounds(0, 29, 220, 40);
            pair.Input.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right; pair.Input.Visible = true;
            pair.Input.AccessibleName = pair.Label.Text;
            fields.Add(field); filters.Controls.Add(field);
        }
        body.Parent = content; body.Dock = DockStyle.Fill; body.Margin = new Padding(0); body.Visible = true;
        if (body is DataGridView grid)
        {
            void CountRows() => footer.Text = $"{grid.Rows.Count:N0} records   •   Select a row to use its available actions";
            grid.DataBindingComplete += (_, _) => CountRows(); CountRows();
        }
        else footer.Text = "";
        owner.Controls.Add(this); BringToFront();
        owner.AutoScroll = false; owner.MinimumSize = new Size(640, 480);
        SizeChanged += (_, _) => Reflow(); Reflow();
        owner.ResumeLayout(true);
    }

    private void Reflow()
    {
        if (title == null || back == null) return;
        float scale = DeviceDpi / 96F;
        int gap = (int)(24 * scale);
        layout.Padding = new Padding(ClientSize.Width < 700 * scale ? 16 : gap, 12, ClientSize.Width < 700 * scale ? 16 : gap, 8);
        int width = header.ClientSize.Width;
        back.SetBounds(Math.Max(0, width - back.Width), 4, back.Width, back.Height);
        if (refresh != null) refresh.SetBounds(Math.Max(0, back.Left - refresh.Width - 8), 4, refresh.Width, refresh.Height);
        int commandLeft = refresh?.Left ?? back.Left;
        title.SetBounds(0, 0, Math.Max(100, commandLeft - 16), 48);
        description.SetBounds(0, 49, width, 30);
        int columns = Math.Max(1, Math.Min(fields.Count + options, width / (int)(230 * scale)));
        int fieldWidth = Math.Max(120, (width - columns * 12) / columns);
        foreach (Panel field in fields) field.Width = fieldWidth;
    }

    public void AddFilterOption(Control control)
    {
        control.Dock = DockStyle.None; control.Size = new Size(160, 40); control.Margin = new Padding(0, 29, 12, 0);
        filters.Controls.Add(control);
        options++; Reflow();
    }
    public void AddNotice(Control control)
    {
        control.Dock = DockStyle.None; control.Width = 300;
        if (control is Label label) { label.AutoSize = true; label.MaximumSize = new Size(600, 0); }
        commands.Controls.Add(control); control.Visible = true;
    }
}
