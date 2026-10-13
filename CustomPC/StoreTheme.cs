namespace CustomPC;

// Central colours keep shopping pages and operational pages consistent.
public static class StoreTheme
{
    public static readonly Color Background = Color.FromArgb(243, 245, 247);
    public static readonly Color Surface = Color.White;
    public static readonly Color Ink = Color.FromArgb(32, 41, 35);
    public static readonly Color Muted = Color.FromArgb(92, 105, 98);
    public static readonly Color Green = Color.FromArgb(84, 177, 55);
    public static readonly Color Border = Color.FromArgb(222, 228, 223);

    public static void Apply(Control control)
    {
        if (control is Form) control.BackColor = Background;
        if (control is Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = Border;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(230, 242, 225);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(212, 233, 204);
            button.Cursor = Cursors.Hand;
        }
        foreach (Control child in control.Controls) Apply(child);
    }
}
