namespace CustomPC;

// Dialog fields reflow independently from a pinned action bar.
public class EditorSheet : UserControl
{
    private readonly TableLayoutPanel layout = new() { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(24, 12, 24, 0) };
    private readonly Panel header = new() { Dock = DockStyle.Fill, Margin = new Padding(0) };
    private readonly FlowLayoutPanel fields = new() { Dock = DockStyle.Fill, WrapContents = true, AutoScroll = true, BackColor = Color.White, Padding = new Padding(20), Margin = new Padding(0) };
    private readonly FlowLayoutPanel actions = new() { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 16, 0, 0), Margin = new Padding(0) };
    private readonly Dictionary<Control, Control> groups = new();
    private readonly Dictionary<Control, Label> hints = new();
    private bool twoColumns;
    private bool compactSpacing;
    public void UseCompactSpacing()
    {
        compactSpacing = true;
        Reflow();
    }
    public void FitOwnerHeightToContent(Form owner)
    {
        Reflow();
        layout.PerformLayout();
        int contentHeight = fields.Controls.Cast<Control>().Where(control => control.Visible)
            .Sum(control => control.Height + control.Margin.Vertical) + fields.Padding.Vertical;
        int requiredHeight = contentHeight + (int)layout.RowStyles[0].Height +
            (int)layout.RowStyles[2].Height + layout.Padding.Vertical;
        Rectangle workArea = Screen.FromControl(owner).WorkingArea;
        int frameHeight = owner.Height - owner.ClientSize.Height;
        owner.ClientSize = new Size(owner.ClientSize.Width,
            Math.Min(requiredHeight, workArea.Height - frameHeight));
        Reflow();
        owner.Location = new Point(owner.Left,
            workArea.Top + Math.Max(0, (workArea.Height - owner.Height) / 2));
    }
    public EditorSheet()
    {
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 112)); layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        layout.Controls.Add(header, 0, 0); layout.Controls.Add(fields, 0, 1); layout.Controls.Add(actions, 0, 2);
        Controls.Add(layout); Dock = DockStyle.Fill; BackColor = StoreTheme.Background; Font = new Font("Roboto", 10F);
        SizeChanged += (_, _) => Reflow(); fields.SizeChanged += (_, _) => Reflow();
        Load += (_, _) => Reflow();
        DpiChangedAfterParent += (_, _) => Reflow();
    }
    public void Bind(Form owner, Label heading, Button back, Button save, string detail, bool wide, params (Label Label, Control Input)[] inputs)
    {
        owner.SuspendLayout(); foreach (Control old in owner.Controls) old.Visible = false; twoColumns = wide;
        heading.Parent = header; heading.Dock = DockStyle.Top; heading.Height = 54; heading.Visible = true; heading.Font = AppFonts.Heading(24); heading.BackColor = BackColor;
        foreach (var pair in inputs) AddField(pair.Label, pair.Input);
        save.Parent = actions; save.Dock = DockStyle.None; save.Size = new Size(190, 44); save.Visible = true;
        back.Parent = actions; back.Dock = DockStyle.None; back.Size = new Size(116, 44); back.Text = "Cancel"; back.Visible = true;
        foreach (Button button in new[] { save, back })
        {
            button.TextChanged += (_, _) => Reflow();
            button.FontChanged += (_, _) => Reflow();
        }
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
        label.FontChanged += (_, _) => Reflow();
        input.Parent = group; input.Dock = DockStyle.Bottom; input.Height = inputHeight; input.Visible = true; input.AccessibleName = label.Text;
        groups[input] = group; fields.Controls.Add(group);
    }
    public void AddFieldHint(Control input, string text)
    {
        if (!groups.TryGetValue(input, out Control? group)) return;
        var hint = new Label { Name = input.Name + "Hint", Text = text, AutoSize = false,
            ForeColor = StoreTheme.Muted, Font = AppFonts.Body(9.5F), UseMnemonic = false };
        hints.Add(input, hint); group.Controls.Add(hint);
        input.AccessibleDescription = text;
        Reflow();
    }
    public void AddOption(Control option, bool first = false)
    {
        Panel group = new() { Height = 54, Width = 240, Margin = new Padding(0, 0, 16, 8) };
        option.Parent = group; option.Dock = DockStyle.Fill; option.Visible = true; groups[option] = group; fields.Controls.Add(group);
        if (first) fields.Controls.SetChildIndex(group, 0);
        Reflow();
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
        try {
        int Px(int value) => (int)(value * DeviceDpi / 96F);
        if (compactSpacing)
        {
            layout.Padding = new Padding(Px(24), Px(16), Px(24), 0);
            fields.Padding = new Padding(Px(20));
            actions.Padding = new Padding(0, Px(12), 0, 0);
            layout.RowStyles[2].Height = Px(64);
        }
        int headingHeight = 0;
        foreach (Label title in header.Controls.OfType<Label>())
        {
            title.Height = Math.Max((int)((compactSpacing ? 48 : 54) * DeviceDpi / 96F), TextRenderer.MeasureText(title.Text, title.Font,
                new Size(Math.Max(1, header.Width), int.MaxValue), TextFormatFlags.WordBreak).Height + 4);
            headingHeight = title.Height;
        }
        layout.RowStyles[0].Height = headingHeight + 12;
        int columns = twoColumns && fields.ClientSize.Width >= 660 * DeviceDpi / 96F ? 2 : 1;
        fields.FlowDirection = columns == 1 ? FlowDirection.TopDown : FlowDirection.LeftToRight;
        fields.WrapContents = columns != 1;
        int width = Math.Max(100, (fields.ClientSize.Width - fields.Padding.Horizontal - SystemInformation.VerticalScrollBarWidth) / columns - 16);
        if (compactSpacing)
            width = Math.Max(100, (fields.ClientSize.Width - fields.Padding.Horizontal -
                (fields.VerticalScroll.Visible ? SystemInformation.VerticalScrollBarWidth : 0)) / columns - (columns > 1 ? Px(12) : 0));
        fields.SuspendLayout();
        foreach (Control field in fields.Controls)
        {
            if (compactSpacing) field.Margin = new Padding(0, 0, 0, Px(14));
            field.Width = width;
            if (field.Controls.Count == 1 && field.Controls[0] is StoreCheckBox checkbox && checkbox.WrapText)
                field.Height = Math.Max((int)(54 * DeviceDpi / 96F), TextRenderer.MeasureText(checkbox.Text, checkbox.Font,
                    new Size(Math.Max(1, width - 32), int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix).Height + (int)(12 * DeviceDpi / 96F));
            if (field.Controls.Count == 1 && field.Controls[0] is Label summary)
                field.Height = Math.Max((int)(54 * DeviceDpi / 96F), TextRenderer.MeasureText(summary.Text, summary.Font,
                    new Size(width, int.MaxValue), TextFormatFlags.WordBreak).Height + 12);
            if (field.GetType() == typeof(Panel) && field.Controls.OfType<Label>().FirstOrDefault() is Label label &&
                field.Controls.Cast<Control>().FirstOrDefault(control => control != label) is Control input)
            {
                label.Height = Math.Max((int)(28 * DeviceDpi / 96F), TextRenderer.MeasureText(label.Text, label.Font,
                    new Size(width, int.MaxValue), TextFormatFlags.WordBreak).Height + 4);
                field.Height = input.Height + label.Height + 12;
                if (compactSpacing)
                {
                    label.Height = Math.Max(Px(22), TextRenderer.MeasureText(label.Text, label.Font,
                        new Size(width, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix).Height + 2);
                    input.Height = Px(input is StoreTextField multiline && multiline.Multiline ? 108 : 42);
                    input.Dock = DockStyle.None;
                    input.SetBounds(0, label.Height + Px(6), width, input.Height);
                    field.Height = input.Bottom;
                }
                if (hints.TryGetValue(input, out Label? hint))
                {
                    int gap = (int)((compactSpacing ? 6 : 8) * DeviceDpi / 96F);
                    int hintHeight = TextRenderer.MeasureText(hint.Text, hint.Font,
                        new Size(width, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix).Height + 4;
                    input.Dock = DockStyle.None;
                    input.SetBounds(0, label.Height + (compactSpacing ? Px(6) : 4), width, input.Height);
                    hint.SetBounds(0, input.Bottom + gap, width, hintHeight);
                    field.Height = hint.Bottom + (compactSpacing ? 0 : gap);
                }
            }
        }
        foreach (Button button in actions.Controls.OfType<Button>())
        {
            button.Width = Math.Max((int)(116 * DeviceDpi / 96F), TextRenderer.MeasureText(button.Text, button.Font).Width + (int)(64 * DeviceDpi / 96F));
            button.Height = (int)(44 * DeviceDpi / 96F);
        }
        fields.ResumeLayout(true);
        fields.AutoScrollMinSize = Size.Empty;
        fields.AutoScrollPosition = new Point(0, -fields.AutoScrollPosition.Y);
        }
        finally { fitting = false; }
    }
}
