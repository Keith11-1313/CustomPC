namespace CustomPC;

// All application forms share this small display safeguard. Their layouts still live in Designer files.
public class StoreWindow : Form
{
    public StoreWindow()
    {
        if (Ui.IsDesign) return;
        Icon = BrandLogo.WindowIcon;
        AppFonts.Apply(this);
    }

    protected override void OnControlAdded(ControlEventArgs e)
    {
        base.OnControlAdded(e);
        if (!Ui.IsDesign && e.Control != null) AppFonts.Apply(e.Control);
    }

    protected override void OnFontChanged(EventArgs e)
    {
        base.OnFontChanged(e);
        if (!Ui.IsDesign) AppFonts.Apply(this);
    }

    protected override void OnLoad(EventArgs e)
    {
        if (!Ui.IsDesign)
        {
            StoreTheme.Apply(this);
            FitToWorkingArea(Screen.FromControl(this).WorkingArea);
            FitHeader();
        }
        base.OnLoad(e);
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        if (!Ui.IsDesign) FitHeader();
    }

    private void FitHeader()
    {
        foreach (DataGridView grid in Controls.OfType<DataGridView>().Where(table => table.Name == "grid"))
        {
            // AutoScroll's virtual width must not stretch a table beyond the visible page.
            grid.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            grid.Width = Math.Max(280, ClientSize.Width - grid.Left - 32);
            FlowLayoutPanel? actions = Controls.OfType<FlowLayoutPanel>().FirstOrDefault(row => row.Name == "actionRow");
            grid.Height = Math.Max(100, ClientSize.Height - grid.Top - (actions?.Height ?? 16) - 16);
        }
        // Some dialogs grew after their header was designed. Keep Back inside the surface.
        foreach (Panel header in Controls.OfType<Panel>().Where(panel => panel.Name == "headerSurface"))
        {
            header.Width = ClientSize.Width;
            Button? back = header.Controls.OfType<Button>().FirstOrDefault(button => button.Name == "back");
            if (back == null) continue;
            back.Left = Math.Max(0, header.ClientSize.Width - back.Width - 24);
            Label? heading = header.Controls.OfType<Label>().FirstOrDefault(label => label.Name == "heading");
            if (heading != null) heading.Width = Math.Max(1, back.Left - heading.Left - 16);
        }
    }

    public void FitToWorkingArea(Rectangle workingArea)
    {
        if (Width <= workingArea.Width && Height <= workingArea.Height) return;
        Size content = ClientSize;
        // Preserve the original content size and provide scrollbars instead of cutting off buttons.
        AutoScroll = true;
        AutoScrollMinSize = content;
        MinimumSize = new Size(Math.Min(MinimumSize.Width, workingArea.Width), Math.Min(MinimumSize.Height, workingArea.Height));
        Size = new Size(Math.Min(Width, workingArea.Width), Math.Min(Height, workingArea.Height));
    }
}
