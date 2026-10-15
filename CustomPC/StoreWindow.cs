namespace CustomPC;

// All application forms share this small display safeguard. Their layouts still live in Designer files.
public class StoreWindow : Form
{
    private bool fittingHeader;
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
            foreach (Panel header in Controls.Find("headerSurface", true).OfType<Panel>())
                header.SizeChanged += (_, _) => FitHeader();
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
        if (fittingHeader) return;
        fittingHeader = true;
        try
        {
            foreach (DataGridView grid in Controls.OfType<DataGridView>().Where(table => table.Name == "grid"))
            {
                // AutoScroll's virtual width must not stretch a table beyond the visible page.
                grid.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                grid.Width = Math.Max(280, ClientSize.Width - grid.Left - 32);
                FlowLayoutPanel? actions = Controls.OfType<FlowLayoutPanel>().FirstOrDefault(row => row.Name == "actionRow");
                grid.Height = Math.Max(100, ClientSize.Height - grid.Top - (actions?.Height ?? 16) - 16);
            }
            // Also fit headers nested inside a page's layout panel.
            foreach (Panel header in Controls.Find("headerSurface", true).OfType<Panel>())
            {
                if (header.Parent == this) header.Width = ClientSize.Width;
                Button? back = header.Controls.OfType<Button>().FirstOrDefault(button => button.Name == "back");
                if (back == null) continue;
                back.Left = Math.Max(0, header.ClientSize.Width - back.Width - 24);
                Label? heading = header.Controls.OfType<Label>().FirstOrDefault(label => label.Name == "heading");
                if (heading != null) heading.Width = Math.Max(1, back.Left - heading.Left - 16);
            }
        }
        finally { fittingHeader = false; }
    }

    public void FitToWorkingArea(Rectangle workingArea)
    {
        if (!TopLevel) return;
        if (this is DashboardForm or BuilderForm or CatalogForm)
        {
            AutoScroll = false;
            AutoScrollMinSize = Size.Empty;
            AutoScrollPosition = Point.Empty;
        }
        // Windows owns maximized bounds, including the invisible resize borders.
        if (WindowState == FormWindowState.Maximized) return;
        if (Width <= workingArea.Width && Height <= workingArea.Height)
        {
            Location = new Point(Math.Clamp(Left, workingArea.Left, Math.Max(workingArea.Left, workingArea.Right - Width)),
                Math.Clamp(Top, workingArea.Top, Math.Max(workingArea.Top, workingArea.Bottom - Height)));
            return;
        }
        // Resize to usable monitor space, without preserving an oversized virtual desktop.
        if (this is not DashboardForm && Controls.OfType<TableLayoutPanel>().All(panel => panel.Dock != DockStyle.Fill))
            AutoScroll = true;
        MinimumSize = new Size(Math.Min(MinimumSize.Width, workingArea.Width), Math.Min(MinimumSize.Height, workingArea.Height));
        Size = new Size(Math.Min(Width, workingArea.Width), Math.Min(Height, workingArea.Height));
        Location = new Point(Math.Clamp(Left, workingArea.Left, Math.Max(workingArea.Left, workingArea.Right - Width)),
            Math.Clamp(Top, workingArea.Top, Math.Max(workingArea.Top, workingArea.Bottom - Height)));
    }
}
