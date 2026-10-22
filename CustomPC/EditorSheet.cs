namespace CustomPC;

// Dialog fields reflow independently from a pinned action bar.
public class EditorSheet : UserControl
{
    private readonly TableLayoutPanel layout = new() { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(24, 12, 24, 0) };
    private readonly Panel header = new() { Dock = DockStyle.Fill, Margin = new Padding(0) };
    private readonly Label description = new() { Dock = DockStyle.Bottom, Height = 44, ForeColor = StoreTheme.Muted };
    private readonly FlowLayoutPanel fields = new() { Dock = DockStyle.Fill, WrapContents = true, AutoScroll = true, BackColor = Color.White, Padding = new Padding(20), Margin = new Padding(0) };
    private readonly FlowLayoutPanel actions = new() { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 16, 0, 0), Margin = new Padding(0) };
    private readonly Dictionary<Control, Control> groups = new();
    private bool twoColumns;
    public EditorSheet()
    {
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 112)); layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        header.Controls.Add(description); layout.Controls.Add(header, 0, 0); layout.Controls.Add(fields, 0, 1); layout.Controls.Add(actions, 0, 2);
        Controls.Add(layout); Dock = DockStyle.Fill; BackColor = StoreTheme.Background; Font = new Font("Roboto", 10F);
        SizeChanged += (_, _) => Reflow(); fields.SizeChanged += (_, _) => Reflow();
    }
    public void Bind(Form owner, Label heading, Button back, Button save, string detail, bool wide, params (Label Label, Control Input)[] inputs)
    {
        owner.SuspendLayout(); foreach (Control old in owner.Controls) old.Visible = false; twoColumns = wide;
        heading.Parent = header; heading.Dock = DockStyle.Top; heading.Height = 54; heading.Visible = true; heading.Font = AppFonts.Heading(24); heading.BackColor = BackColor;
        description.Text = detail;
        foreach (var pair in inputs) AddField(pair.Label, pair.Input);
        save.Parent = actions; save.Dock = DockStyle.None; save.Size = new Size(190, 44); save.Visible = true;
        back.Parent = actions; back.Dock = DockStyle.None; back.Size = new Size(116, 44); back.Text = "Cancel"; back.Visible = true;
        owner.Controls.Add(this); BringToFront(); owner.AutoScroll = false; owner.AutoScrollMinSize = Size.Empty;
        owner.MinimumSize = new Size(440, 440); owner.ClientSize = new Size(wide ? 860 : 580, wide ? 730 : 680);
        owner.MinimizeBox = false; owner.MaximizeBox = false; owner.ShowInTaskbar = false;
        owner.StartPosition = FormStartPosition.CenterParent;
        owner.ResumeLayout(true); Reflow();
    }
    private void AddField(Label label, Control input)
    {
        int inputHeight = input is StoreTextField text && text.Multiline ? 108 : 42;
        Panel group = new() { Height = inputHeight + 40, Width = 240, Margin = new Padding(0, 0, 16, 12) };
        label.Parent = group; label.Dock = DockStyle.Top; label.Height = 28; label.Visible = true; label.BackColor = Color.White;
        input.Parent = group; input.Dock = DockStyle.Bottom; input.Height = inputHeight; input.Visible = true; input.AccessibleName = label.Text;
        groups[input] = group; fields.Controls.Add(group);
    }
    public void AddOption(Control option)
    {
        Panel group = new() { Height = 54, Width = 240, Margin = new Padding(0, 0, 16, 8) };
        option.Parent = group; option.Dock = DockStyle.Fill; option.Visible = true; groups[option] = group; fields.Controls.Add(group); Reflow();
    }
    public void AddPhoto(PictureBox photo, Button choose, Button remove, Label status)
    {
        var group = new TableLayoutPanel { Width = 300, Height = 240, ColumnCount = 2, RowCount = 3, Margin = new Padding(0, 0, 16, 16) };
        group.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50)); group.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        group.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); group.RowStyles.Add(new RowStyle(SizeType.Absolute, 42)); group.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        group.Controls.Add(photo, 0, 0); group.SetColumnSpan(photo, 2); photo.Dock = DockStyle.Fill; photo.Visible = true;
        group.Controls.Add(choose, 0, 1); group.Controls.Add(remove, 1, 1); choose.Dock = remove.Dock = DockStyle.Fill; choose.Visible = remove.Visible = true;
        group.Controls.Add(status, 0, 2); group.SetColumnSpan(status, 2); status.Dock = DockStyle.Fill; status.Visible = true;
        fields.Controls.Add(group); Reflow();
    }
    public void SetFieldVisible(Control input, bool visible)
    {
        if (!groups.TryGetValue(input, out Control? group)) return;
        group.Visible = visible; input.Visible = visible; input.TabStop = visible; Reflow();
    }
    private bool fitting;
    private void Reflow()
    {
        if (fitting) return; fitting = true;
        try { int columns = twoColumns && fields.ClientSize.Width >= 660 * DeviceDpi / 96F ? 2 : 1;
        int width = Math.Max(100, (fields.ClientSize.Width - fields.Padding.Horizontal - SystemInformation.VerticalScrollBarWidth) / columns - 16);
        fields.SuspendLayout(); foreach (Control field in fields.Controls) field.Width = width; fields.ResumeLayout(true); }
        finally { fitting = false; }
    }
}
