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
        }
        base.OnLoad(e);
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
