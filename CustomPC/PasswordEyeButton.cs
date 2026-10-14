namespace CustomPC;

// A normal button keeps mouse clicks, Tab, Space and accessibility support.
internal class PasswordEyeButton : Button
{
    [System.ComponentModel.DefaultValue(false)]
    public bool PasswordVisible { get; set; }
    public PasswordEyeButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        BackColor = Color.White;
        Cursor = Cursors.Hand;
        AccessibleRole = AccessibleRole.PushButton;
        Size = new Size(34, 32);
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(BackColor);
        int size = 20;
        IconAssets.Paint(e.Graphics, PasswordVisible ? "eye-off" : "eye",
            new Rectangle((Width - size) / 2, (Height - size) / 2, size, size), StoreTheme.Muted);
        if (Focused) ControlPaint.DrawFocusRectangle(e.Graphics, Rectangle.Inflate(ClientRectangle, -3, -3));
    }
}
