namespace CustomPC;

// Shared page structure: title, commands, wrapping filters, and one scrolling content area.
// Existing controls and event handlers are retained when a form adopts this layout.
public partial class WorkspacePage : UserControl
{
    private Label? title;
    private Button? back;
    private readonly List<Panel> fields = new();
    private int options;
    public void SetResults(int count, string unit) => footer.Text = $"{count:N0} {unit}";
    public WorkspacePage()
    {
        InitializeComponent();
        header.SizeChanged += (_, _) => Reflow();
        Load += (_, _) => Reflow();
        DpiChangedAfterParent += (_, _) => Reflow();
    }

    public void Bind(Form owner, Label heading, Button returnButton, string detail, Control body,
        Button? refreshButton, IEnumerable<Button> actions, params (Label Label, Control Input)[] inputs)
    {
        owner.SuspendLayout();
        title = heading; back = returnButton;
        if (refreshButton != null) refreshButton.Visible = false;
        foreach (Control old in owner.Controls) old.Visible = false;
        title.Parent = header; title.Dock = DockStyle.None; title.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        title.Font = AppFonts.Heading(23); title.BackColor = BackColor; title.Visible = true;
        back.Parent = header; back.Dock = DockStyle.None; back.Anchor = AnchorStyles.Top | AnchorStyles.Right; back.Visible = true;
        back.Size = new Size(92, 38);
        back.FontChanged += (_, _) => Reflow();
        foreach (Button button in actions)
        {
            button.Dock = DockStyle.None; button.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            button.Height = 40; button.Width = Math.Max(button.Width, TextRenderer.MeasureText(button.Text, button.Font).Width + 72);
            button.FontChanged += (_, _) => Reflow();
            button.Margin = new Padding(0, 0, 8, 8); commands.Controls.Add(button);
            button.Visible = true;
        }
        foreach (var pair in inputs)
        {
            Panel field = new() { Height = 78, Width = 220, Margin = new Padding(0, 0, 12, 0) };
            pair.Label.Parent = field; pair.Label.Dock = DockStyle.None; pair.Label.SetBounds(0, 0, 220, 25);
            pair.Label.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right; pair.Label.Visible = true;
            pair.Input.Parent = field; pair.Input.Dock = DockStyle.None; pair.Input.SetBounds(0, 29, 220, 40);
            pair.Input.SizeChanged += (_, _) => field.Height = Math.Max(78, pair.Input.Bottom + 12);
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
        // Runtime fonts and monitor DPI can differ from Designer sizes. Include
        // room for the icon, its gap, and both sides of the button.
        int ButtonWidth(Button button, int minimum) => Math.Max((int)(minimum * scale),
            TextRenderer.MeasureText(button.Text, button.Font).Width + (int)(60 * scale));
        back.Size = new Size(ButtonWidth(back, 92), (int)(38 * scale));
        foreach (Button button in commands.Controls.OfType<Button>())
        {
            button.Width = ButtonWidth(button, 140);
            button.Height = (int)(42 * scale);
        }
        back.SetBounds(Math.Max(0, width - back.Width), 4, back.Width, back.Height);
        int commandLeft = back.Left;
        int titleWidth = Math.Max(100, commandLeft - (int)(16 * scale));
        int titleHeight = Math.Max((int)(48 * scale), TextRenderer.MeasureText(title.Text, title.Font,
            new Size(titleWidth, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix).Height + 4);
        title.SetBounds(0, 0, titleWidth, titleHeight);
        layout.RowStyles[0].Height = Math.Max(back.Bottom, title.Bottom) + (int)(12 * scale);
        layout.RowStyles[4].Height = Math.Max((int)(32 * scale), TextRenderer.MeasureText(footer.Text, footer.Font,
            new Size(Math.Max(1, width), int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix).Height + 8);
        int columns = Math.Max(1, Math.Min(fields.Count + options, width / (int)(230 * scale)));
        int fieldWidth = Math.Max(120, (width - columns * 12) / columns);
        foreach (Panel field in fields) field.Width = fieldWidth;
    }

    public void AddFilterOption(Control control)
    {
        control.Dock = DockStyle.None;
        control.Size = new Size(Math.Max(160, TextRenderer.MeasureText(control.Text, control.Font).Width + (int)(56 * DeviceDpi / 96F)), 40);
        control.Margin = new Padding(0, 29, 12, 0);
        control.FontChanged += (_, _) => control.Width = TextRenderer.MeasureText(control.Text, control.Font).Width + (int)(56 * DeviceDpi / 96F);
        filters.Controls.Add(control);
        options++; Reflow();
    }
    public void AddNotice(Control control)
    {
        control.Dock = DockStyle.None; control.Width = 300;
        if (control is Label label) { label.AutoSize = true; label.MaximumSize = new Size(600, 0); }
        commands.Controls.Add(control); control.Visible = true;
    }

    public void AddSummaryAction(Label summary, Button action)
    {
        var row = new Panel { Margin = new Padding(0, 0, 0, 8) };
        summary.Parent = row; summary.AutoSize = false; summary.MaximumSize = Size.Empty;
        summary.Font = AppFonts.Body(10.5F); summary.ForeColor = StoreTheme.Muted;
        summary.TextAlign = ContentAlignment.MiddleRight; summary.Visible = true;
        action.Parent = row; action.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        action.Visible = true;
        bool fitting = false;
        void Fit()
        {
            if (fitting) return;
            fitting = true;
            try
            {
                int gap = (int)(20 * DeviceDpi / 96F);
                row.Width = Math.Max(1, commands.ClientSize.Width - commands.Padding.Horizontal);
                action.Size = new Size(TextRenderer.MeasureText(action.Text, action.Font).Width + (int)(64 * DeviceDpi / 96F), (int)(42 * DeviceDpi / 96F));
                int textWidth = Math.Max(1, row.Width - action.Width - gap);
                int height = Math.Max(action.Height, TextRenderer.MeasureText(summary.Text, summary.Font,
                    new Size(textWidth, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix).Height + 8);
                row.Height = height;
                summary.SetBounds(action.Width + gap, 0, textWidth, height);
                action.Location = Point.Empty;
            }
            finally { fitting = false; }
        }
        commands.Controls.Add(row);
        commands.SizeChanged += (_, _) => Fit();
        summary.TextChanged += (_, _) => Fit();
        action.FontChanged += (_, _) => Fit();
        Load += (_, _) => Fit();
        Fit();
    }
}
